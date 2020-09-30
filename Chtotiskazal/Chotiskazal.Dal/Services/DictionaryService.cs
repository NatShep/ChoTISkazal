﻿using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using Chotiskazal.LogicR.yapi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chotiskazal.Dal.Enums;

namespace Chotiskazal.Dal.Services
{
    public class DictionaryService
    {
        private readonly DictionaryRepository _dicRepository;

        public DictionaryService( DictionaryRepository repository) => _dicRepository = repository;

        // Add Words or Phrases to dictionary
        public int AddNewWordPairToDictionary(string enword, string ruword, string transcription, TranslationSource sourse)
        {
            var word = new WordDictionary(enword,ruword,transcription,sourse);
            return _dicRepository.AddWordPair(word);
        }
        public int AddPhraseForWordPair(int pairId, string enPhrase, string RuTranslate)
        {
            //TODO look if there are some phrases
            //TODO if not, add prases
            //TODO if there is, update phrases
            return _dicRepository.AddPhrase(pairId, enPhrase,RuTranslate);
        }
      
        public string[] GetAllTranslations(string word) => _dicRepository.GetAllTranslate(word);
        
        public WordDictionary GetPairByIdOrNull(int id) => _dicRepository.GetPairByIdOrNull(id);
        
        public WordDictionary GetPairWithPhrasesById(int id) =>  _dicRepository.GetPairWithPhrasesById(id);
        
        public WordDictionary[] GetAllPairsByWord(string word) => _dicRepository.GetAllWordPairsByWord(word);
        
        public Phrase[] GetAllPhrasesByWordPairId(int pairId) => _dicRepository.GetAllPhrasesByPairId(pairId);

        //TODO
        public string GetTranscription(string word) => throw new NotImplementedException();
    }
}
