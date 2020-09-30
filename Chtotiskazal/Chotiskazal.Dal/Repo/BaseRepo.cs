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

        public BaseRepo(string fileName) => _fileName = fileName;
        
        public SQLiteConnection SimpleDbConnection() => new SQLiteConnection("Data Source=" + DbFile);

        protected void ApplyMigrations() => DoMigration.ApplyMigrations(DbFile);
    }
}
