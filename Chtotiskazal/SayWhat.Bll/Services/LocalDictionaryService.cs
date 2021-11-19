using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll.Dto;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.Bll.Services {

public class LocalDictionaryService {
    private readonly LocalDictionaryRepo _dicRepository;
    private readonly ExamplesRepo _exampleRepository;

    public LocalDictionaryService(LocalDictionaryRepo repository, ExamplesRepo exampleRepository) {
        _dicRepository = repository;
        _exampleRepository = exampleRepository;
    }

    /*public Task AddNewWordPairToDictionaryAsync(
        string enword, 
        string ruword, 
        string transcription, 
        TranslationSource sourse)
    {
        var word = new MongoDAL.Dictionary.DictionaryWord
        {
            Id = ObjectId.GenerateNewId(),
            Language = Language.En,
            Source = sourse,
            Transcription = transcription,
            Word = enword,
            Translations = new[]
            {
                new MongoDAL.Dictionary.DictionaryTranslation
                {
                    Word = ruword,
                    Language = Language.Ru,
                    Id = ObjectId.GenerateNewId()
                }
            }
        };
        return _dicRepository.Add(word);
    }*/

    /*
    public Task AddNewWordPairToDictionaryWithPhrasesAsync(string enword, string ruword,
        string transcription, TranslationSource source, List<Phrase> examples)
    {
        var word = new MongoDAL.Dictionary.DictionaryWord
        {
            Id = ObjectId.GenerateNewId(),
            Language = Language.En,
            Source = source,
            Transcription = transcription,
            Word = enword,
            Translations = new[]
            {
                new MongoDAL.Dictionary.DictionaryTranslation
                {
                    Word = ruword,
                    Language = Language.Ru,
                    Id = ObjectId.GenerateNewId(),
                    Examples = examples.Select(p=>new ReferenceToExample
                    {
                        OriginExample =  p.EnPhrase,
                        TranslationExample = p.PhraseRuTranslate
                    }).ToArray()
                }
            }
        };
        return _dicRepository.Add(word);
    }
    */

    public async Task AppendRuTranslation(string en, params (string ru, ObjectId[] exampleIds)[] translations) {
        var (localWord, _) = await GetTranslationWithExamplesByEnWord(en);
        if (localWord == null)
            throw new InvalidOperationException($"Word {en} not found in local dictionary");
        var newTranslations = translations.Where(
                                              t => !localWord.Translations.Any(
                                                  l => l.Word.Equals(
                                                      t.ru, StringComparison.InvariantCultureIgnoreCase)))
                                          .ToList();
        if (newTranslations.Any())
        {
            var localTranslations = localWord.Translations.ToList();
            foreach (var newTranslation in newTranslations)
            {
                localTranslations.Add(
                    new DictionaryTranslationDbEntity {
                        Language = Language.Ru,
                        Word = newTranslation.ru,
                        Examples = Array.Empty<DictionaryReferenceToExample>()
                    });
            }

            localWord.Translations = localTranslations.ToArray();
            await _dicRepository.Update(localWord);
        }
    }

    public async Task<List<DictionaryWord>> GetAll() {
        var allWords  = await _dicRepository.GetAll();
        var allExamples = await _exampleRepository.GetAll();
        var examples = allExamples.ToDictionary(k => k.Id, v => v);
        foreach (var word in allWords)
        {
            foreach (var trans in word.Translations)
            {
                foreach (var example in trans.Examples)
                {
                    example.TryLoadExample(examples);
                }
            }
        }
        return allWords;
    }
    public async Task<string[]> GetAllTranslationWords(string enword) {
        var results = await _dicRepository.GetOrDefault(enword);
        if (results == null)
            return new string[0];
        return results.Translations.Select(t => t.Word).ToArray();
    }

    public async Task<IReadOnlyList<Translation>> GetTranslationsWithExamplesByEnWord(string enword) {
        var (_, translations) = await GetTranslationWithExamplesByEnWord(enword);
        return translations;
    }
    
    public async Task<(DictionaryWord,  IReadOnlyList<Translation>)> GetTranslationWithExamplesByEnWord(string enword) {
        var word = await _dicRepository.GetOrDefault(enword);
        var translations = await GetTranslationsWithExamples(word);
        return (word, translations);
    }

    private async Task<IReadOnlyList<Translation>> GetTranslationsWithExamples(DictionaryWord word) {
        if (word == null)
            return Array.Empty<Translation>();

        var result = new List<Translation>();
        foreach (var translation in word.Translations)
        {
            var examples = await GetExamples(translation.Examples.Select(e => e.ExampleId));
            result.Add(
                new Translation(
                    word.Word,
                    translation.Word,
                    word.Transcription,
                    word.Language == Language.En ? TranslationDirection.EnRu : TranslationDirection.RuEn,
                    word.Source, examples));
        }

        return result;
    }

    private async Task<List<Example>> GetExamples(IEnumerable<ObjectId> examplesId) => 
        examplesId.Any() 
            ? await _exampleRepository.GetAll(examplesId) 
            : new List<Example>();

    public async Task<IReadOnlyList<Translation>> GetTranslationsWithoutExamples(string enword) {
        var word = await _dicRepository.GetOrDefault(enword);
        if (word == null)
            return new Translation[0];

        return word.Translations
                   .Select(
                       translation => new Translation(
                           originText: word.Word,
                           translatedText: translation.Word,
                           originTranscription: word.Transcription,
                           translationDirection: word.Language == Language.En
                               ? TranslationDirection.EnRu
                               : TranslationDirection.RuEn,
                           source: word.Source,
                           translation.Examples
                                      .Select(e => new Example { Id = e.ExampleId })
                                      .ToList()))
                   .ToList();
    }
    /// <summary>
    /// Adds word, its translation and examples to database
    /// </summary>
    /// <param name="word"></param>
    public async Task AddNewWord(DictionaryWord word) {
        var allExamples = word.Translations
                              .SelectMany(t => t.Examples)
                              .Select(e => e.ExampleOrNull)
                              .Where(e => e != null);
        await _exampleRepository.Add(allExamples);
        await _dicRepository.Add(word);
    }
}

}