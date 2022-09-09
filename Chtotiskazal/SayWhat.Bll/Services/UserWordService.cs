using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace SayWhat.Bll.Services
{
    public class UsersWordsService
    {
        private readonly UserWordsRepo _userWordsRepository;
        private readonly  ExamplesRepo _examplesRepo;

        public UsersWordsService(UserWordsRepo repository, ExamplesRepo examplesRepo)
        {
            _userWordsRepository = repository;
            _examplesRepo = examplesRepo;
        }

        public Task AddUserWord(UserWordModel entity) =>
             _userWordsRepository.Add(entity);

        private async Task<IEnumerable<UserWordModel>> GetWorstForUserWithPhrasesAsync(UserModel user, int count)
        {
            var words = await _userWordsRepository.GetWorstLearned(user, count);
            await IncludeExamples(words);
            return words;
        }

        private async Task IncludeExamples(IReadOnlyCollection<UserWordModel> words)
        {
            var ids = new List<ObjectId>();

            foreach (var word in words)
            {
                foreach (var translation in word.RuTranslations)
                {
                    ids.AddRange(translation.Examples.Select(e => e.ExampleId));
                }
            }
        
            var examples = (await _examplesRepo.GetAll(ids)).ToDictionary(e => e.Id);

            foreach (var word in words)
            {
                foreach (var translation in word.RuTranslations)
                {
                    foreach (var example in translation.Examples)
                    {
                        example.ExampleOrNull = examples[example.ExampleId];
                    }
                }
            }
        }

        public async Task<IReadOnlyList<UserWordModel>> GetWordsWithExamples(UserModel user, int maxCount, int minimumQuestionAsked, int  minBestLearnedWords)
        {
            if (maxCount <= 0)
                return new UserWordModel[0];

            var words = await _userWordsRepository.GetLastAsked(user, maxCount, minimumQuestionAsked);
            words.AddRange(await _userWordsRepository.GetWellLearned(user, maxCount/2));
            words.AddRange(await _userWordsRepository.GetBestLearned(user, minBestLearnedWords ));

            await IncludeExamples(words);
            return words;
        }


        public async Task RegisterFailure(UserWordModel userWordModelForLearning)
        {
            userWordModelForLearning.OnQuestionFailed();
            await _userWordsRepository.UpdateMetrics(userWordModelForLearning);
        }
        
        public async Task UpdateCurrentScoreForRandomWords(UserModel user, int count)
        {
            var words = await _userWordsRepository.GetOldestUpdatedWords(user, count);
            foreach (var word in words)
            {
                var scoreBefore = word.Score;
                word.UpdateCurrentScore();
                await _userWordsRepository.UpdateMetrics(word);
                user.OnStatsChangings(word.Score - scoreBefore);
            }
        }

        public async Task RegisterSuccess(UserWordModel model)
        {
            model.OnQuestionPassed();
            await _userWordsRepository.UpdateMetrics(model);
        }

        public Task<bool> HasWordsFor(UserModel user) => _userWordsRepository.HasAnyFor(user);
        public Task UpdateWord(UserWordModel model) =>
             _userWordsRepository.Update(model);
        public Task RemoveWord(UserWordModel model) =>
            _userWordsRepository.Remove(model);

        public Task UpdateWordMetrics(UserWordModel model) =>
            _userWordsRepository.UpdateMetrics(model);
        
        public async Task<UserWordModel> GetWordNullByEngWord(UserModel user, string enWord) 
            => await _userWordsRepository.GetWordOrDefault(user, enWord);

        public  Task AddMutualPhrasesToVocabAsync(UserModel user, int maxCount)
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

        
        public async Task<UserWordModel[]> GetWordsForLearningWithPhrasesAsync(
            UserModel user, 
            int count,
            int maxTranslationSize)
        {
            var wordsForLearning = await GetWorstForUserWithPhrasesAsync(user, count);

            foreach (var wordForLearning in wordsForLearning)
            {
                
                var translations = wordForLearning.TextTranslations.ToArray();
                if (translations.Length <= maxTranslationSize)
                    continue;

                var usedTranslations = translations.Shuffle().Take(maxTranslationSize).ToArray();
                wordForLearning.RuTranslations = usedTranslations.Select(t=>new UserWordTranslation(t)).ToArray();

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

        public async Task<IReadOnlyList<UserWordModel>> AppendAdvancedWordsToExamList(UserModel user, UserWordModel[] learningWords, ExamSettings examSettings, List<UserWordModel> examsList)
        {
            //TODO нормальный нейминг. Это максимальо количество экзаменов
            var advancedlistMaxCount = Math.Min(Rand.RandomIn(examSettings.MinAdvancedQuestionsCount, examSettings.MaxAdvancedQuestionsCount),
                examSettings.MaxExamSize - examsList.Count);
            
            if (advancedlistMaxCount <= examSettings.MinAdvancedQuestionsCount) 
                advancedlistMaxCount = examSettings.MinAdvancedQuestionsCount;

            Console.WriteLine($" Максимальное Количество экзаменов для продвинутых  слов {advancedlistMaxCount}");
            
            // TODO тут вместо макс количество экзаменов, должно быть макс количество слов
            var advancedList = await GetWordsWithExamples(
                user: user,
                maxCount: advancedlistMaxCount,
                minimumQuestionAsked: examSettings.MinimumQuestionAsked,
                minBestLearnedWords: examSettings.MinBestLearnedWords);
          
            //TODO Подобрать количество экзаменов для advanced слов
            var minimumTimesThatWordHasToBeAsked =
                Rand.RandomIn(examSettings.MinAdvancedExamMinQuestionAskedForOneWordCount,
                    examSettings.MaxAdvancedExamMinQuestionAskedForOneWordCount);

            Console.WriteLine($"Минимальное кол во повтора продвинутых слов {minimumTimesThatWordHasToBeAsked}");
            
            examsList.AddRange(advancedList);
            
            Console.WriteLine($"Количество экзаменов для advanced слов: {advancedList.Count}");
            Console.WriteLine(string.Join(" \r\n", advancedList.ToList()));
            
            return examsList;
        }
        
        public Task<IReadOnlyCollection<UserWordModel>> GetAllWords(UserModel user) 
            => _userWordsRepository.GetAllWords(user);

        public Task<bool> Contains(UserModel user, string word) =>  _userWordsRepository.Contains(user, word);
    }
}