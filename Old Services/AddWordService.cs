using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Dal.Repo;
using Chotiskazal.Dal.Services;
using Chotiskazal.LogicR.yapi;
using System.Timers;
using Chotiskazal.ConsoleTesting;
using Chotiskazal.DAL;
using Chotiskazal.Dal.Enums;
using Chotiskazal.DAL.ModelsForApi;
using Chotiskazal.LogicR;
using Chotiskazal.WebApp.Models;


namespace Chotiskazal.Api.OldServices
{
    public class AddWordService
    {
        private UsersPairsService _usersPairsService;
        private readonly YandexDictionaryApiClient _yapiDicClient;
        private readonly YandexTranslateApiClient _yapiTransClient;
        private readonly DictionaryService _dictionaryService;

        public AddWordService(UsersPairsService usersPairsService, YandexDictionaryApiClient yapiDicClient,
            YandexTranslateApiClient yapiTransClient, DictionaryService dictionaryService)
        {
            _usersPairsService = usersPairsService;
            _yapiDicClient = yapiDicClient;
            _yapiTransClient = yapiTransClient;
            _dictionaryService = dictionaryService;
        }

        public void AddMutualPhrasesToVocab(int userId, int maxCount)
        {
            var allWords = _usersPairsService.GetAllWords(userId).Select(s => s.ToLower().Trim()).ToHashSet();
            var allPhrases = _usersPairsService.GetAllPhrases(userId);
            
           
            List<Phrase> searchedPhrases = new List<Phrase>();
            int endings = 0;
            foreach (var phrase in allPhrases)
            {
                var phraseText = phrase.EnPhrase;
                int count = 0;
                int endingCount = 0;
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
                Console.WriteLine("Adding " + phrase.EnPhrase);
                
              //TODO   _usersWordService.AddPhraseAsWordToUserCollection();
              _dictionaryService.RemovePhrase(phrase.Id);
            }

            Console.WriteLine($"Found: {searchedPhrases.Count}+{endings}");
        }

        public (bool isYaDicOnline, bool isYaTransOnline) PingYandex()
        {
            var dicPing = _yapiDicClient.Ping();
            var transPing = _yapiTransClient.Ping();
            Task.WaitAll(dicPing, transPing);
            var timer = new Timer(5000) {AutoReset = false};
            timer.Enabled = true;
            timer.Elapsed += (s, e) =>
            {
                var pingDicApi = _yapiDicClient.Ping();
                var pingTransApi = _yapiTransClient.Ping();
                Task.WaitAll(pingDicApi, pingTransApi);
                timer.Enabled = true;
            };

            return (_yapiDicClient.IsOnline, _yapiTransClient.IsOnline);
        }

        public List<TranslationAndContext> TranslateAndAddToDb(string word)
        {
            List<TranslationAndContext> translationsWithContext = new List<TranslationAndContext>();

            if (!word.Contains(' '))
            {
                Task<YaDefenition[]> task = null;
                task = _yapiDicClient.Translate(word);
                task?.Wait();

                //Создаем из ответа(если он есть)  TranslationAndContext или WordDictionary?
                if (task?.Result?.Any() == true)
                {
                    var variants = task.Result.SelectMany(r => r.Tr);
                    var transcription = task.Result.Select(r => r.Ts).FirstOrDefault();

                    foreach (var yandexTranslation in variants)
                    {
                        var yaPhrases = yandexTranslation.GetPhrases(word);
                        var dbPhrases = new List<Phrase>();

                        //Заполняем бд(wordDictionary и фразы)
                        var id = _dictionaryService.AddNewWordPairToDictionary(
                            word,
                            yandexTranslation.Text,
                            transcription,
                            TranslationSource.Yadic);
                        foreach (var yaPhrase in yaPhrases)
                        {
                            var phrase = yaPhrase.MapToDbPhrase();
                            dbPhrases.Add(phrase);
                            _dictionaryService.AddPhraseForWordPair(id,word, null, phrase.EnPhrase, phrase.RuTranslate);
                        }

                        translationsWithContext.Add(new TranslationAndContext(id, word, yandexTranslation.Text,
                            transcription,
                            dbPhrases.ToArray()));
                    }

                    return translationsWithContext;
                }
            }

            try
            {
                var transAnsTask = _yapiTransClient.Translate(word);
                transAnsTask.Wait();
                if (!string.IsNullOrWhiteSpace(transAnsTask.Result))
                {
                    //1. Заполняем бд(wordDictionary, фраз нет)
                    var id = _dictionaryService.AddNewWordPairToDictionary(
                        word,
                        transAnsTask.Result,
                        "[]",
                        TranslationSource.Yadic);
                    //2. Дополняем ответ
                    translationsWithContext.Add(new TranslationAndContext(id, word, transAnsTask.Result, "[]",
                        new Phrase[0]));
                }
            }
            catch (Exception e)
            {
                return translationsWithContext;
            }

            return translationsWithContext;
        }
        
        public List<TranslationAndContext> FindInDictionary(string word)
        {
            var pairWords = _dictionaryService.GetAllPairsByWord(word);
          
            // map to viewModel
            var translateWithContexts = new List<TranslationAndContext>();
            foreach (var wordDictionary in pairWords)
                translateWithContexts.Add(wordDictionary.MapToTranslationAndContext());
            
            return translateWithContexts;
        }
        
        public List<TranslationAndContext> FindInDictionaryWithPhrases(string word)
        {
            var pairWords = _dictionaryService.GetAllPairsByWordWithPhrases(word);
          
            // map to viewModel
            var translateWithContexts = new List<TranslationAndContext>();
            foreach (var wordDictionary in pairWords)
                translateWithContexts.Add(wordDictionary.MapToTranslationAndContext());
            
            return translateWithContexts;        }

        public void AddPairToUserCollection(in int userId, int id)
        {
            var userPair = _usersPairsService.GetPairByDicId(userId, id);
            if (userPair == null)
                _usersPairsService.AddWordToUserCollection(userId, id);
        }
    }
}