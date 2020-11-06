using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chotiskazal.Dal.Services
{
    public class ExamsAndMetricService
    {
        private readonly ExamsAndMetricsRepo _examsAndMetricsRepo;

        public ExamsAndMetricService(ExamsAndMetricsRepo repo) => _examsAndMetricsRepo = repo;

        public QuestionMetric GetAllMetricsForPair(int metricId) => _examsAndMetricsRepo.FindMetricOrNull(metricId);

        public void SaveQuestionMetrics(QuestionMetric questionMetric) =>
            _examsAndMetricsRepo.AddQuestionMetric(questionMetric);


        public void RegistrateExam(int userId, DateTime started, int count, int successCount)
        {
            _examsAndMetricsRepo.AddExam(new Exam
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