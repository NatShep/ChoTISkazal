using System.Security.Cryptography;
using System.Threading.Tasks;
using Chotiskazal.DAL;
using Chotiskazal.Dal.Services;

namespace Chotiskazal.Api.Services
{
    public class AuthorizeService
    {
        private UserService _userService;

        public AuthorizeService(UserService userService)=> _userService = userService;

        public async Task<User> CreateUser(string name, string login, string password, string email)
        {
            var user = new User(name, login, password, email);
            try
            {
               int id= await _userService.AddUserAsync(user);
               user.UserId = id;
            }
            catch
            {
                return null;
            }
            return user;
        }

        public async Task<User> LoginUser(string login, string password)=>
            await _userService.GetUserByLoginOrNullAsync(login,password);
         
    }
}