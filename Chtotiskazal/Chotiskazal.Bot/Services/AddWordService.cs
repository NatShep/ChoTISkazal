using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using DictionaryTranslation = SayWhat.Bll.Dto.DictionaryTranslation;

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

        public async Task<IReadOnlyList<DictionaryTranslation>> TranslateAndAddToDictionary(string englishWord)
        {
            englishWord = englishWord.ToLower();
            
            if (!englishWord.Contains(' '))
            {
                var result =await _yaDicClient.TranslateAsync(englishWord);

                //Создаем из ответа(если он есть)  
                if (result?.Any() != true) 
                    return new DictionaryTranslation[0];
                
                var variants = result.SelectMany(r => r.Tr.Select(tr => new
                {
                    defenition = r,
                    translation = tr,
                }));

                var word = new DictionaryWord
                {
                    Id = ObjectId.GenerateNewId(),
                    Language = Language.En,
                    Word = englishWord,
                    Source = TranslationSource.Yadic,
                    Transcription = result.FirstOrDefault()?.Ts,
                    Translations = variants.Select(v => new SayWhat.MongoDAL.Dictionary.DictionaryTranslation
                    {
                        Id = ObjectId.GenerateNewId(),
                        Word = v.translation.Text,
                        Transcription = v.defenition.Ts,
                        Examples = v.translation.Ex?.Select(e => new DictionaryExample
                        {
                            Id = ObjectId.GenerateNewId(),
                            OriginExample = e.Text,
                            TranslationExample = e.Tr.First().Text,
                        }).ToArray()?? new DictionaryExample[0]
                    }).ToArray()
                };
                await _dictionaryService.AddNewWord(word);
                return word.Translations.Select(
                    t=> new DictionaryTranslation(word.Word,t.Word, t.Transcription, word.Source,
                            t.Examples.Select(e=>new Phrase(e.OriginExample,e.TranslationExample)).ToList()
                    )).ToArray();

            }
            return null;
        }

        public async Task<IReadOnlyList<DictionaryTranslation>> FindInDictionaryWithPhrases(string word) 
            => await _dictionaryService.GetAllPairsByWordWithPhrasesAsync(word.ToLower());

        public async Task<int> AddSomeWordsToUserCollectionAsync(User user, DictionaryTranslation[] results)
        {
            var allPhrasesForUser = new List<Phrase>();
            
            foreach (var result in results)
                allPhrasesForUser.AddRange(result.Phrases);

            var originWord = results.FirstOrDefault()?.EnWord;
            if (originWord == null)
                return 0;
            var userWord = await _usersWordsService.GetWordNullByEngWord(user, originWord);

            if (userWord == null)
            {
                //Word is new
                await _usersWordsService.AddWordToUserCollectionAsync(user,
                    new UserWordModel(new UserWord
                    {
                        UserId = user.Id,
                        Word = originWord,
                        Language = TranlationDirection.EnRu,
                        Translations = results.Select(r => new UserWordTranslation
                        {
                            Transcription = r.Transcription,
                            Word = r.RuWord,
                            Examples = r.Phrases.Select(p => new UserWordTranslationExample
                            {
                                Origin = p.EnPhrase,
                                Translation = p.PhraseRuTranslate
                            }).ToArray()
                        }).ToArray(),
                    }));
                return results.Length;
            }
            var translates = userWord.GetTranslations().ToList();
            var newTranslations = new List<UserWordTranslation>();
            foreach (var r in results)
            {
                if (translates.Contains(r.RuWord))
                    continue;
                newTranslations.Add(new UserWordTranslation
                {
                    Transcription = r.Transcription,
                    Word = r.RuWord,
                    Examples = r.Phrases.Select(p => new UserWordTranslationExample
                    {
                        Origin = p.EnPhrase,
                        Translation = p.PhraseRuTranslate
                    }).ToArray()
                });
            }
            if(newTranslations.Count==0)
                return 0;
            userWord.AddTranslations(newTranslations);
            await _usersWordsService.UpdateWord(userWord);
            return newTranslations.Count;
        }
    }
}