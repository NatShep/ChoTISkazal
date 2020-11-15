using System.Threading.Tasks;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;

namespace Chotiskazal.Bot.Questions
{
    //TODO Do we need this in Telgram?
    public class ClearScreenExamDecorator:IExam {
        public bool NeedClearScreen => true;

        private readonly IExam _origin;

        public ClearScreenExamDecorator(IExam origin)
        {
            _origin = origin;
        }

        public string Name => "Clean "+ _origin.Name;
        public Task<ExamResult> Pass(ChatIO chatIo, ExamService service, UserWordForLearning word, UserWordForLearning[] examList) 
            => _origin.Pass(chatIo, service, word, examList);
    }
}
