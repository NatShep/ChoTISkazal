using System;
using System.Collections.Generic;
using System.Text;

namespace Chotiskazal.DAL
{
    public class PhraseDictionary
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public string EnPhrase { get; set; }
        public string RuTranslate { get; set; }
    }
}
