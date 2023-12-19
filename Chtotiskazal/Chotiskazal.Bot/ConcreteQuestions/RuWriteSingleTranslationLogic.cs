using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class RuWriteSingleTranslationLogic : IQuestionLogic {
    private readonly LocalDictionaryService _localDictionaryService;

    public RuWriteSingleTranslationLogic(LocalDictionaryService localDictionaryService) {
        _localDictionaryService = localDictionaryService;
    }

    public QuestionInputType InputType => QuestionInputType.NeedsEnInput;

    public Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) =>
        RuWriteQuestionHelper.PassRuWriteQuestion(
            chat, word, word.RuTranslations.GetRandomItemOrNull().Word, _localDictionaryService);
}