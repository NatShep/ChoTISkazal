using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Dic.AddWords.ConsoleApp.Exams;
using Dic.Logic;
using Dic.Logic.DAL;
using Dic.Logic.Dictionaries;
using Dic.Logic.Services;
using Dic.Logic.yapi;


namespace Dic.AddWords.ConsoleApp
{
    
    class Program
    {
        private static YandexDictionaryApiClient _yapiDicClient;
        private static YandexTranslateApiClient _yapiTransClient;

        static void Main(string[] args)
        {
            _yapiDicClient = new YandexDictionaryApiClient("dict.1.1.20200117T131333Z.11b4410034057f30.cd96b9ccbc87c4d9036dae64ba539fc4644ab33d",
                TimeSpan.FromSeconds(5));

            _yapiTransClient = new YandexTranslateApiClient(
                "trnsl.1.1.20200117T130225Z.a19f679c4b3b6b66.bb3f6acc4e2a62270ef9b80d7ea870960ec45d2c",
                TimeSpan.FromSeconds(5));

            var repo = new WordsRepository("MyWords.sqlite");
            repo.ApplyMigrations();

            Console.WriteLine("Dic started"); 
            //string path = "T:\\Dictionary\\eng_rus_full.json";
            //Console.WriteLine("Loading dictionary");
            //var dictionary = Dic.Logic.Dictionaries.Tools.ReadFromFile(path);
            var service = new NewWordsService(new RuengDictionary(), repo);

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("===========================================");

                Console.WriteLine("ESC: Quit");

                Console.WriteLine("1: Enter words");
                Console.WriteLine("2: Examination");
                Console.WriteLine("3: Randomization");
                Console.WriteLine("4: Stats");

                Console.Write("Choose mode:");

                var val = Console.ReadKey();
                Console.WriteLine();
                switch (val.Key)
                {
                    case ConsoleKey.Escape:
                        return;
                    case ConsoleKey.D1:
                        EnterMode2(service);
                        break;
                    case ConsoleKey.D2:
                        ExamMode(service);
                        break;
                    case ConsoleKey.D3:
                        Randomize(service);
                        break;
                    case ConsoleKey.D4:
                        ShowStats(service);
                        break;

                }
            }
        }

        private static void ShowStats(NewWordsService service)
        {
            var allWords = service.GetAll();
            
            Console.WriteLine($"Context phrases count = {service.GetContextPhraseCount()}");
            Console.WriteLine($"Words count = {allWords.Length}");
            
            var groups = allWords
                .GroupBy(s => s.State)
                .OrderBy(s=>(int)s.Key)
                .Select(s => new {state = s.Key, count = s.Count()});

            var doneCount = 0;
            foreach (var group in groups)
            {
                Console.WriteLine($"{group.state} {group.count}");
                if (group.state == LearningState.Done) 
                doneCount = group.count;
            }
            Console.WriteLine($"Done: {(doneCount*100/allWords.Length)}%");
            Console.WriteLine($"Unknown: {allWords.Length- doneCount} words");


        }

        static void ExamMode(NewWordsService service)
        {
            Console.WriteLine("Examination");
            var words = service.GetPairsForTest(9,3);
            Console.Clear();
            Console.WriteLine("Examination: ");
            foreach (var pairModel in words.Randomize())
            {
                Console.WriteLine($"{pairModel.OriginWord}\t\t:{pairModel.Translation}");
            }
            Console.WriteLine();
            Console.WriteLine("Press any key to start an examination");
            Console.ReadKey();
            Console.Clear();
            for (int examNum = 0; examNum < 2; examNum++)
            { 
                int examsCount = 0;
                int examsPassed = 0;
                DateTime started = DateTime.Now;
                for (int i = 0; i < 3; i++)
                {
                    foreach (var pairModel in words.Randomize())
                    {
                        Console.WriteLine();
                        IExam exam;

                        exam = ExamSelector.GetNextExamFor(i==0, pairModel);
                        bool retryFlag = false;
                        do
                        {
                            retryFlag = false;
                            
                            var result = exam.Pass(service, pairModel, words);
                            switch (result)
                            {
                                case ExamResult.Impossible:
                                    exam = ExamSelector.GetNextExamFor(i == 0, pairModel);
                                    retryFlag = true;
                                    break;
                                case ExamResult.Passed:
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("\r\n[PASSED]");
                                    Console.ResetColor();
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    examsCount++;
                                    examsPassed++;
                                    break;
                                case ExamResult.Failed:
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("\r\n[failed]");
                                    Console.ResetColor();
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    examsCount++;
                                    break;
                                case ExamResult.Retry:
                                    retryFlag = true;
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    break;
                                case ExamResult.Exit: return;
                            }
                        } while (retryFlag);

                    }
                }
                service.UpdateAgingAndRandomize();
                service.RegistrateExam(started, examsCount, examsPassed);

                Console.WriteLine();
                Console.WriteLine($"Test done:  {examsPassed}/{examsCount}");
                Console.WriteLine($"Repeat? [Y]es [N]o");
                var key = Console.ReadKey();
                if(key.Key!= ConsoleKey.Y)
                    break;
            }

            foreach (var pairModel in words)
            {
                Console.WriteLine(pairModel.OriginWord+" - "+ pairModel.Translation+"  ("+ pairModel.PassedScore+")");
            }
            Console.WriteLine();

        }
          

        static void Randomize(NewWordsService service)
        {
            Console.WriteLine("Updating Db");
            service.UpdateAgingAndRandomize();

            var allModels = service.GetAll();
            
            foreach (var pairModel in allModels)
            {
                Console.WriteLine(
                    $"{pairModel.OriginWord} - {pairModel.Translation}   score: {pairModel.PassedScore} / {pairModel.Examed}  {pairModel.AggregateScore:##.##}");
            }
            
            Console.WriteLine($"Has {allModels.Length} models");

            var examSum   = allModels.Sum(a => a.Examed);
            var passedSum = allModels.Sum(a => a.PassedScore);
            var passedAggregatedSum = allModels.Sum(a => Math.Min(1, a.PassedScore/(double)PairModel.MaxExamScore));
            var passedCount = allModels.Count(a => a.PassedScore >= PairModel.MaxExamScore);

            var count = allModels.Length;
            Console.WriteLine($"Knowledge:  {100*passedAggregatedSum/ (double)(count):##.##} %");
            Console.WriteLine($"Known:  {100*passedCount/(double)(count):##.##} %");
            Console.WriteLine($"Failures :  {100*passedSum / (double)(examSum):##.##} %");
        }

        static void EnterMode2(NewWordsService service)
        {
            Console.WriteLine("Enter word mode");
            var dicPing = _yapiDicClient.Ping();
            var transPing = _yapiTransClient.Ping();
            Task.WaitAll(dicPing, transPing);
            var timer = new Timer(5000) { AutoReset = false };
            timer.Enabled = true;
            timer.Elapsed += (s, e) => {
                var pingDicApi = _yapiDicClient.Ping();
                var pingTransApi = _yapiTransClient.Ping();
                Task.WaitAll(pingDicApi, pingTransApi);
                timer.Enabled = true;
            };
            
            if (_yapiDicClient.IsOnline)
                Console.WriteLine("Yandex dic is online");
            else
                Console.WriteLine("Yandex dic is offline");

            if (_yapiTransClient.IsOnline)
                Console.WriteLine("Yandex trans is online");
            else
                Console.WriteLine("Yandex trans is offline");

            while (true)
            {
                Console.Write("Enter eng word: ");
                string word = Console.ReadLine();
                Task<YaDefenition[]> task = null;
                if (_yapiDicClient.IsOnline)
                    task = _yapiDicClient.Translate(word);

                task?.Wait();
                List<TranslationAndContext> translations = new List<TranslationAndContext>();
                if (task?.Result?.Any() == true)
                {
                    var variants = task.Result.SelectMany(r => r.Tr);
                    foreach (var yandexTranslation in variants)
                    {
                        List<Phrase> phrases = new List<Phrase>();
                        if (yandexTranslation.Ex != null)
                        {
                            foreach (var example in yandexTranslation.Ex)
                            {
                                var phrase = new Phrase
                                {
                                    Created = DateTime.Now,
                                    OriginWord = word,
                                    Origin = example.Text,
                                    Translation = example.Tr.FirstOrDefault()?.Text,
                                    TranslationWord = yandexTranslation.Text,
                                };
                                phrases.Add(phrase);
                            }
                        }

                        translations.Add(new TranslationAndContext(word, yandexTranslation.Text, yandexTranslation.Pos, phrases.ToArray()));
                    }
                    
                }
                else
                {
                    var dictionaryMatch = service.GetTranslations(word);
                    if (dictionaryMatch != null)
                    {
                        translations.AddRange(
                            dictionaryMatch.Translations.Select(t =>
                                new TranslationAndContext(dictionaryMatch.Origin, t, dictionaryMatch.Transcription,
                                    new Phrase[0])));
                    }
                }

                if (!translations.Any())
                {
                    try
                    {
                        var transAnsTask = _yapiTransClient.Translate(word);
                        transAnsTask.Wait();
                        
                        if(string.IsNullOrWhiteSpace(transAnsTask.Result))
                        {
                            Console.WriteLine("No translations found. Check the word and try again");
                        }
                        else
                        {
                            translations.Add(new TranslationAndContext(word, transAnsTask.Result, null, new Phrase[0]));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("No translations found. Check the word and try again");
                    }
                }

                if(translations.Any())
                {
                   // Console.WriteLine($"[{translations.Transcription}]");
                    Console.WriteLine("e: [back to main menu]");
                    Console.WriteLine("c: [CANCEL THE ENTRY]");
                    int i = 1;
                    foreach (var translation in translations)
                    {
                        Console.WriteLine($"{i}: {translation.Translation}");
                        i++;
                    }

                    try
                    {
                        var results = ChooseTranslation(translations.ToArray());
                        if (results?.Any() == true)
                        {
                            var allTranslations = results.Select(t => t.Translation).ToArray();
                            var allPhrases = results.SelectMany(t => t.Phrases).ToArray();
                            service.SaveForExams(
                                word: word,
                                transcription: translations[0].Transcription,
                                translations: allTranslations,
                                phrases: allPhrases);
                            Console.WriteLine($"Saved. Tranlations: {allTranslations.Length}, Phrases: {allPhrases.Length}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                }
            }
        }

        static TranslationAndContext[] ChooseTranslation(TranslationAndContext[] translations)
        {
            while (true)
            {
                Console.Write("Choose the word:");
                var res = Console.ReadLine().Trim();
                if(res.ToLower() == "e")
                    throw new OperationCanceledException();
                if (res.ToLower() == "c")
                    return null;

                if (!int.TryParse(res, out var ires))
                {
                    var subItems = res.Split(',');
                    if (subItems.Length > 1)
                    {
                        try
                        {
                            return subItems
                                .Select(s => int.Parse(s.Trim()))
                                .Select(i => translations[i-1])
                                .ToArray();
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }
                    if (res.Length > 1)
                        return new[]{new TranslationAndContext(translations[0].Origin, res, translations[0].Transcription, new Phrase[0])};
                    else continue;
                }
                if (ires == 0)
                    return null;
                if (ires > translations.Length || ires < 0)
                    continue;
                return new []{translations[ires - 1]};
            }
        }
    }
}
