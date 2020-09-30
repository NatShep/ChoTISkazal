using System;
using System.Linq;
using Chotiskazal.Logic.Services;
using Dic.Logic.DAL;

namespace Chotiskazal.App.Modes
{
    class RandomizeMode: IConsoleMode
    {
        public string Name => "Randomize";
        public void Enter(NewWordsService service)
        {
            Console.WriteLine("Updating Db");
            service.UpdateAgingAndRandomize();

            var allModels = service.GetAll();

            foreach (var pairModel in allModels)
            {
                Console.WriteLine(
                    $"{pairModel.OriginWord} - {pairModel.Translation}   score: {pairModel.PassedScore} / {pairModel.Examed}  {pairModel.AggregateScore:##.##}");
            }

            Console.WriteLine($"Has {allModels.Length} models");

            var examSum = allModels.Sum(a => a.Examed);
            var passedSum = allModels.Sum(a => a.PassedScore);
            var passedAggregatedSum = allModels.Sum(a => Math.Min(1, a.PassedScore / (double)PairModel.MaxExamScore));
            var passedCount = allModels.Count(a => a.PassedScore >= PairModel.MaxExamScore);

            var count = allModels.Length;
            Console.WriteLine($"Knowledge:  {100 * passedAggregatedSum / (double)(count):##.##} %");
            Console.WriteLine($"Known:  {100 * passedCount / (double)(count):##.##} %");
            Console.WriteLine($"Failures :  {100 * passedSum / (double)(examSum):##.##} %");
        }
    }
}
