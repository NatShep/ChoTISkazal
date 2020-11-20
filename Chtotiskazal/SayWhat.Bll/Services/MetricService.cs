using System;
using System.Threading.Tasks;

namespace SayWhat.Bll.Services
{
    public class MetricService
    {

        public MetricService() {}

        public Task SaveQuestionMetrics(QuestionMetric questionMetric) => Task.CompletedTask;
           
        public Task RegisterExamAsync(long userId, DateTime started, int count, int successCount) 
            => Task.CompletedTask;
    }
}