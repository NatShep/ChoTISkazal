using System.Threading.Tasks;
using Chotiskazal.Bot.Interface;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions
{
    public class AssemblePhraseQuestion :IQuestion
    {
        public bool NeedClearScreen => false;

        public string Name => "Assemble phrase";

        public async Task<QuestionResultMarkdown> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) 
        {
            if (!word.HasAnyExamples)
                return QuestionResultMarkdown.Impossible;

            var targetPhrase = word.GetRandomExample();

            string shuffled;
            while (true)
            {
                var wordsInExample = targetPhrase.SplitWordsOfPhrase;
                
                if (wordsInExample.Length < 2)
                    return QuestionResultMarkdown.Impossible;

                shuffled = string.Join(" ", wordsInExample.Shuffle());
                if(shuffled!= targetPhrase.OriginPhrase)
                    break;
            }

            await chat.SendMarkdownMessageAsync(
                QuestionMarkups.FreeTemplateMarkdown(
                    chat.Texts.WordsInPhraseAreShuffledWriteThemInOrderMarkdown.AddEscaped(":")
                        .AddNewLine() + MarkdownObject.Escaped(shuffled).ToSemiBold()));
            var entry = await chat.WaitUserTextInputAsync();

            if (entry.IsRussian())
            {
                await chat.SendMessageAsync(chat.Texts.EnglishInputExpected);
                return QuestionResultMarkdown.RetryThisQuestion;
            }
            
            var closeness = targetPhrase.OriginPhrase.CheckCloseness(entry.Trim());
            
            if (closeness== StringsCompareResult.Equal)
                return QuestionResultMarkdown.Passed(chat.Texts);
            
            if (closeness == StringsCompareResult.BigMistakes){
                await chat.SendMessageAsync(chat.Texts.RetryAlmostRightWithTypo);
                return QuestionResultMarkdown.RetryThisQuestion;
            }

            return QuestionResultMarkdown.Failed(MarkdownObject
                                                     .Escaped($"{chat.Texts.FailedOriginExampleWas2Markdown}:")
                                                     .AddNewLine() +
                                                 MarkdownObject.Escaped($"\"{targetPhrase.OriginPhrase}\"")
                                                     .ToSemiBold(),
                chat.Texts);
        }
    }
}