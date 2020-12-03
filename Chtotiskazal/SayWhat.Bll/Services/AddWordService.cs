using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using DictionaryTranslation = SayWhat.Bll.Dto.DictionaryTranslation;

namespace SayWhat.Bll.Services
{
    public class AddWordService
    {
        private readonly UsersWordsService _usersWordsService;
        private readonly YandexDictionaryApiClient _yaDicClient;
        private readonly YandexTranslateApiClient _yaTransClient;
        private readonly DictionaryService _dictionaryService;
        private readonly UserService _userService;

        public AddWordService(UsersWordsService usersWordsService, YandexDictionaryApiClient yaDicClient,
            YandexTranslateApiClient yaTransClient, 
            DictionaryService dictionaryService, 
            UserService userService)
        {
            _usersWordsService = usersWordsService;
            _yaDicClient = yaDicClient;
            _yaTransClient = yaTransClient;
            _dictionaryService = dictionaryService;
            _userService = userService;
        }

        public async Task<IReadOnlyList<DictionaryTranslation>> TranslateAndAddToDictionary(string originWord)
        {
            originWord = originWord.ToLower();

            if (originWord.Count(e => e == ' ') < 3)
            {
                if (IsRussian(originWord))
                    return await TranslateRuEnWordAndAddItToDictionary(originWord);
                else
                    return await TranlateEnRuWordAndAddItToDictionary(originWord);
            }

            //todo go to translate api
            return null;
        }

        private static bool IsRussian(string englishWord) => englishWord.Count(e => e >= 'А' && e <= 'я') > 1;

        private async Task<IReadOnlyList<DictionaryTranslation>> TranlateEnRuWordAndAddItToDictionary(string englishWord)
        {
            // Go to yandex api
            var yaResponse = await _yaDicClient.EnRuTranslateAsync(englishWord);
            if (yaResponse?.Any() != true)
                return new DictionaryTranslation[0];
            var word = ConvertToDictionaryWord(englishWord, yaResponse, Language.En, Language.Ru);
            await _dictionaryService.AddNewWord(word);
            return ToDictionaryTranslations(word);
        }
        
          private async Task<IReadOnlyList<DictionaryTranslation>> TranslateRuEnWordAndAddItToDictionary(string russianWord)
        {
            // Go to yandex api
            var yaResponse = await _yaDicClient.RuEnTranslateAsync(russianWord);
            if (yaResponse?.Any() != true)
                return new DictionaryTranslation[0];
            var word = ConvertToDictionaryWord(russianWord, yaResponse, Language.Ru, Language.En);
            await _dictionaryService.AddNewWord(word);
            return ToDictionaryTranslations(word);
        }

          private static IReadOnlyList<DictionaryTranslation> ToDictionaryTranslations(DictionaryWord word)
          {
              return word.Translations.Select(
                  t => new DictionaryTranslation(
                      enWord: word.Word,
                      ruWord: t.Word,
                      enTranscription: word.Transcription,
                      source: word.Source,
                      phrases: t.Examples.Select(e => e.ExampleOrNull).ToList()
                  )).ToArray();
          }

          private static DictionaryWord ConvertToDictionaryWord(string originText, YaDefenition[] definitions, 
              Language langFrom,
              Language langTo
              )
          {
              if(langFrom==langTo)
                  throw new InvalidOperationException();
              
              var variants = definitions.SelectMany(r => r.Tr.Select(tr => new
              {
                  defenition = r,
                  translation = tr,
              }));

              var word = new DictionaryWord
              {
                  Id = ObjectId.GenerateNewId(),
                  Language = langFrom,
                  Word = originText,
                  Source = TranslationSource.Yadic,
                  Transcription = definitions.FirstOrDefault()?.Ts,
                  Translations = variants.Select(v => new SayWhat.MongoDAL.Dictionary.DictionaryTranslation
                  {
                      Word = v.translation.Text,
                      Language = langTo,
                      Examples = v.translation.Ex?.Select(e =>
                      {
                          var id = ObjectId.GenerateNewId();
                          return new DictionaryReferenceToExample
                          {
                              ExampleId = id,
                              ExampleOrNull = new Example
                              {
                                  Id = id,
                                  OriginWord = originText,
                                  TranslatedWord = v.translation.Text,
                                  Direction = langFrom== Language.En? TranlationDirection.EnRu: TranlationDirection.RuEn,
                                  OriginPhrase = e.Text,
                                  TranslatedPhrase = e.Tr.First().Text,
                              }
                          };
                      }).ToArray() ?? new DictionaryReferenceToExample[0]
                  }).ToArray()
              };
              return word;
          }

          public async Task<IReadOnlyList<DictionaryTranslation>> FindInDictionaryWithExamples(string word) 
            => await _dictionaryService.GetTranslationsWithExamples(word.ToLower());
        public async Task<IReadOnlyList<DictionaryTranslation>> FindInDictionaryWithoutExamples(string word) 
            => await _dictionaryService.GetTranslationsWithExamples(word.ToLower());

        public async Task AddWordsToUser(User user, DictionaryTranslation[] words)
        {
            var originWord = words.FirstOrDefault()?.EnWord;
            if (originWord == null) return;

            var alreadyExistsWord = await _usersWordsService.GetWordNullByEngWord(user, originWord);

            if (alreadyExistsWord == null)
            {
                //the Word is new for the user
                var model = new UserWord
                {
                    UserId = user.Id,
                    Word = originWord,
                    Language = TranlationDirection.EnRu,
                    Translations = words.Select(r => new UserWordTranslation
                    {
                        Transcription = r.EnTranscription,
                        Word = r.RuWord,
                        Examples = r.Examples
                            .Select(p => new UserWordTranslationReferenceToExample(p.Id))
                            .ToArray()
                    }).ToArray(),
                };
                await _usersWordsService.AddUserWord(model);
                
                user.WordsCount++;
                user.PairsCount += model.Translations.Length;
                user.ExamplesCount += model.Translations.Sum(t => t.Examples?.Length ?? 0);
                
                await _userService.UpdateCounters(user);
            }
            else
            {
                // User already have the word.

                var translates = alreadyExistsWord.GetTranslations().ToList();
                var newTranslations = new List<UserWordTranslation>();
                foreach (var r in words)
                {
                    if (translates.Contains(r.RuWord))
                        continue;
                    newTranslations.Add(new UserWordTranslation
                    {
                        Transcription = r.EnTranscription,
                        Word = r.RuWord,
                        Examples = r.Examples.Select(p => new UserWordTranslationReferenceToExample(p.Id)).ToArray()
                    });
                }

                if (newTranslations.Count == 0) return;
                alreadyExistsWord.AddTranslations(newTranslations);
                await _usersWordsService.UpdateWord(alreadyExistsWord);

                user.PairsCount += newTranslations.Count;
                user.ExamplesCount += newTranslations.Sum(t => t.Examples?.Length ?? 0);
                await _userService.UpdateCounters(user);
            }
        }

        public Task RegistrateTranslationRequest(User user)
        {
            user.EnglishWordTranlationRequestsCount++;
            return _userService.UpdateCounters(user);
        }
    }
}