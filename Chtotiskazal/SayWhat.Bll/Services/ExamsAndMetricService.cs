using System;
using System.Threading.Tasks;
using Chotiskazal.Dal.DAL;

namespace SayWhat.Bll.Services
{
    public class ExamsAndMetricService
    {

        public ExamsAndMetricService() {}

        public Task SaveQuestionMetricsAsync(QuestionMetric questionMetric) => Task.CompletedTask;
           
        public Task RegisterExamAsync(long userId, DateTime started, int count, int successCount) 
            => Task.CompletedTask;
    }
}