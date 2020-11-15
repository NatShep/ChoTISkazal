using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using Chotiskazal.Logic.DAL.Migrations;
using Dapper;
using Dic.Logic.DAL;

namespace Chotiskazal.Logic.DAL
{
    public class WordsRepository
    {
        private readonly string _fileName;

        public WordsRepository(string fileName)
        {
            _fileName = fileName;
        }
        public PairModel CreateNew(string word, string translation, string[] allMeanings, string transcription, Phrase[] phrases = null)
        {
            var pair = PairModel.CreatePair(word, translation, allMeanings, transcription, phrases);
            pair.Revision = 1;
            pair.UpdateAgingAndRandomization();
            Add(pair);
            return pair;
        }

        public int GetContextPhrasesCount()
        {
            if (!File.Exists(DbFile))
                return 0;

            using var cnn = SimpleDbConnection();
            
            cnn.Open();
            return cnn.Query<int>(@"Select count(*) From ContextPhrases").FirstOrDefault();
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

        public Phrase[] GetAllPhrases()
        {
            if (!File.Exists(DbFile))
                return new Phrase[0];

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return cnn.Query<Phrase>(@"Select * From ContextPhrases").ToArray();
            }
        }
        public PairModel[] GetPairsForTests(int count, int learnRate)
        {
            if (!File.Exists(DbFile))
                return new PairModel[0];
            using var cnn = SimpleDbConnection();
            cnn.Open();
            var lookup = new Dictionary<long, PairModel>();

            cnn.Query<PairModel, Phrase, PairModel>(
                @"Select w.*, c.* from 
                (SELECT * FROM Words where PassedScore > @learnRate order by AggregateScore desc limit @count) w 
                LEFT JOIN ContextPhrases c 
                on c.OriginWord = w.OriginWord",
                (w, c) =>
                {
                    if (!lookup.TryGetValue(w.Id, out var pair))
                        lookup.Add(w.Id, pair = w);

                    if (pair.Phrases == null)
                        pair.Phrases = new List<Phrase>();
                    if (!string.IsNullOrWhiteSpace(c.Origin))
                        pair.Phrases.Add(c);
                    return pair;
                }, new { count, learnRate });
            return lookup.Values.ToArray();
        }

        public PairModel[] GetWorst(int count)
        {
            if (!File.Exists(DbFile))
                return new PairModel[0];
            using var cnn = SimpleDbConnection();
            cnn.Open();

            var lookup = new Dictionary<long, PairModel>();

            cnn.Query<PairModel, Phrase, PairModel>(
            @"Select w.*, c.* from 
                (SELECT * FROM Words order by AggregateScore desc limit @count) w 
                LEFT JOIN ContextPhrases c 
                on c.OriginWord = w.OriginWord",
                (w, c) =>
                {
                    if (!lookup.TryGetValue(w.Id, out var pair))
                        lookup.Add(w.Id, pair = w);

                    if (pair.Phrases == null)
                        pair.Phrases = new List<Phrase>();
                    if (!string.IsNullOrWhiteSpace(c.Origin))
                        pair.Phrases.Add(c);
                    return pair;
                }, new {count});
            return lookup.Values.ToArray();
        }

        public void UpdateAgingAndRandomization(int count)
        {
            if (!File.Exists(DbFile))
                return;

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                foreach (var word in cnn.Query<PairModel>(@"Select * From Words order by RANDOM() limit @count", new {count}).ToArray())
                {
                    word.UpdateAgingAndRandomization();
                    var op = $"Update words set AggregateScore = {word.AggregateScore.ToString(CultureInfo.InvariantCulture)} where Id = {word.Id}";
                    cnn.Execute(op);
                }
            }
        }
        public void UpdateAgingAndRandomization()
        {
            if (!File.Exists(DbFile))
                return;

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
                    $"AllMeanings = @AllMeanings," +
                    $"Revision = @Revision," +
                    $"Examed = @Examed where Id = @Id";
                cnn.Execute(op, word);
            }
        }

        public QuestionMetric[] GetAllQuestionMetrics()
        {
            if (!File.Exists(DbFile))
                return new QuestionMetric[0];
            using var cnn = SimpleDbConnection();

            cnn.Open();
            return cnn.Query<QuestionMetric>(@"Select * from QuestionMetrics").ToArray();
        }

        public void AddQuestionMetric(QuestionMetric metric)
        {
            if (!File.Exists(DbFile))
            {
                ApplyMigrations();
            }

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                cnn.Execute(
                @"INSERT INTO QuestionMetrics ( 
                    Created,  
                    WordId,
                    ElaspedMs,
                    PreviousExam,  
                    WordAdded, 
                    AggregateScoreBefore, 
                    PhrasesCount, 
                    PassedScoreBefore, 
                    ExamsPassed, 
                    Result,
                    Type)   
                    
                Values( 
                    @Created,  
                    @WordId,
                    @ElaspedMs,
                    @PreviousExam,  
                    @WordAdded, 
                    @AggregateScoreBefore, 
                    @PhrasesCount, 
                    @PassedScoreBefore, 
                    @ExamsPassed, 
                    @Result,
                    @Type)             
                    ", metric);
            }
        }

        public  string DbFile => Path.Combine(Environment.CurrentDirectory, _fileName );

        private SQLiteConnection SimpleDbConnection() => new SQLiteConnection("Data Source=" + DbFile);

        public void Add(Phrase phrase)
        {
            if (!File.Exists(DbFile))
            {
                ApplyMigrations();
            }

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                phrase.Created = DateTime.Now;
                cnn.Execute(
                    @"INSERT INTO ContextPhrases ( Origin,  Translation,  Created, OriginWord, TranslationWord)   
                                      VALUES( @Origin,  @Translation,  @Created, @OriginWord, @TranslationWord)", phrase);
            }
        }

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
                    @"INSERT INTO Words (  OriginWord,  Translation,  Transcription, Created, LastExam, PassedScore, AggregateScore, Examed, AllMeanings,Revision )
                                      VALUES( @OriginWord,  @Translation,  @Transcription, @Created, @LastExam, @PassedScore, @AggregateScore, @Examed, @AllMeanings, @Revision  ); 
                          select last_insert_rowid()", pair).First();

                if (pair.Phrases != null)
                {
                    foreach (var phrase in pair.Phrases)
                    {
                        phrase.Created = DateTime.Now;
                        cnn.Execute(
                            @"INSERT INTO ContextPhrases ( Origin,  Translation,  Created, OriginWord, TranslationWord)   
                                      VALUES( @Origin,  @Translation,  @Created, @OriginWord, @TranslationWord)", phrase);
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

        public PairModel GetOrNullWithPhrases(string word)
        {
            if (!File.Exists(DbFile))
                return null;
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var lookup = new Dictionary<long, PairModel>();

                var result = cnn.Query<PairModel, Phrase,PairModel>(
                    @"SELECT w.*, c.* 
                    FROM Words w 
                    LEFT JOIN ContextPhrases c on c.OriginWord = @word
                    WHERE w.OriginWord = @word 
",
                    (w, c) =>
                    {
                        if (!lookup.TryGetValue(w.Id, out var pair))
                        {
                            lookup.Add(w.Id, pair = w);
                        }
                        if(pair.Phrases==null)
                            pair.Phrases = new List<Phrase>();
                        pair.Phrases.Add(c);
                        return pair;
                    } , new { word }).FirstOrDefault();
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

        public Exam[] GetAllExams()
        {
            if (!File.Exists(DbFile))
                ApplyMigrations();

            using var cnn = SimpleDbConnection();
            cnn.Open();
            return cnn.Query<Exam>(@"Select * from ExamHistory").ToArray();
        }
        public void AddExam(Exam exam)
        {
            if (!File.Exists(DbFile))
                ApplyMigrations();
            
            using var cnn = SimpleDbConnection();
            cnn.Open();
            cnn.Execute(
                @"INSERT INTO ExamHistory (Count, Passed, Failed, Started, Finished)
                                Values(@Count, @Passed, @Failed,@Started, @Finished)", exam);
        }
        public void ApplyMigrations()
        {
            var migrationsList = new IMigration[]
            {
                new InitMigration(),
                new AddWordsTableMigration(),
                new AddHistoryMigration(),
                new AddPhraseMigration(),
                new AddQuestionMetricsMigration(),
                new AddWordsPropertiesMigration(),
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
                catch( Exception e)
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

        public void Remove(Phrase phrase)
        {
            if (!File.Exists(DbFile))
                ApplyMigrations();

            using var cnn = SimpleDbConnection();
            cnn.Open();
            cnn.Execute(@"DELETE FROM ContextPhrases where Origin = @Origin", phrase);

        }
    }
}
