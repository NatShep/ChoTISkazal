using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Dapper;
using System.Linq;

using System;
using System.Collections.Generic;
using System.Text;

namespace Chotiskazal.Dal.Repo
{
    public class UserRepo:BaseRepo
    {
        private readonly string _fileName;

        public UserRepo(string fileName) : base(fileName) { }

        public int AddUser(User user)
        {
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var id = cnn.ExecuteScalar<int>(
                    @"INSERT INTO Users ( Name,  Login,  Password, Email, Created, Online)   
                                      VALUES( @Name,  @Login,  @Password, @Email, @Created, @Online);
                      SELECT last_insert_rowid();", user);
                return id;
            }
        }

        public User GetUser(string login)
        {
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var user = cnn.Query<User>(
                    @"SELECT UserId, Name, Login, Password,Email,Created,Online
                    FROM Users
                    WHERE Login = @login", new { login }).FirstOrDefault(); 
                return user;
            }

        }

        public User[] GetAllUsers()
        {
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var user= cnn.Query<User>(@"Select * from Users order by Name").ToArray();
                return user;
            }
        }
    }
}
