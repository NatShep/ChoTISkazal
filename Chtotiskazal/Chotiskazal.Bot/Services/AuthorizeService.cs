using System;
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

        public async Task<User> AuthorizeAsync(long telegramId,string name)
        {
            var user = await LoginUserAsync(telegramId);
            if(user==null)
                 user = await CreateUserAsync(telegramId,name);
            if(user==null)
                throw  new Exception("I can't add user!");
            return user;
        }
        private async Task<User> CreateUser(string name, string login, string password, string email)
        {
            var user = new User(name, login, password, email);
            try
            {
                user.UserId=await _userService.AddUserAsync(user);
            }
            catch
            {
                return null;
            }
            return user;
        }
        
        private async Task<User> CreateUserAsync(long telegramID, string name)
        {
            var user = new User(telegramID,name);
            
            try
            {
                user.UserId= await _userService.AddUserAsync(user);
            }
            catch
            {
                return null;
            }
            return user;
        }
        
        private async Task<User> LoginUserAsync(long telegramId)=>
            await  _userService.GetUserByTelegramIdAsync(telegramId);

        public async Task<User> LoginUser(string login, string password)=>
            await _userService.GetUserByLoginOrNullAsync(login,password);
    }
}