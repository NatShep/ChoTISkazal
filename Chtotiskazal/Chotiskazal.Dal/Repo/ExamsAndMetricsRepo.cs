using Dapper;
using System.Threading.Tasks;
using Chotiskazal.Dal.DAL;

namespace Chotiskazal.Dal.Repo
{
    public class ExamsAndMetricsRepo : BaseRepo
    {
        public ExamsAndMetricsRepo(string fileName) : base(fileName)
        {
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
            {
                cnn.Open();
                await cnn.ExecuteAsync(
                    @"INSERT INTO Exams (UserId, Count, Passed, Failed, Started, Finished)
                                Values(@UserId, @Count, @Passed, @Failed,@Started, @Finished)", exam);
            }
        }
    }
}
