using System.Threading.Tasks;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.Questions;

public class ClearScreenScenarioDecorator: IQuestionScenario {

    private readonly IQuestionScenario _origin;

    public ClearScreenScenarioDecorator(IQuestionScenario origin)=> _origin = origin;

    public QuestionInputType InputType => _origin.InputType;

    public Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) 
        => _origin.Pass(chat, word, examList);
}