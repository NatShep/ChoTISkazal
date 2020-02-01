namespace Chotiskazal.Logic.DAL.Migrations
{
    public class InitMigration : IMigration
    {
        public string Name => "Init";

        public string Query =>
            @"create table if not exists Migrations (
                 Id     integer primary key AUTOINCREMENT,
                 Name   nvarchar(100) not null)";

    }
}