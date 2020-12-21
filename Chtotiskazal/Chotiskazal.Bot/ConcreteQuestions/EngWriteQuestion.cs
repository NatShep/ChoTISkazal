using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class EngWriteQuestion : IQuestion
    {
        private readonly DictionaryService _dictionaryService;

        public EngWriteQuestion(DictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }
        public bool NeedClearScreen => false;

        public string Name => "Eng Write";

        public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word,
            UserWordModel[] examList)
        {
            var translations = word.TextTranslations.ToArray();
            
            var minCount = translations.Min(t => t.Count(c => c == ' '));
            if (minCount > 0 && word.AbsoluteScore < minCount * WordLeaningGlobalSettings.FamiliarWordMinScore)
                return QuestionResult.Impossible;

            await chat.SendMarkdownMessageAsync($"\\=\\=\\=\\=\\=\\>   *{word.Word}*    \\<\\=\\=\\=\\=\\=\r\n" +
                                                $"`{chat.Texts.WriteTheTranslation}`");
            var translation = await chat.WaitUserTextInputAsync();
           
            if (string.IsNullOrEmpty(translation))
                return QuestionResult.RetryThisQuestion;

            var (text,comparation) =  translations.GetClosestTo(translation.Trim());
            
            switch (comparation)
            {
                case StringsCompareResult.Equal:
                    return QuestionResult.Passed(chat.Texts);
                case StringsCompareResult.SmallMistakes:
                    await chat.SendMessageAsync(chat.Texts.YouHaveATypoLetsTryAgainMarkdown(text));
                    return QuestionResult.RetryThisQuestion;
                case StringsCompareResult.BigMistakes:
                    return QuestionResult.Failed(chat.Texts.FailedMistakenMarkdown(text), 
                        chat.Texts);
            }
            var allMeaningsOfWord = await _dictionaryService.GetAllTranslationWords(word.Word);
            var (otherMeaning, otherComparation) = allMeaningsOfWord.GetClosestTo(translation);
            if (otherComparation == StringsCompareResult.Equal) 
            {
                await chat.SendMessageAsync(
                    $"{chat.Texts.OutOfScopeTranslation}: " +
                    word.AllTranslationsAsSingleString);
                return QuestionResult.RetryThisQuestion;
            }
            if (otherComparation == StringsCompareResult.SmallMistakes)
            {
                await chat.SendMessageAsync(
                    chat.Texts.OutOfScopeWithCandidate(otherMeaning)+": "+
                    word.AllTranslationsAsSingleString);
                return QuestionResult.RetryThisQuestion;
            }

            return QuestionResult.Failed(
                chat.Texts.FailedTranslationWasMarkdown +$" '{word.AllTranslationsAsSingleString}'", 
                chat.Texts);
        }
    }
}