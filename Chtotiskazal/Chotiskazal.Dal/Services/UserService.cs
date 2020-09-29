using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;


namespace Chotiskazal.Dal.Services
{
    public class UserService
    {
        private readonly UserRepo _repository;
        public UserService(UserRepo repository) => _repository = repository;
        
        public int AddUser(User user) => _repository.AddUser(user);
        
        public User GetUserByLogin(string login) => _repository.GetUserByLoginOrNull(login);
        
        //TODO
        public void DeleteUser(User user) {}

        //TODO
        public bool IsUserOnline(User user) => true;
       
        //TODO
        public User[] GetAllUser() => _repository.GetAllUsers();
        


       
    }
}
