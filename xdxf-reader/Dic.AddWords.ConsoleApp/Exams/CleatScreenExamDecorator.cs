using System;
using System.Collections.Generic;
using System.Text;
using Dic.Logic.DAL;
using Dic.Logic.Services;

namespace Dic.AddWords.ConsoleApp.Exams
{
    public class CleatScreenExamDecorator: IExam
    {
        private readonly IExam _origin;

        public CleatScreenExamDecorator(IExam origin)
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
