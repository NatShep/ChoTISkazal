using System;
using Chotiskazal.Logic.Services;
using Dic.Logic.DAL;

namespace Chotiskazal.App.Exams
{
    public class ClearScreenExamDecorator: IExam
    {
        private readonly IExam _origin;

        public ClearScreenExamDecorator(IExam origin)
        {
            _origin = origin;
        }

        public string Name => _origin.Name;
        public ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList)
        {
            Console.WriteLine("Press any key to clear the screen");
            Console.ReadKey();
            Console.Clear();
            return _origin.Pass(service, word, examList);
        }
    }
}
