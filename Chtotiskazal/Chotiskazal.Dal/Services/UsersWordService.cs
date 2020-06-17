using Chotiskazal.Dal.Repo;
using Chotiskazal.DAL;
using Chotiskazal.LogicR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public string[] GetAllUserTranslatesForWord(User user, string word)
        {
            return _UserWordRepository.GetAllTranslateForUser(user, word);
        }
        public Phrase[] GetAllPhrasesForWord(User user, string word)
        {
            return new Phrase[0];
        }
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
        public UsersPair[] GetBestPairsForLearning(User user, int count, int maxTranslationSize)
        {
            var fullPairs = _UserWordRepository.GetWorstForUser(count);
          /*  foreach (var pairModel in fullPairs)
            {
                var translations = pairModel.GetTranslations.ToArray();
                if (translations.Length <= maxTranslationSize)
                    continue;
                var usedTranslations = translations.Randomize().Take(maxTranslationSize).ToArray();
                pairModel.SetTranslations(usedTranslations);
                for (int i = 0; i < pairModel.Phrases.Count; i++)
                {
                    var phrase = pairModel.Phrases[i];
                    if (!usedTranslations.Contains(phrase.Translation))
                        pairModel.Phrases.RemoveAt(i);
                }
            }*/
                return fullPairs;
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
