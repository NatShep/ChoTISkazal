using System;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using Dapper;
using Dic.Logic.DAL.Migrations;

namespace Dic.Logic.DAL
{
    public class WordsRepository
    {
        private readonly string _fileName;

        public WordsRepository(string fileName)
        {
            _fileName = fileName;
        }
        public PairModel CreateNew(string word, string translation, string transcription, Phrase[] phrases = null)
        {
            var pair = PairModel.CreatePair(word, translation, transcription, phrases);
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
                return cnn.Query<PairModel>(@"Select * From Words order by PassedScore").ToArray();
            }
        }
        public PairModel[] GetWorst(int count)
        {
            if (!File.Exists(DbFile))
                return new PairModel[0];
            using var cnn = SimpleDbConnection();
            cnn.Open();
            var query = $"Select * from Words order by AggregateScore desc limit {count}";
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
        public void UpdateScoresAndTranslation(PairModel word)
        {
            if (!File.Exists(DbFile))
                return;

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var op =
                    $"Update words set AggregateScore =  @AggregateScore," +
                    $"PassedScore = @PassedScore, " +
                    $"Translation = @Translation,"+
                    $"Created = @Created," +
                    $"LastExam = @LastExam," +
                    $"Examed = @Examed where Id = @Id";
                cnn.Execute(op, word);
            }
        }

        public  string DbFile => Path.Combine(Environment.CurrentDirectory, _fileName );

        private SQLiteConnection SimpleDbConnection() => new SQLiteConnection("Data Source=" + DbFile);

        private void Add(PairModel pair)
        {
            if (!File.Exists(DbFile))
            {
                ApplyMigrations();
            }

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                pair.Id = cnn.Query<long>(
                    @"INSERT INTO Words (  OriginWord,  Translation,  Transcription, Created, LastExam, PassedScore, AggregateScore, Examed )   
                                      VALUES( @OriginWord,  @Translation,  @Transcription, @Created, @LastExam, @PassedScore, @AggregateScore, @Examed ); 
                          select last_insert_rowid()", pair).First();

                if (pair.Phrases != null)
                {
                    foreach (var phrase in pair.Phrases)
                    {
                        phrase.Created = DateTime.Now;
                        cnn.Execute(
                            @"INSERT INTO Words (  Origin,  Translation,  Created, OriginWord, TranslationWord)   
                                      VALUES( @Origin,  @Translation,  @Created, @OriginWord, @TranslationWord); 
                          select last_insert_rowid()", phrase);
                    }
                }
            }
        }

        public PairModel GetOrNull(string word)
        {
            if (!File.Exists(DbFile))
                return null;
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<PairModel>(
                    @"SELECT * FROM Words WHERE OriginWord = @word", new { word }).FirstOrDefault();
                return result;
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
        public void AddExam(DateTime started, DateTime finished, int count, int passed, int failed)
        {
            if (!File.Exists(DbFile))
                ApplyMigrations();
            
            using var cnn = SimpleDbConnection();
            cnn.Open();
            cnn.Execute(
                @"INSERT INTO ExamHistory (Count, Passed, Failed, Started, Finished)
                                Values(@Count, @Passed, @Failed,@Started, @Finished)", new
                {
                    Count = count,
                    Passed = passed,
                    Failed = failed,
                    Started = started,
                    Finished = finished,
                });
        }
        public void ApplyMigrations()
        {
            var migrationsList = new IMigration[]
            {
                new InitMigration(),
                new AddWordsTableMigration(),
                new AddHistoryMigration(),
                new AddPhraseMigration()
            };
            Console.WriteLine(")Applying migrations");
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                int lastAppliedMigrationIndex = -1;
                try
                {
                    var lastMigrationName = cnn.Query<string>("Select Name from migrations Order by id desc limit 1")
                        .FirstOrDefault();
                    Console.WriteLine("Last migration: "+ lastMigrationName);

                    if (lastMigrationName != null)
                    {
                        lastAppliedMigrationIndex =  Array.IndexOf(migrationsList, migrationsList.Single(m => m.Name == lastMigrationName));
                    }
                }
                catch( Exception e)
                {
                    Console.WriteLine("Init migration skipped");
                }

                lastAppliedMigrationIndex++;
                if (lastAppliedMigrationIndex < migrationsList.Length)
                {
                    for (int i = lastAppliedMigrationIndex; i < migrationsList.Length; i++)
                    {
                        Console.WriteLine("Applying migration "+ migrationsList[i]);
                        cnn.Execute(migrationsList[i].Query);
                    }

                    cnn.Execute("insert into migrations (name) values (@name)", new {name = migrationsList.Last().Name});
                }
                else
                {
                    Console.WriteLine("No migration should be applied");
                }
            }
        }
    }
}
