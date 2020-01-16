using Sabio.Models.Domain;
using Sabio.Models.Requests;

namespace Sabio.Services.Services
{
    public interface ILoginService
    {
        void Delete(int id);
        string HashPassword(string password);
        int Register(UserInfo model);
        UserInfo selectByUsername(LoginRequest model);
        int updatePassword(LoginRequest model);
    }
}