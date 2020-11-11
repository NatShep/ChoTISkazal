
namespace Chotiskazal.Dal.Migrations
{ 
public class CreateUserTableMigration : SimpleMigration
    {
        public override string Name => "AddUserTable";

        public override string  Query => @"create table if not exists Users
              (
                 UserId                              integer primary key AUTOINCREMENT,
                 TelegramId                          BIGINT UNIQUE ,
                 Name                                nvarchar(100),
                 Login                               nvarchar(100) UNIQUE,
                 Password                            nvarchar(100),
                 Email                               nvarchar(100) UNIQUE CHECK(Email !=''),
                 Created                             datetime not null,
                 Online                              boolean not null
              )";
    }
}