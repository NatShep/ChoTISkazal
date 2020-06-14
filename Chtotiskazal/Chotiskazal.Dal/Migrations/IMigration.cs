using System.Data.SQLite;
using Chotiskazal.Dal.Repo;
using Dapper;

namespace Chotiskazal.Dal.Migrations
{
    interface IMigration
    {
        string Name { get; }

        void Migrate(SQLiteConnection cnn, BaseRepo baseRepo);
    }

    public abstract class SimpleMigration : IMigration
    {
        protected SimpleMigration()
        {
            
        }
        public abstract string Query { get; }

        public abstract  string Name { get; }
        public void Migrate(SQLiteConnection connection, BaseRepo repository)
        {
            connection.Execute(Query);
        }
    }
}
