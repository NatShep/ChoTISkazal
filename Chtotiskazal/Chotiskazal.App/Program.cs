using System;
using Chotiskazal.App.Modes;
using Chotiskazal.Logic.DAL;
using Chotiskazal.Logic.Services;
using Dic.Logic.DAL;
using Dic.Logic.Dictionaries;
using Dic.Logic.yapi;
using Microsoft.Extensions.Configuration;

namespace Chotiskazal.App
{
    
    class Program
    {

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            IConfigurationRoot configuration = builder.Build();

            var yadicapiKey = configuration.GetSection("yadicapi").GetSection("key").Value;
            var yadicapiTimeout = TimeSpan.FromSeconds(5);

            var dbFileName = configuration.GetSection("wordDb").Value;
            var yatransapiKey = configuration.GetSection("yatransapi").GetSection("key").Value;
            var yatransapiTimeout = TimeSpan.FromSeconds(5);


            var yapiDicClient = new YandexDictionaryApiClient(yadicapiKey, yadicapiTimeout);

            var yapiTransClient = new YandexTranslateApiClient(yatransapiKey, yatransapiTimeout);

            var modes = new IConsoleMode[]
            {
                new ExamMode(),
                new WordAdditionMode(yapiTransClient, yapiDicClient),
                new GraphsStatsMode(),
                new RandomizeMode(),
            };

            var repo = new WordsRepository(dbFileName);
            repo.ApplyMigrations();
            
            Console.WriteLine("Dic started"); 

            //string path = "T:\\Dictionary\\eng_rus_full.json";
            //Console.WriteLine("Loading dictionary");
            //var dictionary = Dic.Logic.Dictionaries.Tools.ReadFromFile(path);
            var service = new NewWordsService(new RuengDictionary(), repo);
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
     ____ _  _ ____ ___ _ ____ _  _ ____ ___  ____ _    
     |    |__| |  |  |  | [__  |_/  |__|   /  |__| |    
     |___ |  | |__|  |  | ___] | \_ |  |  /__ |  | |___                                                    
");
            Console.ResetColor();
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("ESC: Quit");

                for (int i = 0; i < modes.Length; i++)
                {
                    Console.WriteLine($"{i+1}: {modes[i].Name}");
                }

                Console.WriteLine();
                Console.Write("Choose mode:");

                var val = Console.ReadKey();
                Console.WriteLine();

                if(val.Key== ConsoleKey.Escape)
                    return;

                var choice = ((int) val.Key - (int) ConsoleKey.D1);
                if (choice > -1 && choice < modes.Length)
                {
                    var selected = modes[choice];
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("======   "+ selected.Name + "    ======");
                    Console.ResetColor();

                    modes[choice].Enter(service);

                }
                Console.WriteLine();

                Console.WriteLine("===========================================");
            }
        }

    }
}
