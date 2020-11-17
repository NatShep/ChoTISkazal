using System;
using System.Threading.Tasks;
using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Chotiskazal.Dal.DAL;


namespace Chotiskazal.Dal.Services
{
    public class UserService
    {
        private readonly UserRepo _repository;
        public UserService(UserRepo repository) => _repository = repository;
        
        public async Task<int> AddUserAsync(User user) => await _repository.AddUserAsync(user);
        
        public async Task<User> GetUserByLoginOrNullAsync(string login, string password) => 
            await _repository.GetUserByLoginOrNullAsync(login,password);
        
        public async Task<User> GetUserByTelegramIdAsync(long telegramId) =>await _repository.GetUserByTelegramIdAsync(telegramId);
    }
}
