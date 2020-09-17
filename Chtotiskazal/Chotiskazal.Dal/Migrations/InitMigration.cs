namespace Chotiskazal.Dal.Migrations
{
    public class InitMigration : SimpleMigration
    {
        public override string Name => "Init";

        public override string Query =>
            @"create table if not exists Migrations (
                 Id     integer primary key AUTOINCREMENT,
                 Name   nvarchar(100) not null)";


    }
}