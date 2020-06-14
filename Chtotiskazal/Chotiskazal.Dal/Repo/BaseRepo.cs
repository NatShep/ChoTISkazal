using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using Chotiskazal.Dal.Migrations;
using Dapper;

namespace Chotiskazal.Dal.Repo
{
    public abstract class BaseRepo
    {
        private readonly string _fileName;
        protected string DbFile => Path.Combine(Environment.CurrentDirectory, _fileName);

        public BaseRepo(string fileName)
        {
            _fileName = fileName;
        }

        protected SQLiteConnection SimpleDbConnection() => new SQLiteConnection("Data Source=" + DbFile);

        protected void ApplyMigrations()
        {
            var migrationsList = new IMigration[]
            {
                new InitMigration(),
                //....
            };
            Console.WriteLine(")Applying migrations");
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                int lastAppliedMigrationIndex = -1;
                string[] allMigrationNames = new string[0];
                try
                {
                    allMigrationNames = cnn.Query<string>("Select Name from migrations").ToArray();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Init migration skipped");
                }

                foreach (var migration in migrationsList)
                {
                    if (!allMigrationNames.Contains(migration.Name))
                    {
                        Console.WriteLine("Applying migration " + migration.Name);
                        migration.Migrate(cnn, this);
                        cnn.Execute("insert into migrations (name) values (@name)", new { name = migrationsList.Last().Name });
                    }
                }
            }
        }
    }
}
