using System;
using System.Threading.Tasks;
using SayWhat.MongoDAL.Users;

namespace SayWhat.Bll.Services
{
    public class UserService
    {
        private readonly UsersRepo _repository;
        public UserService(UsersRepo repository) => _repository = repository;

        public Task UpdateCounters(User user) =>
            _repository.UpdateCounters(user);
        
        public async Task<User> GetUserOrNull(TelegramUserInfo telegramUserInfo) 
            => await _repository.GetOrDefaultByTelegramIdOrNull(telegramUserInfo.TelegramId);

        public async Task<User> GetOrAddUser(TelegramUserInfo telegramUserInfo)
        {
            var user = await GetUserOrNull(telegramUserInfo);
            // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
            user = user ?? await AddUser(telegramUserInfo);
            if (user == null)
            {
                throw new Exception("I can't add user!");
            }
            return user;
        }
        
        public async Task<User> AddUser(TelegramUserInfo info)
        {
            try
            {
                var user = new User(info.TelegramId, info.FirstName, info.LastName, info.UserNick);
                await _repository.Add(user);
                Botlog.WriteInfo($"Register new user - {user.TelegramNick}",user.TelegramId.ToString());
                return user;
            }
            catch
            {
                Botlog.WriteError(info.TelegramId,$"Can't add new user {info.TelegramId}");
                return null;
            }
        }
        

    }
}
