using System.Collections.Generic;
using System.Threading.Tasks;
using Sabio.Models.Domain;
using Sabio.Models.Requests;

namespace Sabio.Services.Services
{
    public interface IScraper
    {
        Task confirm(UserInfo model, string token);
        void DeleteAll();
        void DeleteById(int id);
        List<EntryModel> Get();
        List<EntryModel> Insert(List<EntryModel> models);
        List<EntryModel> ScrapeData();
        void update(EntryModel model);
    }
}