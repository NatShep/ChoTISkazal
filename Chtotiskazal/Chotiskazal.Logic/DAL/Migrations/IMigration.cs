using System.Data.SQLite;
using Dapper;

namespace Chotiskazal.Logic.DAL.Migrations
{
    interface IMigration
    {
        string Name { get; }

        void Migrate(SQLiteConnection connection, WordsRepository repository);
    }

    public abstract class SimpleMigration : IMigration
    {
        protected SimpleMigration()
        {
            
        }
        public abstract string Query { get; }

        public abstract  string Name { get; }
        public void Migrate(SQLiteConnection connection, WordsRepository repository)
        {
            connection.Execute(Query);
        }
    }
}
