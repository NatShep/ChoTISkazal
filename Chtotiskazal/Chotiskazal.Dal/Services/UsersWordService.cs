using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phrase = Chotiskazal.DAL.Phrase;

namespace Chotiskazal.Dal.Services
{
    public class UsersWordService
    {
        private readonly UserWordsRepo _UserWordRepository;
        private readonly DictionaryRepository _dicRepository;
        private readonly ExamsAndMetricsRepo _metricRepository;
        
        public UsersWordService(UserWordsRepo repository, DictionaryRepository dictionaryRepository, ExamsAndMetricsRepo examsAndMetrics)
        {
            _UserWordRepository = repository;
            _dicRepository = dictionaryRepository;
            _metricRepository = examsAndMetrics;
        }
        
        public int SavePairToUser(WordDictionary pair, int userId)
        {
            return _UserWordRepository.SaveToUserDictionary(pair, userId);
        }

        public int SavePairToUser(Phrase phrase, int userId)
        {
            return 1;
        }

        public string[] GetAllUserTranslatesForWord(User user, int id)
        {
            return _UserWordRepository.GetAllTranslateForUser(user, id);
        }
    //    public LogicR.Phrase[] GetAllPhrasesForWord(User user, string word)
      //  {
        //    return new LogicR.Phrase[0];
     //   }
        public void AddWordToUserCollection(User user, int wordId)
        {
            //Before adding wordToUserCollection, check that this word is in WordPairDictionary
            //Create new record in questionMetric Table for this word and User
            //Add (UserId, MetricId,WordId, timeOfCreated) toUserPair Table
        }
        public void DeleteWordFromUserCollection(User user, int wordId)
        {

        }

        public UsersPair GetPairByWordOrNull(User user, string word)
        {
            //check ru or eng word?
            //find Pair
            return null;
        }
        public UsersPair[] GetPairsForLearning(User user, int count)
        {
            return _UserWordRepository.GetWorstForUser(user,count);
         
        }
        public UsersPair[] GetPairsForTests(User user, int count, int learnRate)
        {

            return _UserWordRepository.GetWordsForUserTests(count, learnRate, user);
               
        }
        public UsersPair[] GetWorstPairs(User user, int count)
        {
            return null;
        }

    
    }
}
