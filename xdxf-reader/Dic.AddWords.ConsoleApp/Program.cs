using System;
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
        private static YandexApiClient _yapiClient;
        static void Main(string[] args)
        {
            _yapiClient = new YandexApiClient("dict.1.1.20200117T131333Z.11b4410034057f30.cd96b9ccbc87c4d9036dae64ba539fc4644ab33d",
                TimeSpan.FromSeconds(5));
            

            WordsRepository.ApplyMigrations();

            Console.WriteLine("Dic started");
            string path = "T:\\Dictionary\\eng_rus_full.json";
            Console.WriteLine("Loading dictionary");
            var dictionary = Dic.Logic.Dictionaries.Tools.ReadFromFile(path);
            var service = new NewWordsService(dictionary, new WordsRepository());

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
                        EnterMode(service);
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
            var words = service.GetPairsForTest(9);
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

                        exam = ExamHelper.GetNextExamFor(i==0, pairModel);
                        bool retryFlag = false;
                        do
                        {
                            retryFlag = false;
                            Console.WriteLine();
                            Console.WriteLine("***** " + exam.Name + " *****");
                            Console.WriteLine();

                            var result = exam.Pass(service, pairModel, words);
                            switch (result)
                            {
                                case ExamResult.Passed:
                                    Console.WriteLine("\r\n[PASSED]");
                                    examsCount++;
                                    examsPassed++;
                                    break;
                                case ExamResult.Failed:
                                    Console.WriteLine("\r\n[failed]");
                                    examsCount++;
                                    break;
                                case ExamResult.Retry:
                                    retryFlag = true;
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

        static void EnterMode(NewWordsService service)
        {
            Console.WriteLine("Enter word mode");
            _yapiClient.Ping().Wait();
            var timer = new Timer(5000){AutoReset = false};
            timer.Enabled = true;
            timer.Elapsed += (s,e) => {
                _yapiClient.Ping().Wait();
                timer.Enabled = true;
            };
            if(_yapiClient.IsOnline)
                Console.WriteLine("Yandex is online");
            else
                Console.WriteLine("Yandex is offline");

            while (true)
            {
                Console.Write("Enter eng word: ");
                string word = Console.ReadLine();
                Task<YaDefenition[]> task = null;
                if (_yapiClient.IsOnline)
                    task = _yapiClient.Translate(word);

                var alreadyExisted = service.Get(word);
                if (alreadyExisted != null)
                {
                    Console.WriteLine("Already found: "+ alreadyExisted.Translation);
                    continue;
                }
                var translations = service.GetTranslations(word);
                task?.Wait();
                if (task?.Result?.Any() == true)
                {
                    var yaTranslated = task.Result.SelectMany(r => r.Tr).Select(r => r.Text).ToArray();
                    translations =  new DictionaryMatch(word, task.Result.First().Ts, yaTranslated);
                }
                if (translations == null)
                {
                    Console.WriteLine("No translations found. Check the word and try again");
                }
                else
                {
                    Console.WriteLine($"[{translations.Transcription}]");
                    Console.WriteLine("0: [CANCEL THE ENTRY]");
                    int i = 1;
                    foreach (var translation in translations.Translations)
                    {
                        Console.WriteLine($"{i}: {translation}");
                        i++;
                    }

                    var result =  ChooseTranslation(translations);
                    if(result==null)
                    {
                    }
                    else
                    {
                        service.SaveForExams(translations.Origin, result, translations.Transcription);
                        Console.WriteLine("Saved.");
                    }
                }
            }
        }

        static string ChooseTranslation(DictionaryMatch match)
        {
            while (true)
            {
                Console.Write("Choose the word:");
                var res = Console.ReadLine();
                if (!int.TryParse(res, out var ires) )
                {
                    if (res.Length > 1)
                        return res;
                    else continue;
                }
                if (ires == 0)
                    return null;
                if(ires> match.Translations.Length|| ires<0)
                    continue;
                return match.Translations[ires - 1];
            }
        }
    }
}
