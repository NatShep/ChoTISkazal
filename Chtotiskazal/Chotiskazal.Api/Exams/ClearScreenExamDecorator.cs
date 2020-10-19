using System;
using Chotiskazal.Api.Models;
using Chotiskazal.ConsoleTesting.Services;

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
        public ExamResult Pass(ExamService service, WordForLearning word, WordForLearning[] examList) 
            => _origin.Pass(service, word, examList);
    }
}
