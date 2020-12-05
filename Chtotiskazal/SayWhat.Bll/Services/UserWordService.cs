using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
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

        public Task AddUserWord(UserWord entity) =>
             _userWordsRepository.Add(entity);

        private async Task<IEnumerable<UserWordModel>> GetWorstForUserWithPhrasesAsync(User user, int count)
        {
            var words = await _userWordsRepository.GetWorstLearned(user, count);
            await IncludeExamples(words);
            return words.Select(w => new UserWordModel(w));
        }

        private async Task IncludeExamples(IReadOnlyCollection<UserWord> words)
        {
            var ids = new List<ObjectId>();

            foreach (var word in words)
            {
                foreach (var translation in word.Translations)
                {
                    ids.AddRange(translation.Examples.Select(e => e.ExampleId));
                }
            }

            var examples = (await _examplesRepo.GetAll(ids)).ToDictionary(e => e.Id);

            foreach (var word in words)
            {
                foreach (var translation in word.Translations)
                {
                    foreach (var example in translation.Examples)
                    {
                        example.ExampleOrNull = examples[example.ExampleId];
                    }
                }
            }
        }

        public async Task<UserWordModel[]> GetWordsWithExamples(User user, int maxCount, int minimumQuestionAsked)
        {
            if (maxCount <= 0)
                return new UserWordModel[0];

            var words = await _userWordsRepository.Get(user, maxCount, minimumQuestionAsked);
            await IncludeExamples(words);
            return words
                .Select(t => new UserWordModel(t))
                .ToArray();;
        }


        public async Task RegisterFailure(UserWordModel userWordForLearning)
        {
            userWordForLearning.OnExamFailed();
            userWordForLearning.UpdateCurrentScore();
            await _userWordsRepository.UpdateMetrics(userWordForLearning.Entity);
        }
        
        public async Task UpdateCurrentScore(User user, int count)
        {
            var sw = Stopwatch.StartNew();
            var words = await _userWordsRepository.GetOldestUpdatedWords(user, count);
            foreach (var word in words)
            {
                var model = new UserWordModel(word);
                model.UpdateCurrentScore();
                await _userWordsRepository.UpdateMetrics(model.Entity);
            }
            sw.Stop();
            Botlog.UpdateMetricInfo(user.TelegramId, nameof(UpdateCurrentScore), $"{words.Count}", sw.Elapsed);
        }

        public async Task RegisterSuccess(UserWordModel model)
        {
            model.OnExamPassed();
            model.UpdateCurrentScore();
            await _userWordsRepository.UpdateMetrics(model.Entity);
        }

        public Task<bool> HasWords(User user) => _userWordsRepository.HasAnyFor(user);
        public Task UpdateWord(UserWordModel model) =>
             _userWordsRepository.Update(model.Entity);

        public Task UpdateWordMetrics(UserWordModel model) =>
            _userWordsRepository.UpdateMetrics(model.Entity);
        
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

        
        public async Task<UserWordModel[]> GetWordsForLearningWithPhrasesAsync(
            User user, 
            int count,
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

        public async Task<IReadOnlyList<UserWordModel>> AppendAdvancedWordsToExamList(User user, UserWordModel[] learningWords, ExamSettings examSettings)
        {
            //Get exam list and test words
            var examsList = new List<UserWordModel>(examSettings.MaxExamSize);

            //Every learning word appears in exam from MIN to MAX times
            for (int i = 0; i < examSettings.MinTimesThatLearningWordAppearsInExam; i++) 
                examsList.AddRange(learningWords.Randomize());
            for (int i = 0; i < examSettings.MaxTimesThatLearningWordAppearsInExam - examSettings.MinTimesThatLearningWordAppearsInExam; i++) 
                examsList.AddRange(learningWords.Randomize().Where(w => Random.Rnd.Next() % 2 == 0));
            
            while (examsList.Count > examSettings.MaxExamSize) 
                examsList.RemoveAt(examsList.Count - 1);
            var advancedlistMaxCount = Math.Min(Random.UpTo(examSettings.MaxAdvancedQuestionsCount),
                examSettings.MaxExamSize - examsList.Count);
            if (advancedlistMaxCount <= 0)
                return examsList;

            var minimumTimesThatWordHasToBeAsked =
                Random.RandomIn(examSettings.MinAdvancedExamMinQuestionAskedCount,
                    examSettings.MaxAdvancedExamMinQuestionAskedCount);
            
            var advancedList = await GetWordsWithExamples(
                user: user,
                maxCount: advancedlistMaxCount,
                minimumQuestionAsked: minimumTimesThatWordHasToBeAsked);
            examsList.AddRange(advancedList);
            return examsList;
        }
    }
}