using Chotiskazal.DAL;
using Dapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chotiskazal.Dal.Repo
{
    public class ExamsAndMetricsRepo:BaseRepo
    {
        public ExamsAndMetricsRepo(string fileName) : base(fileName) { }

      
        public async Task<QuestionMetric>FindMetricOrNullAsync(int metricId )
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return cnn.Query<QuestionMetric>(
                    @"SELECT * FROM QuestionMetric WHERE MetricId=@metricId", new{metricId}).FirstOrDefault();
            }
        }
        
        public async Task AddQuestionMetricAsync(QuestionMetric metric)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                await cnn.ExecuteAsync(
                    @"INSERT INTO QuestionMetric ( 
                    WordId,
                    Created,
                    ElaspedMs,
                    Result,
                    Type,
                    PreviousExam,  
                    ExamsPassed,
                    AggregateScoreBefore, 
                    PassedScoreBefore)  
                    
                Values( 
                    @WordId,
                    @Created,
                    @ElaspedMs,
                    @Result,
                    @Type,
                    @PreviousExam,  
                    @ExamsPassed,
                    @AggregateScoreBefore, 
                    @PassedScoreBefore)", metric);
               }
        }
        public async Task AddExamAsync(Exam exam)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();
            cnn.ExecuteAsync(
                @"INSERT INTO Exams (UserId, Count, Passed, Failed, Started, Finished)
                                Values(@UserId, @Count, @Passed, @Failed,@Started, @Finished)", exam);
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
      
    }
}
