using System;
using System.Security.Cryptography;
using Chotiskazal.DAL;
using Chotiskazal.Dal.Services;

namespace Chotiskazal.Api.Services
{
    public class AuthorizeService
    {
        private UserService _userService;

        public AuthorizeService(UserService userService)=> _userService = userService;

        public User Authorize(long telegramId,string name)
        {
            var user = LoginUser(telegramId);
            if(user==null)
                 user = CreateUser(telegramId,name);
            if(user==null)
                throw  new Exception("I can't add user!");
            return user;
        }
        private User CreateUser(string name, string login, string password, string email)
        {
            var user = new User(name, login, password, email);
            try
            {
                user.UserId=_userService.AddUser(user);
            }
            catch
            {
                return null;
            }
            return user;
        }
        
        private User CreateUser(long telegramID, string name)
        {
            var user = new User(telegramID,name);
            
            try
            {
                user.UserId= _userService.AddUser(user);
            }
            catch
            {
                return null;
            }
            return user;
        }
        
        private User LoginUser(long telegramId)
        {
            var user = _userService.GetUserByTelegramId(telegramId);
            return user;
        }


        public User LoginUser(string login, string password)
        {
            var user = _userService.GetUserByLoginOrNull(login,password);
            return user;
        }
    }
}