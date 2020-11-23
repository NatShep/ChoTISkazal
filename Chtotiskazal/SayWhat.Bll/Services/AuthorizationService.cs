using System;
using System.Threading.Tasks;
using SayWhat.MongoDAL.Users;

namespace SayWhat.Bll.Services
{
    public class AuthorizationService
    {
        private readonly UserService _userService;

        public AuthorizationService(UserService userService)=> _userService = userService;

        public async Task<User> AuthorizeAsync(long telegramId,string name)
        {
            var user = await LoginUserAsync(telegramId) ?? await CreateUserAsync(telegramId,name);
            if(user==null)
                throw  new Exception("I can't add user!");
            return user;
        }
        
        private async Task<User> CreateUserAsync(long telegramID, string name)
        {
            try
            {
                var user = new User(telegramID, name);
                await _userService.AddUserAsync(user);
                return user;
            }
            catch
            {
                return null;
            }
        }
        
        private async Task<User> LoginUserAsync(long telegramId)=>
            await  _userService.GetUserByTelegramIdAsync(telegramId);
    }
}