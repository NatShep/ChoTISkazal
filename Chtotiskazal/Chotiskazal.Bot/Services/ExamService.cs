using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using Chotiskazal.Dal.DAL;
using Chotiskazal.Dal.Services;
using Chotiskazal.DAL.Services;

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

        public async Task<UserWordForLearning[]> GetWordsForLearningWithPhrasesAsync(int userId, int count,
            int maxTranslationSize)
        {
            var wordsForLearning = await _usersWordsService.GetWorstForUserWithPhrasesAsync(userId, count);

            foreach (var wordForLearning in wordsForLearning)
            {
                var translations = wordForLearning.GetTranslations().ToArray();
                if (translations.Length <= maxTranslationSize)
                    continue;

                var usedTranslations = translations.Randomize().Take(maxTranslationSize).ToArray();
                wordForLearning.SetTranslation(usedTranslations);

                // Remove Phrases added as learning word 
                for (var i = 0; i < wordForLearning.Phrases.Count; i++)
                {
                    var phrase = wordForLearning.Phrases[i];
                    if (!usedTranslations.Contains(phrase.PhraseRuTranslate))
                        wordForLearning.Phrases.RemoveAt(i);
                }
            }
            return wordsForLearning.ToArray();
        }

        private async Task<UserWordForLearning[]> GetPairsForExamAsync(int userId, int delta, int randomRate) =>
            await _usersWordsService.GetWorstWordsForUserAsync(userId, delta, randomRate);

        public List<UserWordForLearning> PreparingExamsList(UserWordForLearning[] learningWords)
        {
            var examsList = new List<UserWordForLearning>(learningWords.Length * 4);

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

        public async Task<UserWordForLearning[]> GetTestWordsAsync(int userId, List<UserWordForLearning> examsList)
        {
            //TODO изучть по какому принципу получаем RandomRATE. связан ли он с прогрессом подбираемых слов.
            //Или тут вообще рандомные слова будут
            var delta = Math.Min(7, (32 - examsList.Count));
            var testWords = new UserWordForLearning[0];

            if (delta <= 0)
                return testWords;

            var randomRate = 8 + RandomTools.Rnd.Next(5);
            testWords = await GetPairsForExamAsync(userId, delta, randomRate);
            return testWords;
        }

        public async Task SaveQuestionMetrics(QuestionMetric questionMetric) =>
            await _examsAndMetricService.SaveQuestionMetricsAsync(questionMetric);

        public async Task RegisterExamAsync(int userId, DateTime started, int examsCount, int examsPassed) =>
            await _examsAndMetricService.RegisterExamAsync(userId, started, examsCount, examsPassed);

        public async Task RegisterSuccessAsync(UserWordForLearning userWordForLearning) =>
            await _usersWordsService.RegisterSuccessAwait(userWordForLearning);

        public async Task RegisterFailureAsync(UserWordForLearning userWordForLearning) =>
            await _usersWordsService.RegisterFailureAsync(userWordForLearning);

        public QuestionMetric CreateQuestionMetric(UserWordForLearning pairModel, IExam exam)
        {
            var questionMetric = new QuestionMetric
            {
                WordId = pairModel.Id,
                Created = DateTime.Now,
                AggregateScoreBefore = pairModel.AggregateScore,
                ExamsPassed = pairModel.Examed,
                PassedScoreBefore = pairModel.PassedScore,
                PreviousExam = pairModel.LastExam,
                Type = exam.Name,
            };
            return questionMetric;
        }

        public async Task RandomizationAndJobsAsync(int userId)
        {
            if (RandomTools.Rnd.Next() % 30 == 0)
                await AddMutualPhrasesToVocabAsync(userId, 10);
            else
                await _usersWordsService.UpdateAgingAndRandomizeAsync(userId,50);
        }

        private async Task AddMutualPhrasesToVocabAsync(int userId, int maxCount)
        {
            var allWords = (await _usersWordsService.GetAllEnWordsForUserAsync(userId)).Select(s => s.ToLower().Trim())
                .ToHashSet();
            
            var allWordsForLearning = await _usersWordsService.GetAllUserWordsWithPhrasesForUserAsync(userId);

            var allPhrasesIdForUser = new List<int>();
            
            foreach (var word in allWordsForLearning)
            {
                var phrases = word.GetPhrasesId();
                allPhrasesIdForUser.AddRange(phrases);
            }

            var allPhrases = await _dictionaryService.FindPhrasesBySomeIdsAsync(allPhrasesIdForUser.ToArray());

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
                    if (allWords.Contains(lowerWord))
                        count++;
                    else if (word.EndsWith('s'))
                    {
                        var withoutEnding = lowerWord.Remove(lowerWord.Length - 1);
                        if (allWords.Contains(withoutEnding))
                            endingCount++;
                    }
                    else if (word.EndsWith("ed"))
                    {
                        var withoutEnding = lowerWord.Remove(lowerWord.Length - 2);

                        if (allWords.Contains(withoutEnding))
                            endingCount++;
                    }
                    else if (word.EndsWith("ing"))
                    {
                        var withoutEnding = lowerWord.Remove(lowerWord.Length - 3);

                        if (allWords.Contains(withoutEnding))
                            endingCount++;
                    }

                    if (count + endingCount <= 1)
                        continue;
                    searchedPhrases.Add(phrase);
                    if (endingCount > 0)
                    {
                        endings++;
                    }

                    if (count + endingCount > 2)
                        Console.WriteLine(phraseText);
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

            Console.WriteLine($"Found: {searchedPhrases.Count}+{endings}");
        }

        public async Task<string[]> GetAllMeaningOfWordForExamination(string word) =>
            await _dictionaryService.GetAllTranslationsAsync(word);

        public async Task<bool> HasAnyAsync(int userId) => await _usersWordsService.GetAnyWordAsync(userId);
    }
}
