using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Dic.AddWords.ConsoleApp.Modes
{
    class StatsMode: IConsoleMode
    {
        public string Name => "Show stats";
        public void Enter(NewWordsService service)
        {
            var allWords = service.GetAll();

            Console.WriteLine($"Context phrases count = {service.GetContextPhraseCount()}");
            Console.WriteLine($"Words count = {allWords.Length}");

            var groups = allWords
                .GroupBy(s => s.State)
                .OrderBy(s => (int)s.Key)
                .Select(s => new { state = s.Key, count = s.Count() });

            var doneCount = 0;
            foreach (var group in groups)
            {
                Console.WriteLine($"{group.state} {group.count}");
                if (group.state == LearningState.Done)
                    doneCount = group.count;
            }
            Console.WriteLine($"Done: {(doneCount * 100 / allWords.Length)}%");
            Console.WriteLine($"Unknown: {allWords.Length - doneCount} words");
        }
    }
}
