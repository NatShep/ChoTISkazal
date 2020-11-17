using Chotiskazal.Dal.Repo;
using System;
using System.Threading.Tasks;
using Chotiskazal.Dal.DAL;

namespace Chotiskazal.Dal.Services
{
    public class ExamsAndMetricService
    {
        private readonly ExamsAndMetricsRepo _examsAndMetricsRepo;

        public ExamsAndMetricService(ExamsAndMetricsRepo repo) => _examsAndMetricsRepo = repo;

        public async Task SaveQuestionMetricsAsync(QuestionMetric questionMetric) =>
           await _examsAndMetricsRepo.AddQuestionMetricAsync(questionMetric);

        public async Task RegisterExamAsync(int userId, DateTime started, int count, int successCount)
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
    }
}