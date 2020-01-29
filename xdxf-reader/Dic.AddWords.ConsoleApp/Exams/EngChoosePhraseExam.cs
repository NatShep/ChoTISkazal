using System;
using System.Linq;
using Dic.Logic;
using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Dic.AddWords.ConsoleApp
{
    public class EngChoosePhraseExam : IExam
    {
        public string Name => "Eng Choose Phrase";

        public ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList)
        {
            if (!word.Phrases.Any())
                return new EngChooseExam().Pass(service, word, examList);
            
            var targetPhrase = word.Phrases.GetRandomItem();

            var other = examList.SelectMany(e => e.Phrases)
                .Where(p => !string.IsNullOrWhiteSpace(p?.Origin) && p!= targetPhrase)
                .Take(9)
                .ToArray();

            if(!other.Any())
                return new EngChooseExam().Pass(service, word, examList);

            var variants = other
                .Append(targetPhrase)
                .Randomize()
                .Select(e => e.Translation)
                .ToArray();
            
            Console.WriteLine("=====>   " + targetPhrase.Origin + "    <=====");

            for (int i = 1; i <= variants.Length; i++)
            {
                Console.WriteLine($"{i}: " + variants[i - 1]);
            }

            Console.Write("Choose the translation: ");

            var selected = Console.ReadLine();
            if (selected.ToLower().StartsWith("e"))
                return ExamResult.Exit;

            if (!int.TryParse(selected, out var selectedIndex) || selectedIndex > variants.Length ||
                selectedIndex < 1)
                return ExamResult.Retry;

            if (variants[selectedIndex - 1] == targetPhrase.Translation)
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }
            service.RegistrateFailure(word);
            return ExamResult.Failed;

        }
    }
}