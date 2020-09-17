using System;

namespace Dic.Logic.DAL
{
    public class Phrase
    {
        public string OriginWord { get; set; }
        public string Origin { get; set; }
        public string Trans { get; set; }
        public string TranslationWord { get; set; }
        public DateTime Created { get; set; }
        public string Translation { get; set; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(OriginWord);
    }
}