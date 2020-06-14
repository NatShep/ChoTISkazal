using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chotiskazal.Dal.Services
{
    class UsersWordService
    {
        private readonly UserWordsRepo _repository;
        private 

        public UsersWordService(UserWordsRepo repository)
        { 
            _repository = repository;
        }
        public string[] GetAllUserTranslatesOfWord(User user, string word)
        {
            var userPairs= _repository.GetAllTranslate(user, word);
            return pairs.Select(s => s.Translate).ToArray();
        }
    }
}
