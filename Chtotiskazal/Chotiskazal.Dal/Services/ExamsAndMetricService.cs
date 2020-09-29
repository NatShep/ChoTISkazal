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

        //TODO all methods
        public Exam[] GetAllExams() => _examsAndMetricsRepo.GetAllExams();
        public void UpdateAgingAndRandomize()
        {
            _examsAndMetricsRepo.UpdateAgingAndRandomization();
        }
        public void UpdateAgingAndRandomize(int count)
        {
            _examsAndMetricsRepo.UpdateAgingAndRandomization(count);
        }
        public void RegistrateFailure(UsersPair usersPair)
        {
            var metric = _examsAndMetricsRepo.FindMetricOrNull(usersPair.MetricId);
            metric.OnExamFailed();
            metric.UpdateAgingAndRandomization();
            _examsAndMetricsRepo.UpdateScores(metric);
        }
        public void RegistrateExam(DateTime started, int count, int successCount)
        {
            _examsAndMetricsRepo.AddExam(new Exam
            {
                Started = started,
                Count = count,
                Failed = count - successCount,
                Finished = DateTime.Now,
                Passed = successCount
            });
        }
        public void RegistrateSuccess(UsersPair usersPair)
        {
            var metric = _examsAndMetricsRepo.FindMetricOrNull(usersPair.MetricId);
            metric.OnExamPassed();
            metric.UpdateAgingAndRandomization();
            _examsAndMetricsRepo.UpdateScores(metric);
        }
        public void SaveQuestionMetrics(QuestionMetric questionMetric)
        {
            _examsAndMetricsRepo.AddQuestionMetric(questionMetric);
        }
        public void UpdateRatings(UsersPair usersPairModel)
        {
            _examsAndMetricsRepo.UpdateScoresAndTranslation(usersPairModel);
        }
    }
}
