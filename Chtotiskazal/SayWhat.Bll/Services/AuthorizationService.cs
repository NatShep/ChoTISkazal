using System;
using System.Threading.Tasks;
using SayWhat.MongoDAL.Users;

namespace SayWhat.Bll.Services
{
    public class AuthorizationService
    {
        private readonly UserService _userService;

        public AuthorizationService(UserService userService)=> _userService = userService;

        public async Task<User> AuthorizeAsync(TelegramUserInfo telegramUserInfo)
        {
            var user = await LoginUserAsync(telegramUserInfo.TelegramId) ?? await CreateUserAsync(telegramUserInfo);
            if(user==null)
                throw  new Exception("I can't add user!");
            return user;
        }
        
        private async Task<User> CreateUserAsync(TelegramUserInfo info)
        {
            try
            {
                var user = new User(info.TelegramId, info.FirstName, info.LastName, info.UserNick);
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