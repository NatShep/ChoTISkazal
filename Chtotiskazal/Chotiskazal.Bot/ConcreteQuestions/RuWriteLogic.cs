using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class RuWriteLogic : IQuestionLogic {
    private readonly LocalDictionaryService _localDictionaryService;

    public RuWriteLogic(LocalDictionaryService localDictionaryService) {
        _localDictionaryService = localDictionaryService;
    }

    public QuestionInputType InputType => QuestionInputType.NeedsEnInput;

    public Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) =>
        RuWriteQuestionHelper.PassRuWriteQuestion(
            chat,
            word,
            word.AllTranslationsAsSingleString,
            _localDictionaryService);
}