using System.Threading.Tasks;

namespace Sabio.Services
{
    public interface IUserService
    {
        int Create(object userModel);

        Task<bool> LogInAsync(string email, string password);

        Task<bool> LogInTest(string email, string password, int id, string[] roles = null);
    }
}