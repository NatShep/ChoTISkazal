using System;
using System.Collections.Generic;
using System.Linq;
using Chotiskazal.Logic.DAL;
using Dic.Logic;
using Dic.Logic.DAL;
using Dic.Logic.Dictionaries;

namespace Chotiskazal.Logic.Services
{
    public class NewWordsService
    {
        private readonly RuEngDictionary _dictionary;
        private readonly WordsRepository _repository;

        public NewWordsService(RuEngDictionary dictionary, WordsRepository repository)
        {
            _dictionary = dictionary;
            _repository = repository;
        }

        public void SaveForExams(string word, string translation, string[] allMeanings, string transcription)
        {
            SaveToDictionary(
                word:          word, 
                transcription: transcription, 
                allMeanings: allMeanings,
                translations:  translation
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s=>s.Trim())
                    .ToArray());
        }
        public void SaveToDictionary(string word, string transcription, string[] translations, string[] allMeanings, Phrase[] phrases = null)
        {
            var alreadyExists = _repository.GetOrNull(word);
            if (alreadyExists == null)
                _repository.CreateNew(word, string.Join(", ", translations), allMeanings, transcription,  phrases);
            else
            {

                var updatedTranslations = alreadyExists
                    .Translation
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Union(translations)
                    .ToArray();
                alreadyExists.Translation = string.Join(", ", updatedTranslations);
                alreadyExists.OnExamFailed();
                _repository.UpdateScoresAndTranslation(alreadyExists);
            }
        }
        public DictionaryMatch GetTranslations(string word)
        {
           return _dictionary.GetOrNull(word);
        }
        public int GetContextPhraseCount() => _repository.GetContextPhrasesCount();
        public PairModel[] GetAll() => _repository.GetAll();
        public Phrase[] GetAllPhrases() => _repository.GetAllPhrases();
        public PairModel Get(string word)
        {
            return _repository.GetOrNull(word);
        }
        public PairModel GetOrNullWithPhrases(string word) => _repository.GetOrNullWithPhrases(word);
        public void UpdateAgingAndRandomize()
        {
            _repository.UpdateAgingAndRandomization();
        }
        public void UpdateAgingAndRandomize(int count)
        {
            _repository.UpdateAgingAndRandomization(count);
        }
        public PairModel[] GetPairsForLearning(int count, int maxTranslationSize)
        {
            var fullPairs = _repository.GetWorst(count);
            foreach (var pairModel in fullPairs)
            {
                var translations = pairModel.GetTranslations().ToArray();
                if(translations.Length<=maxTranslationSize)
                    continue;
                var usedTranslations = translations.Randomize().Take(maxTranslationSize).ToArray();
                 pairModel.SetTranslations(usedTranslations);
                 for (int i = 0; i < pairModel.Phrases.Count; i++)
                 {
                     var phrase = pairModel.Phrases[i];
                       if (!usedTranslations.Contains(phrase.Trans))
                           pairModel.Phrases.RemoveAt(i);
                 }
            }
            return fullPairs;
        }
        public PairModel[] GetPairsForTests(int count, int learnRate)
        {
            return _repository.GetPairsForTests(count, learnRate);
        }
        public void RegistrateFailure(PairModel model)
        {
             model.OnExamFailed();
             model.UpdateAgingAndRandomization();
             _repository.UpdateScores(model);
        }
        public Exam[] GetAllExams() => _repository.GetAllExams();
        public void RegistrateExam(DateTime started, int count, int successCount)
        {
            _repository.AddExam(new Exam{
                Started = started,
                Count = count,
                Failed = count - successCount,
                Finished = DateTime.Now,
                Passed = successCount});
        }
        public void RegistrateSuccess(PairModel model)
        {
            model.OnExamPassed();
            model.UpdateAgingAndRandomization();

            _repository.UpdateScores(model);
        }
        public void UpdateRatings(PairModel pairModel)
        {
            _repository.UpdateScoresAndTranslation(pairModel);
        }
        public void SaveQuestionMetrics(QuestionMetric questionMetric)
        {
            _repository.AddQuestionMetric(questionMetric);
        }


        public void Add(Phrase phrase)
        {
            _repository.Add(phrase);
        }
        public void Remove(Phrase phrase)
        {
            _repository.Remove(phrase);
        }

        public void AddMutualPhrasesToVocab(int maxCount)
        {
            var allWords = GetAll().Select(s => s.OriginWord.ToLower().Trim()).ToHashSet();

            var allPhrases = GetAllPhrases();
            List<Phrase> searchedPhrases = new List<Phrase>();
            int endings = 0;
            foreach (var phrase in allPhrases)
            {
                var phraseText = phrase.Origin;
                int count = 0;
                int endingCount = 0;
                foreach (var word in phraseText.Split(new[] { ' ', ',' }))
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
                    if (count + endingCount > 1)
                    {
                        searchedPhrases.Add(phrase);
                        if (endingCount > 0)
                        {
                            endings++;
                        }
                        if (count + endingCount > 2)
                            Console.WriteLine(phraseText);
                        break;

                    }
                }
            }

            var firstPhrases = searchedPhrases.Randomize().Take(maxCount);
            foreach (var phrase in firstPhrases)
            {
                Console.WriteLine("Adding " + phrase.Origin);
        //        SaveForExams(phrase.Origin, phrase.Translation, new[] { phrase.Translation }, null);
                Remove(phrase);
            }
            Console.WriteLine($"Found: {searchedPhrases.Count}+{endings}");
        }
    }
}
