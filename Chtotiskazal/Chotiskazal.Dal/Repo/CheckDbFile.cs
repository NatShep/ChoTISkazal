using System;
using System.IO;
using Chotiskazal.Dal.Migrations;

namespace Chotiskazal.Dal
{
    public static class CheckDbFile
    {
        public static void Check(string nameFile)
        {
            if (!File.Exists(nameFile))
                throw new Exception("No db file!");
            
          //  DoMigration.ApplyMigrations(nameFile);

        }
    }
}