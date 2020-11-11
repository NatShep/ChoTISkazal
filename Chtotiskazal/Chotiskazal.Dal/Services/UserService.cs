using System;
using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;


namespace Chotiskazal.Dal.Services
{
    public class UserService
    {
        private readonly UserRepo _repository;
        public UserService(UserRepo repository) => _repository = repository;
        
        public int AddUser(User user) => _repository.AddUser(user);
        
        public User GetUserByLoginOrNull(string login, string password) => _repository.GetUserByLoginOrNull(login,password);
        public User GetUserByTelegramId(in long telegramId) => _repository.GetUserByTelegramId(telegramId);

     
        //TODO additional methods
        public User[] GetAllUser() =>  throw new NotImplementedException();
        
        public void DeleteUser(User user) {}

        public bool IsUserOnline(User user) =>  throw new NotImplementedException();

    }
}
