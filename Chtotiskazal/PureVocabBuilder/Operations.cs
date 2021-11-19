using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;

namespace PureVocabBuilder {

public  class Operations {
    private UsersWordsService _userWordService;
    private LocalDictionaryService _localDictionaryService;
    private UserService _userService;
    private LearningSetService _learningSetService;
    private AddWordService _addWordService;
    private ExamplesRepo _examplesRepo;
    private LocalDictionaryRepo _localDictionaryRepo;
    private YandexDictionaryApiClient _yandexDictionaryClient;
    public Operations(UsersWordsService userWordService, LocalDictionaryService localDictionaryService, UserService userService, LearningSetService learningSetService, AddWordService addWordService, ExamplesRepo examplesRepo, LocalDictionaryRepo localDictionaryRepo, YandexDictionaryApiClient yandexDictionaryClient) {
        _userWordService = userWordService;
        _localDictionaryService = localDictionaryService;
        _userService = userService;
        _learningSetService = learningSetService;
        _addWordService = addWordService;
        _examplesRepo = examplesRepo;
        _localDictionaryRepo = localDictionaryRepo;
        _yandexDictionaryClient = yandexDictionaryClient;
    }

    private async Task FindPhrases(AllExamplesDictionary examplesDictionary) {
        var essentialPathResult =
            "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/Zip/FilteredOnlyTranslations.essential";
        var essentialPathWithExamplesResult =
            "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/Zip/CheckPhrasePlease.essential";

        var loaded = ChaosBllHelper.LoadJson<List<EssentialWord>>(essentialPathResult);
        int good = 0;
        int optimized = 0;
        int saved = 0;
        int failures = 0;
        int number = 1;
        foreach (var word in loaded)
        {
            var yaTranlations = await _localDictionaryService.GetTranslationsWithExamplesByEnWord(word.En);
            foreach (var translation in word.Translations)
            {
                var yaTrans = yaTranlations.FirstOrDefault(
                    t => t.TranslatedText.Equals(translation.Ru, StringComparison.InvariantCultureIgnoreCase));
                var phrases = examplesDictionary.GetPhrases(word.En, translation.Ru);
                
                if (yaTrans?.Examples.Any()==true)
                {
                    translation.Phrases.AddRange(yaTrans.Examples.Select(EssentialHelper.ToEssentialPhrase));
                    if (translation.Phrases.All(p => !p.Fits(word.En, translation.Ru)))
                    {
                        //if all origins does not fit - add single clean example
                        if (phrases.Any())
                        {
                            var e = phrases.OrderBy(e => e.En.Length).First();
                            translation.Phrases.Add(e);
                            optimized++;
                        }
                    }
                    else
                    {
                        good++;
                    }

                    continue;
                }
                else
                {
                    if (phrases.Any())
                    {
                        translation.Phrases.AddRange(phrases.OrderBy(e => e.En.Length).Take(3));
                        saved++;
                    }
                    else failures++;
                }
            }

            word.Index = number;
            number++;
        }
        ChaosBllHelper.SaveJson(loaded, essentialPathWithExamplesResult);
    }

    private async Task CheckCophrasing(AllExamplesDictionary examplesDictionary) {
        var essentialPathResult =
            "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/Zip/FilteredOnlyTranslations.essential";

        var loaded = ChaosBllHelper.LoadJson<List<EssentialWord>>(essentialPathResult);
        int succ = 0;
        int fail = 0;
        int noCandidates = 0;
        foreach (var word in loaded)
        {
            var fittedPhrases = word.Translations.SelectMany(t => examplesDictionary.GetPhrases(word.En, t.Ru))
                                    .ToList();
            if (fittedPhrases.Any())
            {
                succ++;
            }
            else
            {
                var candidates = examplesDictionary.GetFor(word.En);
                if (candidates.Count == 0)
                {
                    noCandidates++;
                    var res = await _yandexDictionaryClient.EnRuTranslateAsync(word.En);
                    var examples = res.SelectMany(r => r.Tr).SelectMany(t => t.GetPhrases(word.En)).ToList();
                    //await _examplesRepo.Add(examples);
                }
                else
                {
                    fail++;
                }
            }
        }
    }

    private async Task FilterEssentials() {
        var blackList = new HashSet<string> {
            "en", "harry", "jackson", "indiana", "maryland", "con", "ss", "santa", "diego", "hong", "intel", "maine",
            "sql", "perl", "costa", "navy", "adam", "psp", "caribbean", "nebraska", "delaware", "toshiba",
            "institutional", "attempted", "sue", "communist", "stressed", "shocked", "ego", "aide", "spokesperson",
            "contrasting", "spokeswoman", "extremist", "upsetting"
        };


        var insertList = new Dictionary<string, string> {
            { "secure", "безопасный" },
            { "venture", "отважиться" },
            { "easier", "полегче" }, { "char", "символ" }, { "tight", "тугой" }, { "heating", "обогрев" },
            { "carefully ", "осторожно" }, { "admission ", "допуск" }, { "foster ", "взращивать" },
            { "ward ", "сторожить" }, { "intelligent ", "разумный" }, { "locate ", "найти" }, { "burn ", "ожог" },
            { "belief ", "верование" }, { "attraction ", "притяжение" }, { "adopt ", "усыновить" },
            { "dispute", "разногласия" }, { "bunch ", "связка" }, { "fancy ", "изысканный" }, { "twist ", "крутить" },
            { "retrieve ", "забрать" }, { "fare ", "плата за проезд" }, { "dip ", "окунать" },
            { "nomination ", "номинация" }, { "controversy ", "полемика" }, { "withdraw ", "снять со счета" },
            { "harsh ", "жёсткий" }, { "slam ", "хлопнуть" }, { "terrific ", "ужасающий" }, { "curb ", "бордюр" },
            { "elaborate ", "продуманный" }, { "worthwhile ", "путный" }, { "fierce ", "любопытный" },
            { "infamous ", "печально известный" }, { "tyre ", "уставать" }, { "creep ", "слизняк" },
            { "trait ", "особенность" }, { "soak ", "замочить" }, { "unwilling ", "несклонный" },
            { "overlook ", "проглядеть" }, { "discourage ", "обескураживать" }, { "enact ", "вводить в действие" }
        };
        var replaceList = new Dictionary<string, string>() {
            { "shrug", "пожимать плечами" }, { "premises", "помещение" }, { "incur", "навлечь на себя" },
            { "dispose", "избавляться" }
        };
        var essentialPath =
            "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/Zip/EssentialOnlyTranslations.essential";
        var essentialPathResult =
            "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/Zip/FilteredOnlyTranslations.essential";

        var loaded = ChaosBllHelper.LoadJson<List<EssentialWord>>(essentialPath);
        foreach (string item in blackList)
        {
            var toRemove = loaded.FirstOrDefault(l => l.En == item);
            if (toRemove == null)
                throw new InvalidOperationException();
            loaded.Remove(toRemove);
        }

        foreach (var replacement in replaceList)
        {
            var toReplace = loaded.FirstOrDefault(l => l.En == replacement.Key);
            if (toReplace == null)
                throw new InvalidOperationException();
            toReplace.Translations.Clear();
            toReplace.Translations.Add(new EssentialTranslation(replacement.Value, new List<EssentialPhrase>()));
        }

        foreach (var replacement in insertList)
        {
            var toReplace = loaded.FirstOrDefault(l => l.En == replacement.Key.Trim());
            if (toReplace == null)
                throw new InvalidOperationException();
            if (toReplace.Translations.Any(
                    t => t.Ru.Equals(replacement.Value, StringComparison.InvariantCultureIgnoreCase)))
                throw new InvalidOperationException();

            toReplace.Translations.Insert(
                0, new EssentialTranslation(replacement.Value, new List<EssentialPhrase>()));
        }

        int i = 1;
        var missed = new List<string>();
        foreach (var word in loaded)
        {
            try
            {
                if (word.Translations == null || word.Translations.Count == 0)
                    throw new InvalidOperationException();
                word.Index = i;
                var info = await _localDictionaryRepo.GetOrDefault(word.En.ToLower());
                if (info == null)
                {
                    missed.Add(word.En);
                }
                else
                {
                    word.Transcription = info.Transcription;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            i++;
        }

        Console.WriteLine("its done!");
        ChaosBllHelper.SaveJson(loaded, essentialPathResult);
    }

    private Task MergeUi() {
        var engrupath = "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/Zip/5000ManualSorted.enru";
        var yapienrupath = "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/Zip/5000YapiSorted.enru";
        var essentialPath =
            "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/Zip/EssentialOnlyTranslations.essential";
        var manualEngRu = OpTools.ReadEnru(engrupath);
        var yapiEnRu = OpTools.ReadEnSingleRu(yapienrupath);
        if (manualEngRu.Length != yapiEnRu.Length)
            throw new InvalidOperationException();
        int succ = 0;
        int match = 0;
        int fail = 0;
        var words = new List<EssentialWord>();
        for (int i = 0; i < manualEngRu.Length; i++)
        {
            var manual = manualEngRu[i];
            var yapi = yapiEnRu[i];
            string[] result = null;
            if (manual.Item2.Length == 1 && manual.Item2[0] == yapi.Item2)
            {
                result = manual.Item2;
                succ++;
            }
            else if (manual.Item2.Contains(yapi.Item2))
            {
                result = GetNotFullMatchEnter(i, manual.Item1, yapi.Item2, manual.Item2);
                match++;
            }
            else
            {
                result = GetUnmatchedEnter(i, manual.Item1, yapi.Item2, manual.Item2);
                fail++;
            }

            words.Add(
                new EssentialWord(
                    manual.Item1, null,
                    result.Select(r => new EssentialTranslation(r, new List<EssentialPhrase>())).ToList()));
            ChaosBllHelper.SaveJson(words, essentialPath);
        }

        Console.WriteLine("Validated");
        return Task.CompletedTask;
    }

    private static string[] GetNotFullMatchEnter(int i, string en, string yapi, string[] manuals) {
        Console.Clear();
        Console.WriteLine($"{i} [match]----------------------");
        Console.WriteLine(en);

        var options = new[] { yapi }.Concat(manuals.Where(m => m != yapi)).ToArray();
        int j = 1;
        foreach (string manual in options)
            Console.WriteLine($"{j++}) {manual}");

        return ChooseFromInput(options);
    }

    private static string[] ChooseFromInput(string[] inputOptions) {
        while (true)
        {
            var input = Console.ReadLine();
            var items = input.Split(" ");
            List<string> result = new List<string>();
            foreach (var item in items)
            {
                if (!int.TryParse(item, out var i) || i <= 0 || i > inputOptions.Length)
                {
                    Console.WriteLine("invalid input");
                    continue;
                }

                result.Add(inputOptions[i - 1]);
            }

            return result.ToArray();
        }
    }

    private static string[] GetUnmatchedEnter(int i, string en, string yapi, string[] manuals) {
        Console.Clear();
        Console.WriteLine($"{i} [FAILURE]----------------------");
        Console.WriteLine(en);

        var options = new[] { yapi }.Concat(manuals).ToArray();
        int j = 1;
        foreach (string manual in options)
            Console.WriteLine($"{j++}) {manual}");

        return ChooseFromInput(options);
    }

}

}