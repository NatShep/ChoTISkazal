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
    }
}
