using Chotiskasal.DAL;
using Dapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Chotiskazal.Dal.Repo
{
    class ExamsAndMetricsRepo:BaseRepo
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


    }
}
