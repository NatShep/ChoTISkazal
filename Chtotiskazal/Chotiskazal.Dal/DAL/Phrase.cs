using System;
using System.Collections.Generic;
using System.Text;
using Chotiskazal.LogicR.yapi;

namespace Chotiskazal.DAL
{
    public class Phrase
    {
        public Phrase(){}
        
        public Phrase(string enPhrase, string phraseRuTranslate)
        {
            EnPhrase = enPhrase;
            PhraseRuTranslate = phraseRuTranslate;
        }

        public Phrase(int id, string enWord, string ruWord, string enPhrase, string ruTranslate)
            : this(enPhrase, ruTranslate)
        {
            EnWord =enWord;
            WordTranslate = ruWord;
            PairId = id;
        }

        public int Id { get; set; }
        public int PairId { get; set; }
        public string EnWord { get; set; }
        public string WordTranslate { get; set; }
        public string EnPhrase { get; set; }
        public string PhraseRuTranslate { get; set; }
        
        
       
    }
    
   
    
}
