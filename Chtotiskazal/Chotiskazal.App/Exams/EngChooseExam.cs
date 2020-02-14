using System;
using System.Linq;
using Chotiskazal.Logic.Services;
using Dic.Logic;
using Dic.Logic.DAL;

namespace Chotiskazal.App.Exams
{
    public class EngChooseExam : IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose";

        public ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList)
        {
            var variants = examList.Randomize().Select(e => e.Translation).ToArray();

            Console.WriteLine("=====>   " + word.OriginWord + "    <=====");

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

            if (variants[selectedIndex - 1] == word.Translation)
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }
            service.RegistrateFailure(word);
            return ExamResult.Failed;

        }
    }
}