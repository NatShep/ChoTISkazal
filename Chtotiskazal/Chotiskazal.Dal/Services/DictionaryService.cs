using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Dic.Logic.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chotiskazal.Dal.Services
{
    class DictionaryService
    {
        private readonly DictionaryRepository _dicRepository;

        public DictionaryService( DictionaryRepository repository)
        {
            _dicRepository = repository;
        }
        //Add word with all translate as pairs(word+translate) 
        public void AddNewWordPairToDictionary(WordWithTranslation word)
        {
            foreach (var translate in word.Translations )
            {
                _dicRepository.AddWordPair(new WordPairDictionary(word.Origin,word.Transcription, translate, word.Sourse));
            }
        }
        //Add Phrases for Pair(word+translate)
        public void AddPhrasesForWordPair(int pairId, Phrase[]  phrases)
        {
            _dicRepository.AddPhrases(_dicRepository.GetWordPairOrNull(pairId),phrases)
        }
    }
}
