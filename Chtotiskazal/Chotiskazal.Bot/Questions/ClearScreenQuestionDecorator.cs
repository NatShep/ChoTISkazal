using System.Threading.Tasks;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions
{
    public class ClearScreenQuestionDecorator:IQuestion {
        public bool NeedClearScreen => true;

        private readonly IQuestion _origin;

        public ClearScreenQuestionDecorator(IQuestion origin)=> _origin = origin;

        public string Name => "Clean "+ _origin.Name;
        public Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) 
            => _origin.Pass(chat, word, examList);
    }
}
