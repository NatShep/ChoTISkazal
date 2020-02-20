﻿using System;
using System.Linq;
using Chotiskazal.Logic.DAL;
using Dic.Logic;
using Dic.Logic.DAL;
using Dic.Logic.Dictionaries;

namespace Chotiskazal.Logic.Services
{
    public class NewWordsService
    {
        private readonly RuengDictionary _dictionary;
        private readonly WordsRepository _repository;

        public NewWordsService(RuengDictionary dictionary, WordsRepository repository)
        {
            _dictionary = dictionary;
            _repository = repository;
        }
        public void SaveForExams(string word, string translation, string[] allMeanings, string transcription)
        {
            SaveForExams(
                word:          word, 
                transcription: transcription, 
                allMeanings: allMeanings,
                translations:  translation
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s=>s.Trim())
                    .ToArray());
        }
        public void SaveForExams(string word, string transcription, string[] translations, string[] allMeanings, Phrase[] phrases = null)
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
                       if (!usedTranslations.Contains(phrase.Translation))
                           pairModel.Phrases.RemoveAt(i);
                 }
            }
            return fullPairs;
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

        public PairModel Get(string word)
        {
            return _repository.GetOrNull(word);
        }

        public void SaveQuestionMetrics(QuestionMetric questionMetric)
        {
            _repository.AddQuestionMetric(questionMetric);
        }

        public PairModel[] GetPairsForTests(int count, int learnRate)
        {
            return _repository.GetPairsForTests(count, learnRate);
        }

        public void Add(Phrase phrase)
        {
            _repository.Add(phrase);
        }

        public void UpdateRatings(PairModel pairModel)
        {
            _repository.UpdateScoresAndTranslation(pairModel);
        }

        public void Remove(Phrase phrase)
        {
            _repository.Remove(phrase);
        }
    }
}