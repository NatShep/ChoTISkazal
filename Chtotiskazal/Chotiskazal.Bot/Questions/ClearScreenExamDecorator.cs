using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class ClearScreenExamDecorator:IExam {
        public bool NeedClearScreen => true;

        private readonly IExam _origin;

        public ClearScreenExamDecorator(IExam origin)=> _origin = origin;

        public string Name => "Clean "+ _origin.Name;
        public Task<ExamResult> Pass(ChatIO chatIo, UserWordModel word, UserWordModel[] examList) 
            => _origin.Pass(chatIo, word, examList);
    }
}
