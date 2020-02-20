using System;
using System.Collections.Generic;
using System.Linq;
using Chotiskazal.Logic.Services;
using Dic.Logic;
using Dic.Logic.DAL;

namespace Chotiskazal.App.Modes
{
    public class AddPhraseMode: IConsoleMode
    {
        public string Name => "Add phrases";
        public void Enter(NewWordsService service)
        {
            var allWords = service.GetAll().Select(s=>s.OriginWord.ToLower().Trim()).ToHashSet();

            var allPhrases = service.GetAllPhrases();
            List<Phrase> searchedPhrases = new List<Phrase>();
            int endings = 0;
            foreach (var phrase in allPhrases)
            {
                var phraseText = phrase.Origin;
                int count = 0;
                int endingCount = 0;
                foreach (var word in phraseText.Split(new[]{' ',','}))
                {
                    
                    var lowerWord = word.Trim().ToLower();
                    if (allWords.Contains(lowerWord))
                        count++;
                    else if (word.EndsWith('s'))
                    {
                        var withoutEnding = lowerWord.Remove(lowerWord.Length - 1);
                        if (allWords.Contains(withoutEnding))
                            endingCount++;
                    }
                    else if (word.EndsWith("ed"))
                    {
                        var withoutEnding = lowerWord.Remove(lowerWord.Length - 2);

                        if (allWords.Contains(withoutEnding))
                            endingCount++;
                    }
                    else if (word.EndsWith("ing"))
                    {
                        var withoutEnding = lowerWord.Remove(lowerWord.Length - 3);

                        if (allWords.Contains(withoutEnding))
                            endingCount++;
                    }
                    if (count + endingCount > 1 )
                    {
                        searchedPhrases.Add(phrase);
                        if (endingCount > 0)
                        {
                            endings++;
                        }
                        if(count+endingCount>2)
                            Console.WriteLine(phraseText);
                        break;

                    }
                }
            }

            var first10 = searchedPhrases.Randomize().Take(10);
            foreach (var phrase in first10)
            {
                Console.WriteLine("Adding "+ phrase.Origin);
                service.SaveForExams(phrase.Origin,  phrase.Translation, new []{phrase.Translation}, null);
                service.Remove(phrase);
            }
            Console.WriteLine($"Found: {searchedPhrases.Count}+{endings}");
        }
    }
}