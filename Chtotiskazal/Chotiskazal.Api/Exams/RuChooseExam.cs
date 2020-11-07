using System;
using System.Linq;
using Chotiskazal.Api.Services;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.DAL.Services;
using Chotiskazal.LogicR;

namespace Chotiskazal.ApI.Exams
{
    public class RuChooseExam: IExam
    {
        public bool NeedClearScreen => false;

        public string Name => "RuChoose";

        public ExamResult Pass(ExamService service, UserWordForLearning word, UserWordForLearning[] examList)
        {
            var variants = examList.Randomize().Select(e => e.EnWord).ToArray();

            Console.WriteLine("=====>   " + word.UserTranslations + "    <=====");

            for (int i = 1; i <= variants.Length; i++)
            {
                Console.WriteLine($"{i}: " + variants[i - 1]);
            }
            
            Console.Write("Choose the translation: ");
                
            var selected = Console.ReadLine();
            if (selected == null || selected.ToLower().StartsWith("e"))
                return ExamResult.Exit;

            if (!int.TryParse(selected, out var selectedIndex) || selectedIndex > variants.Length ||
                selectedIndex < 1)
                return ExamResult.Retry;

            if (variants[selectedIndex - 1] == word.EnWord)
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }
            service.RegistrateFailure(word);

            return ExamResult.Failed;
        }
    }
}