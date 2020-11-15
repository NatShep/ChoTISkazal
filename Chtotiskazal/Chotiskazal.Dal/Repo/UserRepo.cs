using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Dapper;
using System.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Chotiskazal.Dal.Repo
{
    public class UserRepo : BaseRepo
    {
        private readonly string _fileName;

        public UserRepo(string fileName) : base(fileName) {}

        public async  Task<int> AddUserAsync(User user)
        {
            CheckDbFile(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return await cnn.ExecuteScalarAsync<int>(
                    @"INSERT INTO Users (TelegramId, Name,  Login,  Password, Email, Created, Online)   
                                      VALUES(@TelegramId, @Name,  @Login,  @Password, @Email, @Created, @Online);
                      SELECT last_insert_rowid();", user);
            }
        }

        public async Task<User> GetUserByLoginOrNullAsyc(string login,string password)
        {
            CheckDbFile(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return (await cnn.QueryAsync<User>(
                    @"SELECT *
                    FROM Users
                    WHERE Login = @login AND Password=@Password", new {login,password})).FirstOrDefault();
            }
        }

        public async Task<User> GetUserByTelegramIdAsync(long telegramId)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return (await cnn.QueryAsync<User>(
                    @"SELECT *
                    FROM Users
                    WHERE TelegramId = @telegramId", new {telegramId})).FirstOrDefault();
            }
        }
        
        //TODO additional methods 
        public User[] GetAllUsers()
        {
            CheckDbFile(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var user = cnn.Query<User>(@"Select * from Users order by Login").ToArray();
                return user;
            }
        }

    
    }
}
