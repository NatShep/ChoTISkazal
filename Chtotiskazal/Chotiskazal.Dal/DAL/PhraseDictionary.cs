using System;
using System.Collections.Generic;
using System.Text;

namespace Chotiskazal.DAL
{
    public class PhraseDictionary
    {
        public long Id { get; set; }
        public string OriginPhrase { get; set; }
        public string Translate { get; set; }
        public int WorId { get; set; }
        

    }
}
