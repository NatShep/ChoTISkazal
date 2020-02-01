using System;
using System.Linq;
using Dic.Logic;
using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Chotiskazal.App.Exams
{
    public class RuChooseExam: IExam
    {
        public string Name => "RuChoose";

        public ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList)
        {
            var variants = examList.Randomize().Select(e => e.OriginWord).ToArray();

            Console.WriteLine("=====>   " + word.Translation + "    <=====");

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

            if (variants[selectedIndex - 1] == word.OriginWord)
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }
            service.RegistrateFailure(word);

            return ExamResult.Failed;

        }
    }
}