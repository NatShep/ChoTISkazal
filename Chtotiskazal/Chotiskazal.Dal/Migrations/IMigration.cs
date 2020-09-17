using System.Data.SQLite;
using Dapper;

namespace Chotiskazal.Dal.Migrations
{
    interface IMigration
    {
        string Name { get; }

        void Migrate(SQLiteConnection cnn);
    }

    public abstract class SimpleMigration : IMigration
    {
        protected SimpleMigration()
        {
            
        }
        public abstract string Query { get; }

        public abstract  string Name { get; }
        public void Migrate(SQLiteConnection connection)
        {
            connection.Execute(Query);
        }
    }
}
