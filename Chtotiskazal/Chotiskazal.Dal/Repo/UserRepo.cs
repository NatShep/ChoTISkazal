using Chotiskazal.Dal.Repo;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chotiskazal.Logic.DAL
{
    public class UserRepo:BaseRepo
    {
        private readonly string _fileName;

        public UserRepo(string fileName) : base(fileName) { }
    }
}
