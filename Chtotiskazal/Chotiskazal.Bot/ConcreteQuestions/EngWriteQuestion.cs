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
        private readonly LocalDictionaryService _localDictionaryService;

        public EngWriteQuestion(LocalDictionaryService localDictionaryService)
        {
            _localDictionaryService = localDictionaryService;
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
                                                $"{chat.Texts.WriteTheTranslationMarkdown}");
            var translation = await chat.WaitUserTextInputAsync();
           
            if (string.IsNullOrEmpty(translation))
                return QuestionResult.RetryThisQuestion;

            var (text,comparation) =  translations.GetClosestTo(translation.Trim());
            
            switch (comparation)
            {
                case StringsCompareResult.Equal:
                    return QuestionResult.Passed(chat.Texts);
                case StringsCompareResult.SmallMistakes:
                    await chat.SendMarkdownMessageAsync(chat.Texts.YouHaveATypoLetsTryAgainMarkdown(text));
                    return QuestionResult.RetryThisQuestion;
                case StringsCompareResult.BigMistakes:
                    return QuestionResult.Failed(chat.Texts.FailedMistakenMarkdown(text), 
                        chat.Texts);
            }
            var allMeaningsOfWord = await _localDictionaryService.GetAllTranslationWords(word.Word);
            var (otherMeaning, otherComparation) = allMeaningsOfWord.GetClosestTo(translation);
            if (otherComparation == StringsCompareResult.Equal) 
            {
                await chat.SendMarkdownMessageAsync(
                    $"{chat.Texts.OutOfScopeTranslationMarkdown}: " +
                    $"*{word.AllTranslationsAsSingleString}*");
                return QuestionResult.RetryThisQuestion;
            }
            if (otherComparation == StringsCompareResult.SmallMistakes)
            {
                await chat.SendMarkdownMessageAsync(
                    chat.Texts.OutOfScopeWithCandidateMarkdown(otherMeaning)+": "+
                    "*\""+word.AllTranslationsAsSingleString+"\"*");
                return QuestionResult.RetryThisQuestion;
            }

            return QuestionResult.Failed(
                chat.Texts.FailedTranslationWasMarkdown +$"\r\n*\"{word.AllTranslationsAsSingleString}\"*", 
                chat.Texts);
        }
    }
}