using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll.Dto;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.LearningSets;

namespace SayWhat.Bll.Services;

public class EssentialService {
    private readonly ExamplesRepo _examplesRepo;
    private readonly LocalDictionaryRepo _localDictionaryRepo;
    private readonly AddWordService _addWordService;
    private readonly LearningSetsRepo _learningSetsRepo;

    public EssentialService(
        ExamplesRepo examplesRepo, LocalDictionaryRepo localDictionaryRepo, AddWordService addWordService,
        LearningSetsRepo learningSetsRepo) {
        _examplesRepo = examplesRepo;
        _localDictionaryRepo = localDictionaryRepo;
        _addWordService = addWordService;
        _learningSetsRepo = learningSetsRepo;
    }

    public async Task<List<WordInLearningSet>> MergeEssentials(IList<EssentialWord> esWords) {
        // для каждого слова - смотрим есть ли это слово в  бд
        // если нет - то создаем и добавляем фразы с ним связанные, сверху докатываем Yandex перевод из словаря
        // если есть - то:
        //      для каждого перевод смотрим есть ли такой перевод в словаре 
        //      если нет то добавляем (на соотв позицию) и добавляем фразы
        //      если есть то проверяем позицию и проверяем наличие фраз
        //      
        int i = 0;
        int added = 0;
        int merged = 0;
        var dictionariesCountBefore = await _localDictionaryRepo.GetCount();
        var examplesCOuntBefore = await _examplesRepo.GetCount();
        var allExamples = (await _examplesRepo.GetAll()).ToDictionary(e => e.Id, e => e);

        int foundExamples = 0;
        int lostExamples = 0;
        var results = new List<WordInLearningSet>();

        Console.WriteLine(
            $"Essentials: {esWords.Count}, Dictionaries: {dictionariesCountBefore}, Examples: {examplesCOuntBefore}");

        foreach (var essentialWord in esWords) {
            i++;
            Console.Write($"[{i} of {esWords.Count}] Word: '{essentialWord.En}'");
            try {
                var dicword = await _localDictionaryRepo.GetOrDefault(essentialWord.En);
                if (dicword == null) {
                    Console.Write($" [add] ...\r\n");
                    var tsword = await CreateWordInLocalDictionary(essentialWord);
                    results.Add(tsword);
                    added++;
                }
                else {
                    dicword.LoadExamples(allExamples);
                    Console.Write($" [merge] ...\r\n");
                    var tsword = await MergeEssentialAndLocalDictionaryWords(essentialWord, dicword);
                    results.Add(tsword);
                    merged++;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }

        Console.WriteLine($"FoundExamples: {foundExamples}, lostExamples: {lostExamples}");

        Console.WriteLine($"Added: {added}, Merged: {merged}");
        Console.WriteLine(
            $"Counts before: essentials: {esWords.Count}, Dictionaries: {dictionariesCountBefore}, Examples: {examplesCOuntBefore}");
        var dictionariesCountAfter = await _localDictionaryRepo.GetCount();
        var examplesCOuntAfter = await _examplesRepo.GetCount();
        Console.WriteLine(
            $"Counts after: essentials: {esWords.Count}, Dictionaries: {dictionariesCountAfter},Examples: {examplesCOuntAfter}");
        return results;
    }

    /// <summary>
    /// Merge all essentials words into the db and create learning set
    /// </summary>
    public async Task CreateLearningSet(LearningSetDescription learningSetDescription) {
        if ((await _learningSetsRepo.GetAll()).Any(a => a.ShortName == learningSetDescription.ShortName))
            throw new InvalidOperationException(learningSetDescription.ShortName + " already existing in db");

        var words = await MergeEssentials(learningSetDescription.Words);
        var learningSet = new LearningSet
        {
            Enabled = true,
            EnDescription = learningSetDescription.EnDescription,
            EnName = learningSetDescription.EnName,
            Id = ObjectId.GenerateNewId(),
            RuDescription = learningSetDescription.RuDescription,
            RuName = learningSetDescription.RuName,
            ShortName = learningSetDescription.ShortName,
            Words = words
        };
        await _learningSetsRepo.Add(learningSet);
    }

    private async Task<WordInLearningSet> MergeEssentialAndLocalDictionaryWords(
        EssentialWord esWord, DictionaryWord dicword) {
        Console.Write("Merge...");
        //      для каждого перевод смотрим есть ли такой перевод в словаре 
        //      если нет то добавляем (на соотв позицию) и добавляем фразы
        //      если есть то проверяем позицию и проверяем наличие фраз
        if (!string.IsNullOrWhiteSpace(esWord.Transcription)) {
            if (dicword.Transcription != esWord.Transcription)
                dicword.Transcription = esWord.Transcription;
        }

        var resultTranlsations = new List<DictionaryTranslationDbEntity>();
        var examplesToAdd = new List<Example>();
        var essentialsObjectIds = new List<ObjectId>();
        foreach (var translation in esWord.Translations) {
            var existingTranslation = dicword.GetTranslationOrNull(translation.Ru);
            if (existingTranslation == null) {
                var (newExamplesForTranslation, newTranslation) = CreateNewTranslationModels(esWord, translation);
                examplesToAdd.AddRange(newExamplesForTranslation);
                resultTranlsations.Add(newTranslation);
                essentialsObjectIds.AddRange(newExamplesForTranslation.Select(e => e.Id));
            }
            else {
                //уже есть перевод. 
                resultTranlsations.Add(existingTranslation);
                //Подгружаем Examples
                //Нужно проверять какие фразы есть в нем, а каких нет
                var alreadyPhrases =
                    existingTranslation.Examples
                        .Where(e => e.ExampleOrNull != null)
                        .Select(e => e.ExampleOrNull.ToEssentialPhrase())
                        .ToArray();

                var newEssentialPhrases = translation.Phrases.Except(alreadyPhrases).ToArray();
                if (newEssentialPhrases.Any()) {
                    if (alreadyPhrases.Any()) { }

                    var newExamplesForTranslation =
                        newEssentialPhrases.SelectToArray(b => b.ToExample(esWord, translation));
                    examplesToAdd.AddRange(newExamplesForTranslation);
                    //Вставляем новые фразы в начало
                    existingTranslation.Examples = newExamplesForTranslation
                        .Select(e => new DictionaryReferenceToExample(e))
                        .Concat(
                            existingTranslation
                                .Examples //удаляем 'битые' примеры 
                                .Where(e => e.ExampleOrNull != null))
                        .ToArray();
                }

                var essentialIds = existingTranslation.Examples
                    .Where(
                        e => translation.Phrases.Any(
                            p => p.En.Equals(
                                e.ExampleOrNull.OriginPhrase,
                                StringComparison.OrdinalIgnoreCase)))
                    .Select(e => e.ExampleId);
                essentialsObjectIds.AddRange(essentialIds);
            }
        }

        dicword.Translations = resultTranlsations.ToArray();
        if (examplesToAdd.Any())
            await _examplesRepo.Add(examplesToAdd);
        await _localDictionaryRepo.Update(dicword);
        Console.Write("\r\n");
        return new WordInLearningSet
        {
            Word = esWord.En,
            AllowedTranslations = esWord.Translations.SelectToArray(t => t.Ru),
            AllowedExamples = essentialsObjectIds.ToArray()
        };
    }

    private (Example[] newExamplesForTranslation, DictionaryTranslationDbEntity newTranslation)
        CreateNewTranslationModels(EssentialWord esWord, EssentialTranslation translation) {
        var newExamplesForTranslation = translation
            .Phrases
            .SelectToArray(p => p.ToExample(esWord, translation));
        var newTranslation = new DictionaryTranslationDbEntity
        {
            Language = Language.Ru,
            Word = translation.Ru,
            Examples = newExamplesForTranslation.SelectToArray(
                e => new DictionaryReferenceToExample(e))
        };
        return (newExamplesForTranslation, newTranslation);
    }

    private async Task<WordInLearningSet> CreateWordInLocalDictionary(EssentialWord essentialWord) {
        Console.Write("Get word translation...");
        //load translations from yandex translate
        await _addWordService.TranslateAndAddToDictionary(essentialWord.En);
        var dicword = await _localDictionaryRepo.GetOrDefault(essentialWord.En);
        if (dicword == null) {
            throw new InvalidOperationException();
        }

        return await MergeEssentialAndLocalDictionaryWords(essentialWord, dicword);
    }
}