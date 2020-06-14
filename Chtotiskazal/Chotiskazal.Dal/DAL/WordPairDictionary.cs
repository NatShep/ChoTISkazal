using System;
using System.Collections.Generic;
using System.Text;

namespace Chotiskazal.DAL
{
    public class WordDictionary
    {
        public long Id { get; set; }
        public string OriginWord { get; set; }

        //for one Word has one Translation
        //in table WordsWithTranslation we have same words with different ID and different Translate
        // or we can use composite key(Word+Translate).      
        public string Translation { get; set; }
        public string Transcription { get; set; }

 /* ALTERNATIVE 
        //if IsPhrase is true WordSourse=Id of Word in this table
        //if IsPhrase is false WordSourse=null
        public bool IsPhrase { get; set; }
        public int WordSourse { get;set; }
 */

        public List<PhraseDictionary> Phrases { get; set; }
        public string Sourse { get; set; }


       

      
       
    }
}
