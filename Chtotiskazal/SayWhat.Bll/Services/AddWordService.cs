using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleTranslateFreeApi;
using MongoDB.Bson;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;
using DictionaryTranslation = SayWhat.Bll.Dto.DictionaryTranslation;
using Language = SayWhat.MongoDAL.Language;

namespace SayWhat.Bll.Services
{
    public class AddWordService
    {
        private readonly UsersWordsService _usersWordsService;
        private readonly YandexDictionaryApiClient _yaDicClient;
        private readonly DictionaryService _dictionaryService;
        private readonly UserService _userService;

        public AddWordService(
            UsersWordsService usersWordsService, 
            YandexDictionaryApiClient yaDicClient,
            DictionaryService dictionaryService, 
            UserService userService)
        {
            _usersWordsService = usersWordsService;
            _yaDicClient = yaDicClient;
            _dictionaryService = dictionaryService;
            _userService = userService;
        }

        public async Task<IReadOnlyList<DictionaryTranslation>> TranslateAndAddToDictionary(string originWord)
        {
            originWord = originWord.ToLower();
            
            if (originWord.Count(e => e == ' ') < 4)
            {
                IReadOnlyList<DictionaryTranslation> res = null;
                if (originWord.IsRussian())
                    res = await TranslateRuEnWordAndAddItToDictionary(originWord);
                else
                    res =  await TranlateEnRuWordAndAddItToDictionary(originWord);

                if (res != null)
                    return res;
            }
            
            //todo go to translate api
            return null;
        }


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
                      originText: word.Word,
                      translatedText: t.Word,
                      originTranscription: word.Transcription,
                      source: word.Source,
                      tranlationDirection: word.Language == Language.En? TranslationDirection.EnRu: TranslationDirection.RuEn,
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
                                  Direction = langFrom== Language.En? TranslationDirection.EnRu: TranslationDirection.RuEn,
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

        public async Task AddWordsToUser(UserModel user, DictionaryTranslation translation, int? wordRating = null )
        {
            if (translation == null) return;
            if(translation.TranlationDirection!= TranslationDirection.EnRu)
                throw new InvalidOperationException("Only en-ru dirrection is supported");
            
            var alreadyExistsWord = await _usersWordsService.GetWordNullByEngWord(user, translation.OriginText);

            if (alreadyExistsWord == null)
            {
                //the Word is new for the user
                var model = new UserWordModel(
                    id:          user.Id, 
                    word:        translation.OriginText, 
                    direction:   TranslationDirection.EnRu,
                    absScore:    wordRating ?? 0, 
                    translation: new UserWordTranslation
                    {
                        Transcription = translation.EnTranscription,
                        Word = translation.TranslatedText,
                        Examples = translation.Examples
                            .Select(p => new UserWordTranslationReferenceToExample(p.Id))
                            .ToArray()
                    });
                model.UpdateCurrentScore();
                await _usersWordsService.AddUserWord(model);
                
                user.OnNewWordAdded(
                    pairsCount:    model.Translations.Length,
                    examplesCount: model.Translations.Sum(t => t.Examples?.Length ?? 0));
            }
            else
            {
                // User already have the word.

                var translates = alreadyExistsWord.AllTranslations.ToList();
                var newTranslations = new List<UserWordTranslation>();
                var r = translation;
                if (!translates.Contains(r.TranslatedText))
                    newTranslations.Add(new UserWordTranslation
                    {
                        Transcription = r.EnTranscription,
                        Word = r.TranslatedText,
                        Examples = r.Examples.Select(p => new UserWordTranslationReferenceToExample(p.Id)).ToArray()
                    });
                
                alreadyExistsWord.OnQuestionFailed();
                
                if (newTranslations.Count == 0)
                {
                    await _usersWordsService.UpdateWordMetrics(alreadyExistsWord);
                    return;
                }
                
                alreadyExistsWord.AddTranslations(newTranslations);
                await _usersWordsService.UpdateWord(alreadyExistsWord);
                user.OnPairsAdded(newTranslations.Count,newTranslations.Sum(t => t.Examples?.Length ?? 0));
            }
            await _userService.Update(user);
        }
    }
}