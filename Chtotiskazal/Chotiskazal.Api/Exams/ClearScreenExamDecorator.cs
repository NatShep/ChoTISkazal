using System;
using Chotiskazal.Logic.Services;
using Dic.Logic.DAL;

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
        public ExamResult Pass(NewWordsService service, PairModel word, PairModel[] examList) 
            => _origin.Pass(service, word, examList);
    }
}
