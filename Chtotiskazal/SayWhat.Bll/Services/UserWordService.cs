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

        public async Task RefreshWordsCurrentScoreAsync(UserModel user) {
            var words = await _userWordsRepository.GetAllWords(user);
            foreach (var word in words) {
                await _userWordsRepository.Update(word);
            }
        }

        public async Task RegisterFailure(UserWordModel userWordModelForLearning)
        {
            userWordModelForLearning.OnQuestionFailed();
            await _userWordsRepository.UpdateMetrics(userWordModelForLearning);
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

        public async Task<UserWordModel[]> GetWordsWithPhrasesAsync(
            UserModel user, 
            int count,
            int maxTranslations,
            double lowRating,
            double? highRating = null) 
        {
            Console.WriteLine($"Рейтинг искомых слов: {lowRating} - {highRating}");
            Console.WriteLine($"Количество мест для слов: {count}");

            var wordsForLearning = highRating is null
                ? (await _userWordsRepository.GetWordsForLearningAboveScore(user, count, lowRating)).ToList()
                : (await _userWordsRepository.GetWordsForLearningBetweenLowAndHighScores(user, count, lowRating, highRating.Value)).ToList();
            
            foreach (var wordForLearning in wordsForLearning) {
                var translations = wordForLearning.TextTranslations.ToArray();
                if (translations.Length <= maxTranslations)
                    maxTranslations = translations.Length;

                var usedTranslations = translations.Shuffle().Take(maxTranslations).ToArray();
                wordForLearning.RuTranslations = usedTranslations.Select(t => new UserWordTranslation(t)).ToArray();

                // TODO Remove Phrases added as learning words
            }
            
            Console.WriteLine($"Количество слов: {wordsForLearning.Count()}");
            Console.WriteLine(string.Join(" \r\n", wordsForLearning.ToList()));
            
            await IncludeExamples(wordsForLearning);
            return wordsForLearning.ToArray();
        }
        
        public async Task<UserWordModel[]> GetRandomWordsWithPhrasesAsync(
            UserModel user, 
            int count,
            int fromNumber,
            int maxTranslations,
            double lowRating,
            double highRating) 
        {
            Console.WriteLine($"Рейтинг искомых слов: {lowRating} - {highRating}");
            Console.WriteLine($"Количество мест для слов: {count}");

            var wordsForLearning = (await _userWordsRepository.GetWordsForLearningBetweenLowAndHighScores(user, fromNumber, lowRating, highRating))
                .Shuffle()
                .Take(count)
                .ToList();

            foreach (var wordForLearning in wordsForLearning) {
                var translations = wordForLearning.TextTranslations.ToArray();
                if (translations.Length <= maxTranslations)
                    maxTranslations = translations.Length;

                var usedTranslations = translations.Shuffle().Take(maxTranslations).ToArray();
                wordForLearning.RuTranslations = usedTranslations.Select(t => new UserWordTranslation(t)).ToArray();

                // TODO Remove Phrases added as learning words
            }
            
            Console.WriteLine($"Количество слов: {wordsForLearning.Count()}");
            Console.WriteLine(string.Join(" \r\n", wordsForLearning.ToList()));
            
            await IncludeExamples(wordsForLearning);
            return wordsForLearning.ToArray();
        }
        
        public async Task<UserWordModel[]> GetLastAskedWordsWithPhrasesAsync(
            UserModel user, 
            int count,
            ExamSettings examSettings) 
        {
            Console.WriteLine("Поиск недавно спрошенных слов");
            Console.WriteLine($"Количество мест для слов: {count}");
            
            var newWordsForLearning = (
                await _userWordsRepository.GetLastAsked(user, count, examSettings.MinimumQuestionAsked)
                ).ToList();
            
            foreach (var wordForLearning in newWordsForLearning) {
                var translations = wordForLearning.TextTranslations.ToArray();
                var maxTranslations = translations.Length <= examSettings.MaxTranslationsInOneExam
                    ? translations.Length
                    : examSettings.MaxTranslationsInOneExam;
                
                var usedTranslations = translations.Shuffle().Take(maxTranslations).ToArray();
                wordForLearning.RuTranslations = usedTranslations.Select(t => new UserWordTranslation(t)).ToArray();

                // TODO Remove Phrases added as learning words
                /*
                     todo wtf?
                     for (var i = 0; i < wordForLearning.RuPhrases.Count; i++)
                    {
                        var phrase = wordForLearning.RuPhrases[i];
                        if (!usedTranslations.Contains(phrase.PhraseRuTranslate))
                            wordForLearning.RuPhrases.RemoveAt(i);
                    }*/
            }
            
            Console.WriteLine($"Количество lasted слов: {newWordsForLearning.Count}");
            Console.WriteLine(string.Join(" \r\n", newWordsForLearning));
            
            await IncludeExamples(newWordsForLearning);
            return newWordsForLearning.ToArray();
        }
        
        public Task<IReadOnlyCollection<UserWordModel>> GetAllWords(UserModel user) 
            => _userWordsRepository.GetAllWords(user);

        public Task<bool> Contains(UserModel user, string word) =>  _userWordsRepository.Contains(user, word);
    }
}
