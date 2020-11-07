using Chotiskazal.DAL;
using Chotiskazal.LogicR.yapi;

namespace Chotiskazal.DAL.ModelsForApi
{
    public class TranslationAndContextR
    {
        public TranslationAndContextR(int id, string enWord, string ruWord, string transcription, Phrase[] phrases)
        {
            IdInDB = id;
            EnWord = enWord;
            RuWord = ruWord;
            Transcription = transcription;
            Phrases = phrases;
        }

        //maybe nullable? 
        public int IdInDB { get; }
        public string EnWord { get; }
        public string RuWord { get; }
        public string Transcription { get; }

        public int PhraseCount  => Phrases.Length;
        
        // возможно имеет смысл сделать без фраз. А потом подгружать только нужное
        public Phrase[] Phrases { get; }
    }
}