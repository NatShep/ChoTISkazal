using System;
using System.Collections.Generic;
using System.Text;
using Chotiskazal.LogicR.yapi;

namespace Chotiskazal.DAL
{
    public class Phrase
    {
        public Phrase(string enPhrase, string ruTranslate)
        {
            EnPhrase = enPhrase;
            RuTranslate = ruTranslate;
        }

        public Phrase(int id, string enPhrase, string ruTranslate)
            : this(enPhrase, ruTranslate)
        {
            PairId = id;
        }

        public int Id { get; set; }
        public int PairId { get; set; }
        public string EnPhrase { get; set; }
        public string RuTranslate { get; set; }
        
        
       
    }
    
   
    
}
