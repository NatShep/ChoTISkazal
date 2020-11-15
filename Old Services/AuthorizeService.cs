using System.Security.Cryptography;
using Chotiskazal.DAL;
using Chotiskazal.Dal.Services;

namespace Chotiskazal.Api.OldServices
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
                _userService.AddUser(user);
            }
            catch
            {
                return null;
            }
            return user;
        }

        public User LoginUser(string login, string password)
        {
            var user = _userService.GetUserByLoginOrNull(login);
            if (user?.Password == password)
            {
                return user;
            }
            else
                return null;
        }
    }
}