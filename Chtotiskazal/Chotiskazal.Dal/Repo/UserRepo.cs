using Dapper;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Dal.DAL;

namespace Chotiskazal.Dal.Repo
{
    public class UserRepo : BaseRepo
    {
        public UserRepo(string fileName) : base(fileName)
        {
        }

        public async Task<int> AddUserAsync(User user)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return await cnn.ExecuteScalarAsync<int>(
                    @"INSERT INTO Users (TelegramId, Name,  Login,  Password, Email, Created)   
                                      VALUES(@TelegramId, @Name,  @Login,  @Password, @Email, @Created);
                      SELECT last_insert_rowid();", user);
            }
        }

        public async Task<User> GetUserByLoginOrNullAsync(string login, string password)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return (await cnn.QueryAsync<User>(
                    @"SELECT *
                    FROM Users
                    WHERE Login = @login AND Password=@Password", new {login, password})).FirstOrDefault();
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
    }
}
