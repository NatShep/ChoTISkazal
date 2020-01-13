using System;

namespace Dic.Logic.DAL
{
    public class KnowledgeRepository
    {
        public PairModel CreateNew(string word, string translation, string transcription)
        {
            return PairModel.CreatePair(word, translation, transcription);
        }

        public PairModel[] GetWorst(int count)
        {
            throw new NotImplementedException();
        }

        public void UpdateAgingAndRandomization()
        {

        }
    }
}
