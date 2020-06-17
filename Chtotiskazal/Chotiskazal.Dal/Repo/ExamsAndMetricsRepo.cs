using Chotiskasal.DAL;
using Chotiskazal.DAL;
using Dapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Chotiskazal.Dal.Repo
{
    public class ExamsAndMetricsRepo:BaseRepo
    {
        public ExamsAndMetricsRepo(string fileName) : base(fileName) { }
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

        public void UpdateAgingAndRandomization(int count)
        {
            if (!File.Exists(DbFile))
                return;

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                foreach (var word in cnn.Query<UsersPair>(@"Select * From Words order by RANDOM() limit @count", new { count }).ToArray())
                {
                    
                    FindMetricOrNull(word.MetricId).UpdateAgingAndRandomization();
                    var op = $"Update words set AggregateScore = {FindMetricOrNull(word.MetricId).AggregateScore.ToString(CultureInfo.InvariantCulture)} where Id = {word.Id}";
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
                foreach (var word in cnn.Query<UsersPair>(@"Select * From Words").ToArray())
                {
                    FindMetricOrNull(word.MetricId).UpdateAgingAndRandomization();
                    var op = $"Update words set AggregateScore = { FindMetricOrNull(word.MetricId).AggregateScore.ToString(CultureInfo.InvariantCulture)} where Id = {word.Id}";
                    cnn.Execute(op);
                }
            }
        }

        public QuestionMetric FindMetricOrNull(int metricId )
        {
            return new QuestionMetric();
        }
        public void UpdateScores(QuestionMetric metric)
        {
            if (!File.Exists(DbFile))
                return;

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var op =
                    $"Update words set AggregateScore =  @AggregateScore," +
                    $"PassedScore = @PassedScore, " +
                    $"Created = @Created," +
                    $"LastExam = @LastExam," +
                    $"Examed = @Examed where Id = @Id";
                cnn.Execute(op, metric);
            }
        }
        public void UpdateScoresAndTranslation(UsersPair word)
        {
            if (!File.Exists(DbFile))
                return;

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var op =
                    $"Update words set AggregateScore =  @AggregateScore," +
                    $"PassedScore = @PassedScore, " +
                    $"Translation = @Translation," +
                    $"Created = @Created," +
                    $"LastExam = @LastExam," +
                    $"AllMeanings = @AllMeanings," +
                    $"Revision = @Revision," +
                    $"Examed = @Examed where Id = @Id";
                cnn.Execute(op, word);
            }
        }

    
    }
}
