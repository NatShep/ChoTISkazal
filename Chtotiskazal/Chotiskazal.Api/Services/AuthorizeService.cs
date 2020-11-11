using System.Security.Cryptography;
using Chotiskazal.DAL;
using Chotiskazal.Dal.Services;

namespace Chotiskazal.Api.Services
{
    public class AuthorizeService
    {
        private UserService _userService;

        public AuthorizeService(UserService userService)=> _userService = userService;

        public User CreateUser(string name, string login, string password, string email)
        {
            var user = new User(name, login, password, email);
            try
            {
               int id= _userService.AddUser(user);
               user.UserId = id;
            }
            catch
            {
                return null;
            }
            return user;
        }

        public User LoginUser(string login, string password)
        {
            var user = _userService.GetUserByLoginOrNull(login,password);
            return user;
        }
    }
}