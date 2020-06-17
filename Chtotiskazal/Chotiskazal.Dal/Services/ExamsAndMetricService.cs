using Chotiskasal.DAL;
using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chotiskazal.Dal.Services
{
    class ExamsAndMetricService
    {
        private readonly ExamsAndMetricsRepo _examsAndMetricsRepo;

        public Exam[] GetAllExams() => _examsAndMetricsRepo.GetAllExams();

        public void UpdateAgingAndRandomize()
        {
            _examsAndMetricsRepo.UpdateAgingAndRandomization();
        }
        public void UpdateAgingAndRandomize(int count)
        {
            _examsAndMetricsRepo.UpdateAgingAndRandomization(count);
        }
        public void RegistrateFailure(UsersPair pair)
        {
            var metric = _examsAndMetricsRepo.FindMetricOrNull(pair.MetricId);
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
        public void RegistrateSuccess(UsersPair pair)
        {
            var metric = _examsAndMetricsRepo.FindMetricOrNull(pair.MetricId);
            metric.OnExamPassed();
            metric.UpdateAgingAndRandomization();
            _examsAndMetricsRepo.UpdateScores(metric);
        }
        public void SaveQuestionMetrics(QuestionMetric questionMetric)
        {
            _examsAndMetricsRepo.AddQuestionMetric(questionMetric);
        }
        public void UpdateRatings(UsersPair pairModel)
        {
            _examsAndMetricsRepo.UpdateScoresAndTranslation(pairModel);
        }
    }
}
