using System;
using System.Threading.Tasks;
using SayWhat.MongoDAL.Users;

namespace SayWhat.Bll.Services
{
    public class UserService
    {
        private readonly UsersRepo _repository;
        public UserService(UsersRepo repository) => _repository = repository;

        public Task UpdateCounters(UserModel user) =>
            _repository.UpdateCounters(user);
        
        public Task Update(UserModel user) =>
            _repository.Replace(user);
        
        public async Task<UserModel> GetUserOrNull(TelegramUserInfo telegramUserInfo) 
            => await _repository.GetOrDefaultByTelegramIdOrNull(telegramUserInfo.TelegramId);

        public async Task<UserModel> GetOrAddUser(TelegramUserInfo telegramUserInfo)
        {
            var user = await GetUserOrNull(telegramUserInfo);
            // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
            user = user ?? await AddUserFromTelegram(telegramUserInfo);
            if (user == null)
            {
                throw new Exception("I can't add user!");
            }
            return user;
        }
        
        public async Task<UserModel> AddUserFromTelegram(TelegramUserInfo info)
        {
            try
            {
                var user = new UserModel(
                    telegramId:   info.TelegramId, 
                    firstName:    info.FirstName, 
                    lastName:     info.LastName, 
                    telegramNick: info.UserNick, 
                    source:       UserSource.Telegram);
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
