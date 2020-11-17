using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Chotiskazal.Dal.DAL;
using Chotiskazal.Dal.Enums;
using Chotiskazal.Dal.Services;
using Chotiskazal.Dal.yapi;

namespace Chotiskazal.Bot.Services
{
    public class AddWordService
    {
        private readonly UsersWordsService _usersWordsService;
        private readonly YandexDictionaryApiClient _yaDicClient;
        private readonly YandexTranslateApiClient _yaTransClient;
        private readonly DictionaryService _dictionaryService;

        public AddWordService(UsersWordsService usersWordsService, YandexDictionaryApiClient yaDicClient,
            YandexTranslateApiClient yaTransClient, DictionaryService dictionaryService)
        {
            _usersWordsService = usersWordsService;
            _yaDicClient = yaDicClient;
            _yaTransClient = yaTransClient;
            _dictionaryService = dictionaryService;
        }

        public async Task<IReadOnlyList<UserWordForLearning>> TranslateAndAddToDictionary(string word)
        {
            word = word.ToLower();
            
            var wordsForLearning = new List<UserWordForLearning>();
            
            if (!word.Contains(' '))
            {
                var result =await _yaDicClient.TranslateAsync(word);

                //Создаем из ответа(если он есть)  TranslationAndContext или WordDictionary?
                if (result?.Any() == true)
                {
                    var variants = result.SelectMany(r => r.Tr);
                    var transcription = result.FirstOrDefault()?.Ts;
                  
                    foreach (var yandexTranslation in variants)
                    {
                        var yaPhrases = yandexTranslation.GetPhrases(word);
                       
                        //Заполняем бд(wordDictionary и фразы)
                        var phrasesId= await _dictionaryService.AddNewWordPairToDictionaryWithPhrasesAsync(word,
                            yandexTranslation.Text, transcription, TranslationSource.Yadic, yaPhrases);

                        

                        wordsForLearning.Add(UserWordForLearning.CreatePair(word, yandexTranslation.Text, transcription,
                            yaPhrases,phrasesId.ToArray()));
                    }
                    return wordsForLearning;
                }
            }

            try
            {
                var transAnsTask =  await _yaTransClient.Translate(word);
                if (!string.IsNullOrWhiteSpace(transAnsTask))
                {
                    //1. Заполняем бд(wordDictionary, фраз нет)
                    var id = await _dictionaryService.AddNewWordPairToDictionaryAsync(
                        word,
                        transAnsTask,
                        "[]",
                        TranslationSource.Yadic);
                    //2. Дополняем ответ
                    wordsForLearning.Add(UserWordForLearning.CreatePair(word, transAnsTask, ""));
                }
            }
            catch (Exception e)
            {
                return wordsForLearning;
            }

            return wordsForLearning;
        }

        public async Task<IReadOnlyList<UserWordForLearning>> FindInDictionaryWithPhrases(string word)
        {
            word = word.ToLower();
            
            var pairWords = await _dictionaryService.GetAllPairsByWordWithPhrasesAsync(word);

            return pairWords.Select(wordDictionary => UserWordForLearning.CreatePair(
                wordDictionary.EnWord,
                wordDictionary.RuWord,
                wordDictionary.Transcription,
                wordDictionary.Phrases,
                wordDictionary.Phrases.Select(ph => ph.Id).ToArray())
            ).ToList();
        }
        
        public async Task<int> AddSomeWordsToUserCollectionAsync(int userId, UserWordForLearning[] results)
        {
            var count = 0;
            var userTranslations = results.Select(t => t.UserTranslations).ToArray();
            var allPhrasesForUser = new List<Phrase>();
            
            foreach (var result in results)
                allPhrasesForUser.AddRange(result.Phrases);
            
            var userWord = await _usersWordsService.GetWordForLearningOrNullByEnWordAsync(userId, results.FirstOrDefault()?.EnWord);
            
            if (userWord != null)
            {
                var translates = userWord.GetTranslations().ToList();
                foreach (var userTranslation in userTranslations)
                {
                    if (translates.Contains(userTranslation))
                        continue;
                    translates.Add(userTranslation);
                    
                    count++;
                }

                var userPhrasesId=userWord.GetPhrasesId();
                userWord.PhrasesIds += ',' + string.Join(",", allPhrasesForUser.Where(i=> !userPhrasesId.Contains(i.Id)).Select(i=>i.Id));

                userWord.SetTranslation(translates.ToArray());
                _usersWordsService.UpdateWord(userWord);
                return count;
            }

            await _usersWordsService.AddWordToUserCollectionAsync(
                new UserWordForLearning()
            {
                UserId = userId,
                EnWord = results.FirstOrDefault()?.EnWord,
                UserTranslations = string.Join(",", userTranslations),
                Transcription = results.FirstOrDefault()?.Transcription ?? "",
                PhrasesIds = string.Join(",",allPhrasesForUser.Select(ph => ph.Id).ToArray()),
                Phrases = allPhrasesForUser,
                IsPhrase = false,
            });
            
            return userTranslations.Count();
        }
    }
}