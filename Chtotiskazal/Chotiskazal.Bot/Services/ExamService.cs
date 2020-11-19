using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using Chotiskazal.Dal.DAL;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;

namespace Chotiskazal.Bot.Services
{
    public class ExamService
    {
        private readonly ExamsAndMetricService _examsAndMetricService;
        private readonly DictionaryService _dictionaryService;
        private readonly UsersWordsService _usersWordsService;

        public ExamService(ExamsAndMetricService examsAndMetricService,
            DictionaryService dictionaryService, UsersWordsService usersWordsService)
        {
            _examsAndMetricService = examsAndMetricService;
            _dictionaryService = dictionaryService;
            _usersWordsService = usersWordsService;
        }

        public async Task<UserWordModel[]> GetWordsForLearningWithPhrasesAsync(User user, int count,
            int maxTranslationSize)
        {
            var wordsForLearning = await _usersWordsService.GetWorstForUserWithPhrasesAsync(user, count);

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

        private async Task<UserWordModel[]> GetPairsForExamAsync(User user, int delta, int randomRate) =>
            await _usersWordsService.GetWorstWordsForUserAsync(user, delta, randomRate);

        public List<UserWordModel> PreparingExamsList(UserWordModel[] learningWords)
        {
            var examsList = new List<UserWordModel>(learningWords.Length * 4);

            //Every learning word appears in test from 2 to 4 times
            examsList.AddRange(learningWords.Randomize());
            examsList.AddRange(learningWords.Randomize());
            examsList.AddRange(learningWords.Randomize().Where(w => RandomTools.Rnd.Next() % 2 == 0));
            examsList.AddRange(learningWords.Randomize().Where(w => RandomTools.Rnd.Next() % 2 == 0));

            while (examsList.Count > 32)
            {
                examsList.RemoveAt(examsList.Count - 1);
            }

            return examsList;
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
            testWords = await GetPairsForExamAsync(user, delta, randomRate);
            return testWords;
        }

        public async Task SaveQuestionMetrics(QuestionMetric questionMetric) =>
            await _examsAndMetricService.SaveQuestionMetricsAsync(questionMetric);

        public async Task RegisterExamAsync(User user, DateTime started, int examsCount, int examsPassed) =>
            await _examsAndMetricService.RegisterExamAsync(user.TelegramId??-1, started, examsCount, examsPassed);

        public async Task RegisterSuccessAsync(UserWordModel userWordForLearning) =>
            await _usersWordsService.RegisterSuccessAwait(userWordForLearning);

        public async Task RegisterFailureAsync(UserWordModel userWordForLearning) =>
            await _usersWordsService.RegisterFailureAsync(userWordForLearning);

        public QuestionMetric CreateQuestionMetric(UserWordModel pairModel, IExam exam)
        {
            var questionMetric = new QuestionMetric
            {
                Word = pairModel.Word,
                Created = DateTime.Now,
                AggregateScoreBefore = pairModel.AggregateScore,
                ExamsPassed = pairModel.Examed,
                PassedScoreBefore = pairModel.PassedScore,
                PreviousExam = pairModel.LastExam,
                Type = exam.Name,
            };
            return questionMetric;
        }

        public async Task RandomizationAndJobsAsync(User user)
        {
            if (RandomTools.Rnd.Next() % 30 == 0)
                await AddMutualPhrasesToVocabAsync(user, 10);
            else
                await _usersWordsService.UpdateAgingAndRandomizeAsync(user,50);
        }

        private  Task AddMutualPhrasesToVocabAsync(User user, int maxCount)
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

        public async Task<string[]> GetAllMeaningOfWordForExamination(string word) =>
            await _dictionaryService.GetAllTranslationsAsync(word);

        public async Task<bool> HasAnyAsync(User user) => await _usersWordsService.HasWords(user);
    }
}
