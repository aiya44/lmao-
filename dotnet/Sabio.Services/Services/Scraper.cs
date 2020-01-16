using HtmlAgilityPack;
using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models.Domain;
using Sabio.Models.Requests;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sabio.Services.Services
{
    public class Scraper : IScraper
    {
        private IDataProvider _dataProvider = null;

        public Scraper(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }



        public List<EntryModel> ScrapeData()
        {
            var url = "https://orangecounty.craigslist.org/search/apa?hasPic=1&postedToday=1&availabilityMode=0&sale_date=all+dates";
            List<EntryModel> Data = new List<EntryModel>();
            var web = new HtmlWeb();
            var doc = web.Load(url);

            var Articles = doc.DocumentNode.SelectNodes("//*[@class = 'result-row']");

            foreach (var article in Articles)
            {

                EntryModel model = new EntryModel();
                model.PostLink = article.SelectSingleNode("a").Attributes["href"].Value;
                model.Price = article.SelectSingleNode("a/span").InnerText;
                model.Description = article.SelectSingleNode("p/a").InnerText;



                Data.Add(model);


            }

            return Data;

        }


        private async Task SendEmail(SendGridMessage msg)
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            await client.SendEmailAsync(msg);
        }



        public async Task confirm(UserInfo model, string token)
        {
            string directory = Directory.GetCurrentDirectory();
            var path = Path.Combine(directory, "Template\\ConfirmEmail.html");
            var htmlContent = System.IO.File.ReadAllText(path);
            htmlContent = htmlContent.Replace("{&FirstName}", model.FirstName);
            htmlContent = htmlContent.Replace("{&LastName}", model.LastName);
            htmlContent = htmlContent.Replace("{&Email}", model.Email);
            htmlContent = htmlContent.Replace(" { &Url}", "http://localhost:3000/login");

            var msg = new SendGridMessage()
            {
                From = new EmailAddress("Amber@Amber.com"),
                Subject = "Thank you for registering",
                HtmlContent = htmlContent,
            };
            msg.AddTo(model.Email);
            await SendEmail(msg);
        }



        public List<EntryModel> Insert(List<EntryModel> models)
        {
            List<EntryModel> modelIds = new List<EntryModel>();
            foreach (var model in models)
            {
                EntryModel newmodel = new EntryModel();
                int orgId = 0;
                _dataProvider.ExecuteNonQuery(
                    "dbo.craigslist_INSERT",
                    inputParamMapper: delegate (System.Data.SqlClient.SqlParameterCollection parameterCollection)
                    {
                        System.Data.SqlClient.SqlParameter parameter = new System.Data.SqlClient.SqlParameter();
                        parameter.ParameterName = "@craigslistid";
                        parameter.SqlDbType = System.Data.SqlDbType.Int;
                        parameter.Direction = System.Data.ParameterDirection.Output;
                        parameterCollection.Add(parameter);

                        parameterCollection.AddWithValue("@Description", model.Description);
                        parameterCollection.AddWithValue("@Price", model.Price);
                        parameterCollection.AddWithValue("@PostLink", model.PostLink);
                        parameterCollection.AddWithValue("@image", "null");
                        parameterCollection.AddWithValue("@imageURL", "null");


                    },
                    returnParameters: delegate (System.Data.SqlClient.SqlParameterCollection parameterCollection)
                    {
                        Int32.TryParse(parameterCollection["@craigslistid"].Value.ToString(), out orgId);
                    });

                newmodel.craigslistId = orgId;
                newmodel.Description = model.Description;
                newmodel.Price = model.Price;
                newmodel.PostLink = model.PostLink;
                newmodel.Image = model.Image;

                modelIds.Add(newmodel);

            }
            return modelIds;

        }



        public void DeleteById(int id)
        {
            _dataProvider.ExecuteNonQuery(
                "dbo.craigslist_Delete",
                inputParamMapper: delegate (SqlParameterCollection parameterCollection)
                {
                    parameterCollection.AddWithValue("@craigslistid", id);
                }

            );
        }


        public void update(EntryModel model)
        {



            _dataProvider.ExecuteNonQuery("dbo.craigslist_update",
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@craigslistid", model.craigslistId);
                    paramCol.AddWithValue("@Description", model.Description);
                    paramCol.AddWithValue("@Price", model.Price);
                    paramCol.AddWithValue("@PostLink", model.PostLink);



                });

        }


        public void DeleteAll()
        {
            _dataProvider.ExecuteNonQuery(
                "dbo.craigslist_deleteAll",
                inputParamMapper: null

            );
        }


        public List<EntryModel> Get()
        {
            List<EntryModel> result = null;
            _dataProvider.ExecuteCmd("dbo.craigslist_selectall",
                inputParamMapper: null,
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    EntryModel model = MapCraig(reader);
                    if (result == null)
                    {
                        result = new List<EntryModel>();
                    }
                    result.Add(model);
                }
                );
            return result;
        }





        private static EntryModel MapCraig(IDataReader reader)
        {
            EntryModel model = new EntryModel();
            int index = 0;
            model.craigslistId = reader.GetSafeInt32(index++);
            model.Description = reader.GetSafeString(index++);
            model.Price = reader.GetSafeString(index++);
            model.PostLink = reader.GetSafeString(index++);
            model.Image = reader.GetSafeString(index++);

            model.ImageUrl = reader.GetSafeString(index++);


            return model;
        }







        ////////-----

    }
}
