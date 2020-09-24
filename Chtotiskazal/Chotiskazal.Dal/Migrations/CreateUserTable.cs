
namespace Chotiskazal.Dal.Migrations
{ 
public class CreateUserTableMigration : SimpleMigration
    {
        public override string Name => "AddUserTable";

        public override string  Query => @"create table if not exists Users
              (
                 UserId                              integer primary key AUTOINCREMENT,
                 Name                                nvarchar(100) not null,
                 Login                               nvarchar(100) UNIQUE not null,
                 Password                            nvarchar(100) not null,
                 Email                               nvarchar(100) not null UNIQUE CHECK(Email !=''),
                 Created                             datetime not null,
                 Online                              integer not null
              )";
    }
}