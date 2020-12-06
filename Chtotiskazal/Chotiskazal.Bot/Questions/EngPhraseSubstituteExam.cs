using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.Questions
{
    public class EngPhraseSubstituteExam: IExam
    {
        public bool NeedClearScreen => false;
        public string Name => "Eng phrase substitute";
        public async Task<ExamResult> Pass(ChatIO chatIo, UsersWordsService service, UserWordModel word, UserWordModel[] examList)
        {
            if (!word.Phrases.Any())
                return ExamResult.Impossible;

            var phrase   =  word.GetRandomExample();
            
            var allWordsWithPhraseOfSimilarTranslate = examList
                .SelectMany(e => e.Phrases)
                .Where(p => string.Equals(p.TranslatedPhrase, phrase.TranslatedPhrase,StringComparison.InvariantCultureIgnoreCase))
                .Select(e=>e.OriginWord)
                .ToList();
            
            var replaced =  phrase.OriginPhrase.Replace(phrase.OriginWord, "...");
            if (replaced == phrase.OriginPhrase)
                return ExamResult.Impossible;
            
            var sb = new StringBuilder();
            
            sb.AppendLine($"\"{phrase.TranslatedPhrase}\"");
            sb.AppendLine($" translated as ");
            sb.AppendLine($"\"{replaced}\"");
            sb.AppendLine();
            sb.AppendLine($"Enter missing word: ");
            await chatIo.SendMessageAsync(sb.ToString());
            while (true)
            {
                var enter = await chatIo.WaitUserTextInputAsync();
                if (string.IsNullOrWhiteSpace(enter))
                    continue;
                if (allWordsWithPhraseOfSimilarTranslate.Contains(enter.ToLower().Trim()))
                {
                    await service.RegisterSuccess(word);
                    return ExamResult.Passed;
                }

                await chatIo.SendMessageAsync($"Origin phrase was \"{phrase.OriginPhrase}\"");
                return ExamResult.Failed;
            }
        }
    }
}
