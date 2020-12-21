using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
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

        public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) 
        {
            if (!word.HasAnyExamples)
                return QuestionResult.Impossible;

            var targetPhrase = word.GetRandomExample();

            string shuffled;
            while (true)
            {
                var wordsInExample = targetPhrase.SplitWordsOfPhrase;
                
                if (wordsInExample.Length < 2)
                    return QuestionResult.Impossible;

                shuffled = string.Join(" ", wordsInExample.Randomize());
                if(shuffled!= targetPhrase.OriginPhrase)
                    break;
            }

            await chat.SendMarkdownMessageAsync($"_{chat.Texts.WordsInPhraseAreShuffledWriteThemInOrderMarkdown}:_\r\n\r\n*\"" +  shuffled+ "\"*");
            var entry = await chat.WaitUserTextInputAsync();
            entry = entry.Trim();

            if (targetPhrase.OriginPhrase.AreEqualIgnoreCase(entry.Trim()))
                return QuestionResult.Passed(chat.Texts);

            
            return QuestionResult.Failed(
                $"{chat.Texts.FailedOriginExampleWas2Markdown}:\r\n*\"{targetPhrase.OriginPhrase}\"*", 
                chat.Texts);
        }
    }
}