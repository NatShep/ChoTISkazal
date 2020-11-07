using System.Threading.Tasks;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;

namespace Chotiskazal.Bot.Questions
{
    public class ClearScreenExamDecorator:IExam
    {
        public bool NeedClearScreen => true;

        private readonly IExam _origin;

        public ClearScreenExamDecorator(IExam origin)
        {
            _origin = origin;
        }

        public string Name => "Clean "+ _origin.Name;
        public Task<ExamResult> Pass(Chat chat, ExamService service, UserWordForLearning word, UserWordForLearning[] examList) 
            => _origin.Pass(chat, service, word, examList);
    }
}
