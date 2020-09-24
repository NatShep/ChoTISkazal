using Chotiskazal.DAL;
using Chotiskazal.LogicR.yapi;

namespace Chotiskazal.App
{
    public class TranslationAndContext
    {
        public TranslationAndContext(string enWord, string ruWord, string transcription, Phrase[] phrases)
        {
            EnWord = enWord;
            RuWord = ruWord;
            Transcription = transcription;
            Phrases = phrases;
        }

        public int? IdInDB { get; } = null;
        public string EnWord { get; }
        public string RuWord { get; }
        public string Transcription { get; }

        public int PhraseCount  => Phrases.Length;
        
        // возможно имеет смысл сделать без фраз. А потом подгружать только нужное
        public Phrase[] Phrases { get; }
    }
}