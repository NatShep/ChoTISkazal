using System;
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
            _repository.CreateNew(word, translation, transcription);
        }

        public DictionaryMatch GetTranlations(string word)
        {
           return _dictionary.GetOrNull(word);
        }

        public PairModel[] GetAll() => _repository.GetAll();
        public void UpdateAgingAndRandomize()
        {
            _repository.UpdateAgingAndRandomization();
        }
        public PairModel[] GetPairsForTest(int count)
        {
            return _repository.GetWorst(count);
        }

        public void RegistrateFailure(PairModel model)
        {
             model.OnExamFailed();
             _repository.UpdateScores(model);
        }

        public void RegistrateExam(DateTime started, int count, int successCount)
        {
            _repository.AddExam(started, DateTime.Now,count, successCount, count-successCount);
        }
        public void RegistrateSuccess(PairModel model)
        {
            model.OnExamPassed();
            _repository.UpdateScores(model);
        }

        public PairModel Get(string word)
        {
            return _repository.GetOrNull(word);
        }
    }
}
