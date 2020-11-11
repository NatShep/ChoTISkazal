using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Dapper;
using System.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Chotiskazal.Dal.Repo
{
    public class UserRepo : BaseRepo
    {
        private readonly string _fileName;

        public UserRepo(string fileName) : base(fileName) {}

        public int AddUser(User user)
        {
            CheckDbFile.Check(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var id = cnn.ExecuteScalar<int>(
                    @"INSERT INTO Users (TelegramId, Name,  Login,  Password, Email, Created, Online)   
                                      VALUES(@TelegramId, @Name,  @Login,  @Password, @Email, @Created, @Online);
                      SELECT last_insert_rowid();", user);
                return id;
            }
        }

        public User GetUserByLoginOrNull(string login,string password)
        {
            CheckDbFile.Check(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var user = cnn.Query<User>(
                    @"SELECT *
                    FROM Users
                    WHERE Login = @login AND Password=@Password", new {login,password}).FirstOrDefault();
                return user;
            }
        }

        public User GetUserByTelegramId(in long telegramId)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var user = cnn.Query<User>(
                    @"SELECT *
                    FROM Users
                    WHERE TelegramId = @telegramId", new {telegramId}).FirstOrDefault();
                return user;
            }
        }
        
        //TODO additional methods 
        public User[] GetAllUsers()
        {
            CheckDbFile.Check(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var user = cnn.Query<User>(@"Select * from Users order by Login").ToArray();
                return user;
            }
        }

    
    }
}
