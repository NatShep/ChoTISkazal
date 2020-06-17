using Chotiskazal.DAL;
using Chotiskazal.Logic.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chotiskazal.Logic.Services
{
    public class UserService
    {
        private readonly UserRepo _repository;
        public UserService(UserRepo repository)
        {
            _repository = repository;
        }

        public void AddUser(User user)
        {

        }

        public void DeleteUser(User user)
        {

        }

        public User GetUser(int userId)
        {
            return null;
        }

        public bool IsUserOnline(User user)
        {
            return true;
        }

        public User[] GetAllUser()
        {
            return new User[0];
        }

    
    }
}
