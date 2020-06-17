using Chotiskazal.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chotiskazal.DAL
{
    // хотим ли мы разделять слово-значение1, слово-значение2
    // или слово - все значения, помеченные юзером.
    public class UsersPair
    {

        public int Id { get; set; }
        public DateTime Created { get; set; }
        public int UserId { get; set; }

        /*
         //Find words by UnicName(Word+Translate) in WordDictionary
         // if IsPhrase is True find words by UnicName(Word+Translate) in PhraseDictionary
         public string Word { get; set; }
         public string Translate { get; set; }
         public bool IsPhrase { get; set; }

         // if IsPhrase is True, Phrases=null
         public List<PhraseDictionary> Phrases { get; set; }

         */

        public int WordId { get; set; }

        public int MetricId { get; set; }
    }
}

