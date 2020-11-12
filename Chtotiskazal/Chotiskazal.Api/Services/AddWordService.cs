using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Dal.Services;
using Chotiskazal.LogicR.yapi;
using System.Timers;
using Chotiskazal.DAL;
using Chotiskazal.Dal.Enums;
using Chotiskazal.DAL.Services;
using Chotiskazal.LogicR;


namespace Chotiskazal.Api.Services
{
    public class AddWordService
    {
        private UsersWordsService _usersWordsService;
        private readonly YandexDictionaryApiClient _yapiDicClient;
        private readonly YandexTranslateApiClient _yapiTransClient;
        private readonly DictionaryService _dictionaryService;

        public AddWordService(UsersWordsService usersWordsService, YandexDictionaryApiClient yapiDicClient,
            YandexTranslateApiClient yapiTransClient, DictionaryService dictionaryService)
        {
            _usersWordsService = usersWordsService;
            _yapiDicClient = yapiDicClient;
            _yapiTransClient = yapiTransClient;
            _dictionaryService = dictionaryService;
        }

        public async Task AddMutualPhrasesToVocabAsync(int userId, int maxCount)
        {
            var allWords =(await _usersWordsService.GetAllWordsAsync(userId)).Select(s => s.ToLower().Trim()).ToHashSet();
            var allWordsForLearning = await _usersWordsService.GetAllUserWordsForLearningAsync(userId);
         
            List<int> allPhrasesIdForUser = new List<int>();

            foreach (var word in allWordsForLearning)
            {
                var phrases = word.GetPhrasesId();
                allPhrasesIdForUser.AddRange(phrases);
            }

            var allPhrases = await _dictionaryService.FindSeveralPhrasesByIdAsync(allPhrasesIdForUser.ToArray());
           
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
                
              await _usersWordsService.AddPhraseAsWordToUserCollectionAsync(phrase);
           //cv    _usersWordsService.RemovePhrase(phrase.Id,userId);
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

        public async Task<List<UserWordForLearning>> TranslateAndAddToDictionary(string word)
        {
            word = word.ToLower();
            
            List<UserWordForLearning> WordsForLearning = new List<UserWordForLearning>();
            List<int> phrasesId = new List<int>();
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
                        var id =await _dictionaryService.AddNewWordPairToDictionaryAsync(
                            word,
                            yandexTranslation.Text,
                            transcription,
                            TranslationSource.Yadic);
                        foreach (var phrase in yaPhrases)
                        {
                            dbPhrases.Add(phrase);
                            phrasesId.Add(await _dictionaryService.AddPhraseForWordPairAsync(id, word, yandexTranslation.Text , phrase.EnPhrase, phrase.PhraseRuTranslate));
                        }

                        WordsForLearning.Add(new UserWordForLearning()
                        {
                            EnWord=word,
                            UserTranslations = yandexTranslation.Text,
                            Transcription = transcription,
                            Phrases=dbPhrases,
                            PhrasesIds =string.Join(",", phrasesId),
                            IsPhrase = false,
                        });
                    }
                    return WordsForLearning;
                }
            }

            try
            {
                var transAnsTask = _yapiTransClient.Translate(word);
                transAnsTask.Wait();
                if (!string.IsNullOrWhiteSpace(transAnsTask.Result))
                {
                    //1. Заполняем бд(wordDictionary, фраз нет)
                    var id = await _dictionaryService.AddNewWordPairToDictionaryAsync(
                        word,
                        transAnsTask.Result,
                        "[]",
                        TranslationSource.Yadic);
                    //2. Дополняем ответ
                    WordsForLearning.Add(new UserWordForLearning()
                    {
                        EnWord=word,
                        UserTranslations = transAnsTask.Result,
                        Transcription = "",
                        PhrasesIds = "",
                        IsPhrase = false,
                    });
                }
            }
            catch (Exception e)
            {
                return WordsForLearning;
            }

            return WordsForLearning;
        }


        public async Task<List<UserWordForLearning>> FindInDictionaryWithPhrases(string word)
        {
            word = word.ToLower();
            
            var pairWords = await _dictionaryService.GetAllPairsByWordWithPhrases(word);
          
            var translateWithContexts = new List<UserWordForLearning>();
            foreach (var wordDictionary in pairWords)
            {
                translateWithContexts.Add(new UserWordForLearning()
                {
                    EnWord = wordDictionary.EnWord,
                    UserTranslations = wordDictionary.RuWord,
                    Transcription = wordDictionary.Transcription,
                    PhrasesIds =string.Join(",", wordDictionary.Phrases.Select(ph=>ph.Id)),
                    Phrases = wordDictionary.Phrases,
                    IsPhrase = false,
                });
            }
            return translateWithContexts;
        }

        public async Task<int> AddWordToUserCollectionAsync(UserWordForLearning userWordForLearning) =>
            await _usersWordsService.AddWordToUserCollectionAsync(userWordForLearning);
        
        public async Task<UserWordForLearning> FindInUserWordsOrNullAsync(int userId, string enWord)=>
            await _usersWordsService.GetWordForLearningOrNullByWordAsync(userId, enWord);
        
        public async Task<int> AddResultToUserCollectionAsync(int userId, UserWordForLearning[] results)
        {
            var count = 0;
            var userTranslations = results.Select(t => t.UserTranslations);
            var allPhrases = new List<Phrase>();
            foreach (var result in results)
            {
                allPhrases.AddRange(result.Phrases);
            }

            var userWord = await _usersWordsService.GetWordForLearningOrNullByWordAsync(userId, results.FirstOrDefault().EnWord);
            if (userWord != null)
            {
                var translates = userWord.GetTranslations().ToList();
                foreach (var userTranslation in userTranslations)
                {
                    if (!translates.Contains(userTranslation))
                    {
                        translates.Add(userTranslation);
                        count++;
                    }
                }

                userWord.SetTranslation(translates.ToArray());
                _usersWordsService.UpdateWord(userWord);
                return count;
            }
            
            await _usersWordsService.AddWordToUserCollectionAsync(new UserWordForLearning()
            {
                UserId = userId,
                EnWord = results.FirstOrDefault().EnWord,
                UserTranslations = string.Join(",", userTranslations),
                Transcription = results.FirstOrDefault().Transcription,
                PhrasesIds = "",
                Phrases = allPhrases,
                IsPhrase = false,
            });
            return userTranslations.Count();

        }
        
        //TODO 
        
        public async Task AddPairToUserCollectionAsync(int userId, int id)
        {
            int count = 0;
            var wordFromDic = await _dictionaryService.GetPairWithPhrasesByIdOrNullAsync(id);
            List<int> phrasesId = wordFromDic.Phrases.Select(p => p.Id).ToList();
            
            var wordForLearning=await _usersWordsService.GetWordForLearningOrNullByWordAsync(userId, wordFromDic.EnWord);
         
            if (wordForLearning != null)
            {
                var translates = wordForLearning.GetTranslations().ToList();
                if (!translates.Contains(wordFromDic.RuWord))
                {
                    translates.Add(wordFromDic.RuWord);
                    wordForLearning.SetTranslation(translates.ToArray());
                    count++;
                }
                
            }
            else
            {
                var newWordForLearning= new UserWordForLearning()
                {
                    EnWord = wordFromDic.EnWord,
                    UserTranslations = wordFromDic.RuWord,
                    Transcription = wordFromDic.Transcription,
                    PhrasesIds = string.Join(",",phrasesId),
                    Phrases = wordFromDic.Phrases,
                    IsPhrase=false, 
                };
                await _usersWordsService.AddWordToUserCollectionAsync(newWordForLearning);

            }
             
        }


       
    }
}