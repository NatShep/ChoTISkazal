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

        public void UpdateAgingAndRandomization(int count)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                foreach (var word in cnn.Query<UserPair>(@"Select * From UserPairs order by RANDOM() limit @count", new { count }).ToArray())
                {
                    FindMetricOrNull(word.MetricId).UpdateAgingAndRandomization();
                    var op = $"Update QuestionMetric set AggregateScore = " +
                             $"{FindMetricOrNull(word.MetricId).AggregateScore.ToString(CultureInfo.InvariantCulture)} " +
                             $"where MetricId = {word.MetricId}";
                    cnn.Execute(op);
                }
            }
        }
        public QuestionMetric FindMetricOrNull(int metricId )
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return cnn.Query<QuestionMetric>(
                    @"SELECT * FROM QuestionMetric WHERE MetricId=@metricId", new{metricId}).FirstOrDefault();
            }
        }
        
        public void AddQuestionMetric(QuestionMetric metric)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                cnn.Execute(
                    @"INSERT INTO QuestionMetric ( 
                    ElaspedMs,
                    Result,
                    Type,
                    Revision,
                    PreviousExam,  
                    LastExam,
                    Examed,
                    ExamsPassed,
                    PassedScore,
                    AggregateScoreBefore, 
                    AggregateScore,
                    PassedScoreBefore)  
                    
                Values( 
                    @ElaspedMs,
                    @Result,
                    @Type,
                    @Revision,
                    @PreviousExam,  
                    @LastExam,
                    @Examed,
                    @ExamsPassed,
                    @PassedScore,
                    @AggregateScoreBefore, 
                    @AggregateScore,
                    @PassedScoreBefore)", metric);
               /* cnn.Execute(
                    @"INSERT INTO QuestionMetric ( 
                    ElaspedMs,
                    Result,
                    Type,
                    Revision,
                    PreviousExam,  
                    LastExam,
                    Examed,
                    ExamsPassed,
                    PassedScore,
                    AggregateScoreBefore, 
                    AggregateScore,
                    PassedScoreBefore)  
                    
                Values( 
                    @ElaspedMs,
                    @Result,
                    @Type,
                    @Revision,
                    @PreviousExam,  
                    @LastExam,
                    @Examed,
                    @ExamsPassed,
                    @PassedScore,
                    @AggregateScoreBefore, 
                    @AggregateScore,
                    @PassedScoreBefore)", metric);
        */    }
        }
        public void AddExam(Exam exam)
        {
            CheckDbFile.Check(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();
            cnn.Execute(
                @"INSERT INTO Exams (UserId, Count, Passed, Failed, Started, Finished)
                                Values(@UserId, @Count, @Passed, @Failed,@Started, @Finished)", exam);
        }

        public void UpdateScores(QuestionMetric metric)
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var op =
                    $"Update QuestionMetric set AggregateScore = @AggregateScore," +
                    $"PassedScore = @PassedScore, " +
                    $"LastExam = @LastExam," +
                    $"Examed = @Examed "+
                    $"WHERE MetricId = @MetricId";
                cnn.Execute(op, metric);
            }
        }
      
        
     //TODO additional methods 
        public Exam[] GetAllExams()
        {
            if (!File.Exists(DbFile))
                ApplyMigrations();

            using var cnn = SimpleDbConnection();
            cnn.Open();
            return cnn.Query<Exam>(@"Select * from Exams").ToArray();
        }
        public QuestionMetric[] GetAllQuestionMetrics()
        {
            if (!File.Exists(DbFile))
                return new QuestionMetric[0];
            using var cnn = SimpleDbConnection();

            cnn.Open();
            return cnn.Query<QuestionMetric>(@"Select * from QuestionMetrics").ToArray();
        }
        public void UpdateScoresAndTranslation(UserPair word)
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
                    $"LastExam = @LastExam," +
                    $"AllMeanings = @AllMeanings," +
                    $"Revision = @Revision," +
                    $"Examed = @Examed where PairId = @PairId";
                cnn.Execute(op, word);
            }
        }
        public void UpdateAgingAndRandomization()
        {
            CheckDbFile.Check(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                foreach (var word in cnn.Query<UserPair>(@"Select * From UserPairs").ToArray())
                {
                    FindMetricOrNull(word.MetricId).UpdateAgingAndRandomization();
                    var op = $"Update QuestionMetric set AggregateScore = { FindMetricOrNull(word.MetricId).AggregateScore.ToString(CultureInfo.InvariantCulture)} where Id = {word.Id}";
                    cnn.Execute(op);
                }
            }
        }
    }
}
