using System;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;

namespace Chotiskazal.ApI.Exams
{
    public class ClearScreenExamDecorator: IExam
    {
        public bool NeedClearScreen => true;

        private readonly IExam _origin;

        public ClearScreenExamDecorator(IExam origin)
        {
            _origin = origin;
        }

        public string Name => "Clean "+ _origin.Name;
        public ExamResult Pass(ExamService service,UserWordForLearning word, UserWordForLearning[] examList) 
            => _origin.Pass(service, word, examList);
    }
}
