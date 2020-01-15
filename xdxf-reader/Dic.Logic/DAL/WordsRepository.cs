using System;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using Dapper;

namespace Dic.Logic.DAL
{
    public class WordsRepository
    {
        public PairModel CreateNew(string word, string translation, string transcription)
        {
            var pair = PairModel.CreatePair(word, translation, transcription);
            Add(pair);
            return pair;
        }

        public PairModel[] GetAll()
        {
            if(!File.Exists(DbFile))
                return new PairModel[0];


            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return cnn.Query<PairModel>(@"Select * From Words order by AggregateScore").ToArray();
            }
        }
        public PairModel[] GetWorst(int count)
        {
            if (!File.Exists(DbFile))
                return new PairModel[0];
            using var cnn = SimpleDbConnection();
            cnn.Open();
            var query = $"Select * from Words order by AggregateScore limit {count}";
            return cnn.Query<PairModel>(query).ToArray();
        }

        public void UpdateAgingAndRandomization()
        {
            if (!File.Exists(DbFile))
                return;

            //if (File.Exists(DbFile))
            //    File.Delete(DbFile);
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                foreach (var word in cnn.Query<PairModel>(@"Select * From Words").ToArray())
                {
                    word.UpdateAgingAndRandomization();
                    var op = $"Update words set AggregateScore = {word.AggregateScore.ToString(CultureInfo.InvariantCulture)} where Id = {word.Id}";
                    cnn.Execute(op);
                }
            }
        }

        public void UpdateScores(PairModel word)
        {
            if (!File.Exists(DbFile))
                return;

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var op =
                    $"Update words set AggregateScore =  @AggregateScore,"+
                    $"PassedScore = @PassedScore, " +
                    $"Created = @Created," +
                    $"LastExam = @LastExam," +
                    $"Examed = @Examed where Id = @Id";
                cnn.Execute(op, word);
            }
        }

        public static string DbFile => Environment.CurrentDirectory + "\\MyWords.sqlite";

        public static SQLiteConnection SimpleDbConnection() => new SQLiteConnection("Data Source=" + DbFile);

        private void Add(PairModel pair)
        {
            if (!File.Exists(DbFile))
            {
                CreateDatabase();
            }

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                pair.Id = cnn.Query<long>(
                    @"INSERT INTO Words (  OriginWord,  Translation,  Transcription, Created, LastExam, PassedScore, AggregateScore, Examed )   
                                      VALUES( @OriginWord,  @Translation,  @Transcription, @Created, @LastExam, @PassedScore, @AggregateScore, @Examed ); 
                          select last_insert_rowid()", pair).First();
            }
        }

        public PairModel GetOrNull(int id)
        {
            if (!File.Exists(DbFile))
                return null;
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<PairModel>(
                    @"SELECT Id, OriginWord,  Translation,  Transcription, Created, LastExam, PassedScore, AggregateScore, Examed
            FROM Words
            WHERE Id = @id", new {id}).FirstOrDefault();
                return result;
            }
        }
        private static void CreateDatabase()
        {
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                cnn.Execute(
                    @"create table Words
              (
                 Id                                  integer primary key AUTOINCREMENT,
                 OriginWord                           nvarchar(100) not null,
                 Translation                          nvarchar(100) not null,
                 Created                              datetime not null,
                 LastExam                             datetime not null,
                 PassedScore integer not null,
                 Examed integer not null,
                 Transcription nvarchar(100) null,
                 AggregateScore real not null 

              )");
            }
        }
    }
}
