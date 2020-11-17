using Dapper;
using System;
using System.Data.SQLite;
using System.Linq;

namespace Chotiskazal.Dal.Migrations
{
    public static class DoMigration
    {
        public static void ApplyMigrations(string dbFile)
        {
            var migrationsList = new IMigration[]
            {
                new InitMigration(),
                new CreateUserTableMigration(),
                new CreatePairDictionaryTable(),
                new CreateQuestionMetricTable(),
                new CreatePhraseTableMigration(), 
                new CreateExamTable(), 
                new CreateUserWordTable(), 
            };
            Console.WriteLine("Applying migrations...");

            using (var cnn = new SQLiteConnection("Data Source=" + dbFile))
            {
                cnn.Open();
                var allMigrationNames = new string[0];
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
                    if (allMigrationNames.Contains(migration.Name)) 
                        continue;
                    Console.WriteLine("Applying migration " + migration.Name);
                    migration.Migrate(cnn);
                    cnn.Execute("insert into migrations (name) values (@name)", new { name = migration.Name });
                }
            }
        }
    }
}
