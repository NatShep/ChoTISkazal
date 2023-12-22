using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class RuWriteScenario : IQuestionScenario {
    private readonly LocalDictionaryService _localDictionaryService;

    public RuWriteScenario(LocalDictionaryService localDictionaryService) {
        _localDictionaryService = localDictionaryService;
    }

    public QuestionInputType InputType => QuestionInputType.NeedsEnInput;

    public Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) =>
        RuWriteQuestionScenarioHelper.PassRuWriteQuestion(
            chat,
            word,
            word.AllTranslationsAsSingleString,
            _localDictionaryService);
}