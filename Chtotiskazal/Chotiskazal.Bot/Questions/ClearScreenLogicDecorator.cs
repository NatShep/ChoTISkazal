using System.Threading.Tasks;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions;

public class ClearScreenLogicDecorator:IQuestionLogic {

    private readonly IQuestionLogic _origin;


    public ClearScreenLogicDecorator(IQuestionLogic origin)=> _origin = origin;

    public Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) 
        => _origin.Pass(chat, word, examList);
}