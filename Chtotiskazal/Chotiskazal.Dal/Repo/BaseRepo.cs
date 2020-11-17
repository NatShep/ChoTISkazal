using System;
using System.Data.SQLite;
using System.IO;

namespace Chotiskazal.Dal.Repo
{
    public abstract class BaseRepo
    {
        private readonly string _fileName;
        protected string DbFile => Path.Combine(Environment.CurrentDirectory, _fileName);

        protected BaseRepo(string fileName) => _fileName = fileName;

        protected SQLiteConnection SimpleDbConnection() => new SQLiteConnection("Data Source=" + DbFile);

        protected static void CheckDbFile(string nameFile)
        {
            if (!File.Exists(nameFile))
                throw new Exception("No db file!");
            
            //  DoMigration.ApplyMigrations(nameFile);

        }
    }
    
}
