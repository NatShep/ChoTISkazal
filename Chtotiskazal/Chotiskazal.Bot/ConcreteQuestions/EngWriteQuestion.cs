using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
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

        public async Task<QuestionResultMarkdown> Pass(ChatRoom chat, UserWordModel word,
            UserWordModel[] examList)
        {
            var translations = word.TextTranslations.ToArray();
            
            var minCount = translations.Min(t => t.Count(c => c == ' '));
            if (minCount > 0 && word.AbsoluteScore < minCount * WordLeaningGlobalSettings.FamiliarWordMinScore)
                return QuestionResultMarkdown.Impossible;


            await chat.SendMarkdownMessageAsync(QuestionMarkups.TranslateTemplate(word.Word, chat.Texts.WriteTheTranslation));
            var entry = await chat.WaitUserTextInputAsync();
           
            if (string.IsNullOrEmpty(entry))
                return QuestionResultMarkdown.RetryThisQuestion;
            
            if (!entry.IsRussian())
            {
                await chat.SendMessageAsync(chat.Texts.RussianInputExpected);
                return QuestionResultMarkdown.RetryThisQuestion;
            }
            
            var (text,comparation) =  translations.GetClosestTo(entry.Trim());
            
            switch (comparation)
            {
                case StringsCompareResult.Equal:
                    return QuestionResultMarkdown.Passed(chat.Texts);
                case StringsCompareResult.SmallMistakes:
                    await chat.SendMarkdownMessageAsync(chat.Texts.YouHaveATypoLetsTryAgainMarkdown(text));
                    return QuestionResultMarkdown.RetryThisQuestion;
                case StringsCompareResult.BigMistakes:
                    return QuestionResultMarkdown.Failed(MarkdownObject.Escaped(chat.Texts.FailedMistaken(text)), 
                        chat.Texts);
            }
            var allMeaningsOfWord = await _localDictionaryService.GetAllTranslationWords(word.Word);
            var (otherMeaning, otherComparation) = allMeaningsOfWord.GetClosestTo(entry);
            if (otherComparation == StringsCompareResult.Equal) {
                await chat.SendMarkdownMessageAsync(
                    MarkdownObject.Escaped($"{chat.Texts.OutOfScopeTranslation}: ") +
                    MarkdownObject.Escaped(word.AllTranslationsAsSingleString).ToSemiBold());
                return QuestionResultMarkdown.RetryThisQuestion;
            }
            if (otherComparation == StringsCompareResult.SmallMistakes) {
                await chat.SendMarkdownMessageAsync(
                    MarkdownObject.Escaped($"{chat.Texts.OutOfScopeWithCandidate(otherMeaning)}:") +
                    MarkdownObject.Escaped($"\"{word.AllTranslationsAsSingleString}\"").ToSemiBold());
                return QuestionResultMarkdown.RetryThisQuestion;
            }

            return QuestionResultMarkdown.Failed(
                MarkdownObject.Escaped(chat.Texts.FailedTranslationWas).AddNewLine()+
                MarkdownObject.Escaped($"\"{word.AllTranslationsAsSingleString}\"").ToSemiBold(),
                chat.Texts);
        }
    }
}