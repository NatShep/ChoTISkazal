using System;
using Chotiskazal.App.Modes;
using Chotiskazal.App.Autorisation;
using Chotiskazal.Logic.DAL;
using Chotiskazal.Logic.Services;
using Dic.Logic.DAL;
using Dic.Logic.Dictionaries;
using Dic.Logic.yapi;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

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
            var yadicapiTimeout = TimeSpan.FromSeconds(5); //Configuration.GetValue<TimeSpan>("yadicapi:timeout");

            var dbFileName = configuration.GetSection("wordDb").Value;
            var yatransapiKey = configuration.GetSection("yatransapi").GetSection("key").Value;
            var yatransapiTimeout = TimeSpan.FromSeconds(5); //Configuration.GetValue<TimeSpan>("yatransapi:timeout");


            var yapiDicClient = new YandexDictionaryApiClient(yadicapiKey, yadicapiTimeout);

            var yapiTransClient = new YandexTranslateApiClient(yatransapiKey, yatransapiTimeout);

            var modes = new IConsoleMode[]
            {
                new ExamMode(), 
                new WordAdditionMode(yapiTransClient, yapiDicClient),
                new GraphsStatsMode(),
             //   new RandomizeMode(),
             //   new AddPhraseToWordsMode(yapiDicClient), 
            };

  

            var wordsRepo = new WordsRepository(dbFileName);
            var userRepo = new UserRepo(dbFileName);
           
            wordsRepo.ApplyMigrations();
            
            Console.WriteLine("Dic started");

            //var metrics = repo.GetAllQuestionMetrics();

            //string path = "T:\\Dictionary\\eng_rus_full.json";
            //Console.WriteLine("Loading dictionary");
            //var dictionary = Dic.Logic.Dictionaries.Tools.ReadFromFile(path);
          
            var wordService = new NewWordsService(new RuEngDictionary(), wordsRepo);
            var userService = new UserService(userRepo);
            

        //  Console.Clear();
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

                //Registration user

               
                ConsoleKeyInfo val;
                int choice;
                User user = null;
                while(user==null)
                {
                    Console.WriteLine("1: NewUser");
                    Console.WriteLine("2: Login");
                    Console.WriteLine();
                    Console.Write("Choose action:");
                    val = Console.ReadKey();
                    choice = ((int)val.Key - (int)ConsoleKey.D1);
                    if (choice==1)
                    {
                        Autorize.CreateNewUser(userRepo);
                        user=Autorize.LoginUser(userRepo);
                    }
                    if (choice==2)
                    {
                        user=Autorize.LoginUser(userRepo);
                    }
                    Console.Clear();

                }

                Console.Clear();

                for (int i = 0; i < modes.Length; i++)
                {
                    Console.WriteLine($"{i+1}: {modes[i].Name}");
                }

                Console.WriteLine();
                Console.Write("Choose mode:");

                val = Console.ReadKey();
                Console.WriteLine();

                if(val.Key== ConsoleKey.Escape)
                    return;

                choice = ((int) val.Key - (int) ConsoleKey.D1);
                if (choice > -1 && choice < modes.Length)
                {
                    var selected = modes[choice];
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("======   "+ selected.Name + "    ======");
                    Console.ResetColor();

                    modes[choice].Enter(wordService);

                }
                Console.WriteLine();

                Console.WriteLine("===========================================");
            }
        }

    }
}
