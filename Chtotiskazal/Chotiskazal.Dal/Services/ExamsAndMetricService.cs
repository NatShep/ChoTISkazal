using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chotiskazal.Dal.Services
{
    public class ExamsAndMetricService
    {
        private readonly ExamsAndMetricsRepo _examsAndMetricsRepo;

        public ExamsAndMetricService(ExamsAndMetricsRepo repo) => _examsAndMetricsRepo = repo;

        public async Task<QuestionMetric> GetAllMetricsForPairAsync(int metricId) =>await _examsAndMetricsRepo.FindMetricOrNullAsync(metricId);

        public async Task SaveQuestionMetricsAsync(QuestionMetric questionMetric) =>
           await _examsAndMetricsRepo.AddQuestionMetricAsync(questionMetric);


        public async Task RegistrateExamAsync(int userId, DateTime started, int count, int successCount)
        {
            await _examsAndMetricsRepo.AddExamAsync(new Exam
            {
                UserId = userId,
                Started = started,
                Count = count,
                Failed = count - successCount,
                Finished = DateTime.Now,
                Passed = successCount
            });
        }

       

        public Exam[] GetAllExams() =>  throw new NotImplementedException();

        public void UpdateAgingAndRandomize() =>  throw new NotImplementedException();

        public void UpdateRatings(UserPair userPairModel) => throw new NotImplementedException();
    }
}