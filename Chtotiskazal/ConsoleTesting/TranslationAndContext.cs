
using Chotiskazal.DAL;
using Chotiskazal.LogicR;

namespace Chotiskazal.ConsoleTesting
{
    public class TranslationAndContext
    {
        public TranslationAndContext(string origin, string translation, string transcription, Phrase[] phrases)
        {
            Origin = origin;
            Translation = translation;
            Transcription = transcription;
            Phrases = phrases;
        }

        public string Origin { get; }

        public string Translation { get; }
        public string Transcription { get; }

        public Phrase[] Phrases { get; }
    }
}