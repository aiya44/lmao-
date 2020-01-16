using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models.Domain;
using Sabio.Models.Requests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Sabio.Services.Services
{
    public class LoginService : ILoginService
    {
        private IDataProvider _dataProvider = null;

        public LoginService(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }


        public UserInfo selectByUsername(LoginRequest model)
        {
            string passwordHash = "";
            UserInfo info = null;
            _dataProvider.ExecuteCmd(
                "dbo.USERS_SelectByUsername",
                inputParamMapper: delegate (SqlParameterCollection parameterCollection)
                {
                    parameterCollection.AddWithValue("@Username", model.Username);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {


                    passwordHash = reader.GetSafeString(6);
                    bool isValid = BCrypt.BCryptHelper.CheckPassword(model.Password, passwordHash);

                    if (isValid)
                    {

                        info = MapUserInfo(reader);


                    }



                }
            );
            return info;
        }



        public int updatePassword(LoginRequest model)
        {

            string hashedPassword = HashPassword(model.NewPassword);

            _dataProvider.ExecuteNonQuery("dbo.USERS_UPDATE_PASSWORD",
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@UserId", model.UserId);

                    paramCol.AddWithValue("@Password", hashedPassword);

                });
            return model.UserId; 


        }




        public void Delete(int id)
        {
            _dataProvider.ExecuteNonQuery(
                "dbo.USERS_deleteById",
                inputParamMapper: delegate (SqlParameterCollection parameterCollection)
                {
                    parameterCollection.AddWithValue("@UserId", id);
                }

            );
        }

        public int Register(UserInfo model)
        {
            int id = 0;
            string hashedPassword = HashPassword(model.Password);

            _dataProvider.ExecuteNonQuery("dbo.USERS_INSERT",
                inputParamMapper: delegate (SqlParameterCollection paramCol)
                {
                    paramCol.AddWithValue("@FirstName", model.FirstName);
                    paramCol.AddWithValue("@LastName", model.LastName);
                    paramCol.AddWithValue("@Username", model.Username);
                    paramCol.AddWithValue("@PhoneNumber", model.PhoneNumber);
                    paramCol.AddWithValue("@Email", model.Email);
                    paramCol.AddWithValue("@Password", hashedPassword);
                    paramCol.AddWithValue("@Birthday", model.Birthday);
                    paramCol.AddWithValue("@UserGenderId", model.UserGenderId);
                    paramCol.AddWithValue("@IsConfirmed", model.IsConfirmed);
                    paramCol.AddWithValue("@ShowUserProfile", model.ShowUserProfile);



                    SqlParameter paramId = new SqlParameter("@UserId", System.Data.SqlDbType.Int);
                    paramId.Direction = System.Data.ParameterDirection.Output;
                    paramCol.Add(paramId);
                },
                returnParameters: delegate (SqlParameterCollection param)
                {
                    Int32.TryParse(param["@UserId"].Value.ToString(), out id);
                }
            );

            return id;
        }



        public string HashPassword(string password)
        {
            string salt = BCrypt.BCryptHelper.GenerateSalt();
            string hashedPassword = BCrypt.BCryptHelper.HashPassword(password, salt);
            return hashedPassword;
        }


        private static UserInfo MapUserInfo(IDataReader reader)
        {
            UserInfo model = new UserInfo();
            int index = 0;
            model.UserId = reader.GetSafeInt32(index++);
            model.FirstName = reader.GetSafeString(index++);
            model.LastName = reader.GetSafeString(index++);
            model.Username = reader.GetSafeString(index++);
            model.PhoneNumber = reader.GetSafeString(index++);
            model.Email = reader.GetSafeString(index++);
            model.Password = reader.GetSafeString(index++);
            model.Birthday = reader.GetSafeDateTime(index++);
            model.UserGenderId = reader.GetSafeInt32(index++);
            model.IsConfirmed = reader.GetSafeInt32(index++);
            model.DateCreated = reader.GetSafeDateTime(index++);
            model.DateModified = reader.GetSafeDateTime(index++);
            model.ShowUserProfile = reader.GetSafeInt32(index++);


            return model;
        }







    }
}
