using System;
using System.Threading.Tasks;
using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;


namespace Chotiskazal.Dal.Services
{
    public class UserService
    {
        private readonly UserRepo _repository;
        public UserService(UserRepo repository) => _repository = repository;
        
        public async Task<int> AddUserAsync(User user) => await _repository.AddUserAsync(user);
        
        public async Task<User> GetUserByLoginOrNullAsync(string login, string password) => 
            await _repository.GetUserByLoginOrNullAsyc(login,password);
        public async Task<User> GetUserByTelegramIdAsync(long telegramId) =>await _repository.GetUserByTelegramIdAsync(telegramId);

     
        //TODO additional methods
        public User[] GetAllUser() =>  throw new NotImplementedException();
        
        public void DeleteUser(User user) {}

        public bool IsUserOnline(User user) =>  throw new NotImplementedException();

    }
}
