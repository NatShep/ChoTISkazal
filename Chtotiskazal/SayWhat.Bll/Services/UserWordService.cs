using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace SayWhat.Bll.Services
{
    public class UsersWordsService
    {
        private readonly UserWordsRepo _userWordsRepository;

        public UsersWordsService(UserWordsRepo repository) => _userWordsRepository = repository;

        public Task AddWordToUserCollectionAsync(User user, UserWordModel model) =>
             _userWordsRepository.Add(model.Entity);

        public async Task<IEnumerable<UserWordModel>> GetWorstForUserWithPhrasesAsync(User user, int count)
        {
            var words =await _userWordsRepository.GetWorstLearned(user, count);
            return words.Select(w => new UserWordModel(w));
        }

        public async Task<UserWordModel[]> GetWorstWordsForUserAsync(User user, int maxCount, int learnRate) =>
            (await _userWordsRepository.GetWorstLearned(user,  maxCount, learnRate))
            .Select(t=> new UserWordModel(t))
            .ToArray();


        public async Task RegisterFailure(UserWordModel userWordForLearning)
        {
            userWordForLearning.OnExamFailed();
            userWordForLearning.UpdateAgingAndRandomization();
            await _userWordsRepository.UpdateScores(userWordForLearning.Entity);
        }

        public async Task UpdateAgingAndRandomizeAsync(User user, int count)
        {
            // not implemented
            /*var words = await _userWordsRepository.GetRandowWords(user, count);
            foreach (var word in words)
            {
                var model = new UserWordModel(word);
                model.UpdateAgingAndRandomization();
                await _userWordsRepository.Update(model.Entity);
            }*/
        }

        public async Task RegisterSuccess(UserWordModel model)
        {
            model.OnExamPassed();
            model.UpdateAgingAndRandomization();
            await _userWordsRepository.UpdateScores(model.Entity);
        }

        public Task<bool> HasWords(User user) => _userWordsRepository.HasAnyFor(user);
        public Task UpdateWord(UserWordModel model) =>
             _userWordsRepository.Update(model.Entity);

        public async Task<UserWordModel> GetWordNullByEngWord(User user, string enWord)
        {
            var word = await _userWordsRepository.GetWordOrDefault(user, enWord);
            if (word == null)
                return null;
            return new UserWordModel(word);
        }
        
        public  Task AddMutualPhrasesToVocabAsync(User user, int maxCount)
        {
            return Task.CompletedTask;
            /*
            //var allWords = (await _usersWordsService.GetAllEnWordsForUserAsync(user)).Select(s => s.ToLower().Trim())
            //    .ToHashSet();
            
            var allWordsForLearning = await _usersWordsService.GetAllUserWords(user);
            var allEngWords = allWordsForLearning.Select(a => a.Word.ToLower().Trim()).ToHashSet();

            var allPhrasesIdForUser = new List<int>();
            
            foreach (var word in allWordsForLearning)
            {
                var phrases = word.GetPhrasesId();
                allPhrasesIdForUser.AddRange(phrases);
            }
            

            var allPhrases =  allWordsForLearning.Select(a=>a.Entity) // await _dictionaryService.FindPhrasesBySomeIdsAsync(allPhrasesIdForUser.ToArray());

            var searchedPhrases = new List<Phrase>();
            var endings = 0;
            foreach (var phrase in allPhrases)
            {
                var phraseText = phrase.EnPhrase;
                var count = 0;
                var endingCount = 0;
                foreach (var word in phraseText.Split(new[] {' ', ','}))
                {
                    var lowerWord = word.Trim().ToLower();
                    if (allEngWords.Contains(lowerWord))
                        count++;
                    else if (word.EndsWith('s'))
                    {
                        var withoutEnding = lowerWord.Remove(lowerWord.Length - 1);
                        if (allEngWords.Contains(withoutEnding))
                            endingCount++;
                    }
                    else if (word.EndsWith("ed"))
                    {
                        var withoutEnding = lowerWord.Remove(lowerWord.Length - 2);

                        if (allEngWords.Contains(withoutEnding))
                            endingCount++;
                    }
                    else if (word.EndsWith("ing"))
                    {
                        var withoutEnding = lowerWord.Remove(lowerWord.Length - 3);

                        if (allEngWords.Contains(withoutEnding))
                            endingCount++;
                    }

                    if (count + endingCount <= 1)
                        continue;
                    searchedPhrases.Add(phrase);
                    if (endingCount > 0)
                    {
                        endings++;
                    }

                    //if (count + endingCount > 2)
                    //    Console.WriteLine(phraseText);
                }
            }

            var firstPhrases = searchedPhrases.Randomize().Take(maxCount);
            foreach (var phrase in firstPhrases)
            {
                Console.WriteLine("Adding " + phrase.EnPhrase);
                var userWord =
                    UserWordForLearning.CreatePair(phrase.EnPhrase, phrase.PhraseRuTranslate, "[]", isPhrase: true);
                await _usersWordsService.AddWordToUserCollectionAsync(userWord);
            }

            Console.WriteLine($"Found: {searchedPhrases.Count}+{endings}");*/
        }

        
        public async Task<UserWordModel[]> GetWordsForLearningWithPhrasesAsync(User user, int count,
            int maxTranslationSize)
        {
            var wordsForLearning = await GetWorstForUserWithPhrasesAsync(user, count);

            foreach (var wordForLearning in wordsForLearning)
            {
                var translations = wordForLearning.GetTranslations().ToArray();
                if (translations.Length <= maxTranslationSize)
                    continue;

                var usedTranslations = translations.Randomize().Take(maxTranslationSize).ToArray();
                wordForLearning.SetTranslation(usedTranslations);

                // Remove Phrases added as learning word 
                /*
                 todo wtf?
                 for (var i = 0; i < wordForLearning.RuPhrases.Count; i++)
                {
                    var phrase = wordForLearning.RuPhrases[i];
                    if (!usedTranslations.Contains(phrase.PhraseRuTranslate))
                        wordForLearning.RuPhrases.RemoveAt(i);
                }*/
            }
            return wordsForLearning.ToArray();
        }
        
        public async Task<UserWordModel[]> GetTestWordsAsync(User user, List<UserWordModel> examsList)
        {
            //TODO изучть по какому принципу получаем RandomRATE. связан ли он с прогрессом подбираемых слов.
            //Или тут вообще рандомные слова будут
            var delta = Math.Min(7, (32 - examsList.Count));
            var testWords = new UserWordModel[0];

            if (delta <= 0)
                return testWords;

            var randomRate = 8 + RandomTools.Rnd.Next(5);
            testWords = await GetWorstWordsForUserAsync(user, delta, randomRate);
            return testWords;
        }
    }
}