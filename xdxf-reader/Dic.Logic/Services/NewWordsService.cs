using System;
using System.Linq;
using Dic.Logic.DAL;
using Dic.Logic.Dictionaries;

namespace Dic.Logic.Services
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
        public void SaveForExams(string word, string translation, string transcription)
        {
            SaveForExams(
                word:          word, 
                transcription: transcription, 
                translations:  translation
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s=>s.Trim())
                    .ToArray());
        }
        public void SaveForExams(string word, string transcription, string[] translations, Phrase[] phrases = null)
        {
            var alreadyExists = _repository.GetOrNull(word);
            if (alreadyExists == null)
                _repository.CreateNew(word, string.Join(", ", translations), transcription, phrases);
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
        public void UpdateAgingAndRandomize()
        {
            _repository.UpdateAgingAndRandomization();
        }
        public PairModel[] GetPairsForTest(int count, int maxTranslationSize)
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

        public void RegistrateExam(DateTime started, int count, int successCount)
        {
            _repository.AddExam(started, DateTime.Now,count, successCount, count-successCount);
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
    }
}
