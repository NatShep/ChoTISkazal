using System;
using System.Linq;
using Chotiskazal.Logic.Services;
using Dic.Logic.DAL;

namespace Chotiskazal.App.Exams
{
    public class RuWriteExam : IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng Write";

        public ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList)
        {
            var words = word.OriginWord.Split(',').Select(s => s.Trim());
            var minCount = words.Min(t => t.Count(c => c == ' '));
            if (minCount > 0 && word.PassedScore < minCount * 4)
                return ExamResult.Impossible;

            Console.WriteLine("=====>   " + word.Translation+ "    <=====");

            Console.Write("Write the translation: ");
            var userEntry = Console.ReadLine();
            if (string.IsNullOrEmpty(userEntry))
                return ExamResult.Retry;

            if (words.Any(t => string.Compare(userEntry, t, StringComparison.OrdinalIgnoreCase) == 0))
            {
                service.RegistrateSuccess(word);
                return ExamResult.Passed;
            }
            else
            {
                //search for other translation
                var translationCandidate = service.Get(userEntry.ToLower());
                if (translationCandidate != null)
                {

                    if (translationCandidate.GetTranslations().Any(t1=> word.GetTranslations().Any(t2=> string.CompareOrdinal(t1.Trim(), t2.Trim())==0)))
                    {
                        //translation is correct, but for other word
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"the translation was correct, but the question was about the word '{word.OriginWord}'\r\nlet's try again");
                        Console.ResetColor();
                        return ExamResult.Retry;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"'{userEntry}' translates as {translationCandidate.Translation}");
                        Console.ResetColor();

                        service.RegistrateFailure(word);
                        return ExamResult.Failed;
                    }
                }
                Console.WriteLine("The translation was: " + word.OriginWord);
                service.RegistrateFailure(word);
                return ExamResult.Failed;
            }
        }
    }
}