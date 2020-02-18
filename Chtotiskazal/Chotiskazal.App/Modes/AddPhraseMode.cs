using System;
using System.Collections.Generic;
using System.Linq;
using Chotiskazal.Logic.Services;
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
            foreach (var phrase in allPhrases)
            {
                var phraseText = phrase.Origin;
                int count = 0;
                foreach (var word in phraseText.Split(new[]{' ',','}))
                {
                    
                    var lowerWord = word.Trim().ToLower();
                    if (allWords.Contains(lowerWord))
                        count++;
                    else if (word.EndsWith('s'))
                    {
                        if (allWords.Contains(lowerWord + 's'))
                            count++;
                    }
                    else if (word.EndsWith("ed"))
                    {
                        if (allWords.Contains(lowerWord + "ed"))
                            count++;
                    }
                    else if (word.EndsWith("ing"))
                    {
                        if (allWords.Contains(lowerWord + "ing"))
                            count++;
                    }
                    if (count > 1)
                    {
                        searchedPhrases.Add(phrase);
                        Console.WriteLine(phraseText);
                        break;
                    }
                }
            }
        }
    }
}