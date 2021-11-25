using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class EngChooseWordInPhraseQuestion : IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Eng Choose word in phrase";

        public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word,
            UserWordModel[] examList)
        {
            if (!word.Examples.Any())
                return QuestionResult.Impossible;

            var phrase = word.GetRandomExample();

            var replaced = phrase.OriginPhrase.Replace(phrase.OriginWord, "\\.\\.\\.");

            if (replaced == phrase.OriginPhrase)
                return QuestionResult.Impossible;
            
            var variants = examList
                .Where(p => !p.Examples.Select(e=>e.TranslatedPhrase)
                    .Any(t=>t.AreEqualIgnoreCase(phrase.TranslatedPhrase)))
                .Select(e => e.Word)
                .Shuffle()
                .Take(5)
                .Append(phrase.OriginWord)
                .Shuffle()
                .ToArray();

            var _ = await chat.SendMarkdownMessageAsync(
                QuestionMarkups.TranslatesAsTemplate(
                    phrase.TranslatedPhrase, 
                    chat.Texts.translatesAs, 
                    replaced, 
                    chat.Texts.ChooseMissingWord+":")
                , InlineButtons.CreateVariants(variants));

            var choice = await chat.TryWaitInlineIntKeyboardInput();
            if (choice == null)
                return QuestionResult.RetryThisQuestion;

            if (variants[choice.Value].AreEqualIgnoreCase(word.Word))
                return QuestionResult.Passed(chat.Texts.Passed1Markdown, 
                    chat.Texts);
            
            return QuestionResult.Failed($"{chat.Texts.OriginWas}:\r\n*\"{phrase.OriginPhrase}\"*", 
                chat.Texts);
        }
    }
}