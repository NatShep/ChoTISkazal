using System;
using Chotiskazal.Api.ConsoleModes;
using Chotiskazal.Api.Services;
using Chotiskazal.ConsoleTesting.Services;
using Chotiskazal.DAL;
using Chotiskazal.Dal.Migrations;
using Chotiskazal.Dal.Repo;
using Chotiskazal.Dal.Services;
using Chotiskazal.LogicR.yapi;
using Microsoft.Extensions.Configuration;

namespace Chotiskazal.ConsoleApp
{
    
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            IConfigurationRoot configuration = builder.Build();

            var dbFileName = configuration.GetSection("wordDb").Value;
            
            var yadicapiKey = configuration.GetSection("yadicapi").GetSection("key").Value;
            var yadicapiTimeout = TimeSpan.FromSeconds(5);

            var yatransapiKey = configuration.GetSection("yatransapi").GetSection("key").Value;
            var yatransapiTimeout = TimeSpan.FromSeconds(5);
            
            var addWordService = new AddWordService(
                new UsersPairsService(new UserPairsRepo(dbFileName)), 
                new YandexDictionaryApiClient(yadicapiKey,yadicapiTimeout), 
                new YandexTranslateApiClient(yatransapiKey,yatransapiTimeout),
                new DictionaryService(new DictionaryRepository(dbFileName)));
            
            var examService=new ExamService(
                new UsersPairsService(new UserPairsRepo(dbFileName)), 
                new ExamsAndMetricService(new ExamsAndMetricsRepo(dbFileName)),
                new DictionaryService(new DictionaryRepository(dbFileName)));
            var authorizeService = new AuthorizeService(new UserService(new UserRepo(dbFileName)));
  
            var modes = new IConsoleMode[]
            {   
                new ExamMode(addWordService,examService), 
                new WordAdditionMode(addWordService),
              //  new GraphsStatsMode(),
             //   new RandomizeMode(),
             //   new AddPhraseToWordsMode(yapiDicClient), 
            };

            DoMigration.ApplyMigrations(dbFileName);            
           
            Console.WriteLine("Dic started");
            
            ConsoleKeyInfo val;
            int choice;

            //var metrics = repo.GetAllQuestionMetrics();

            //string path = "T:\\Dictionary\\eng_rus_full.json";
            //Console.WriteLine("Loading dictionary");
            //var dictionary = Dic.Logic.Dictionaries.Tools.ReadFromFile(path);
          
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
     ____ _  _ ____ ___ _ ____ _  _ ____ ___  ____ _    
     |    |__| |  |  |  | [__  |_/  |__|   /  |__| |    
     |___ |  | |__|  |  | ___] | \_ |  |  /__ |  | |___                                                    
");
            Console.ResetColor();
            
            //--------begin registration
            
            User user = null;
            while (user == null)
            {
                Console.WriteLine();
                Console.WriteLine("ESC: Quit");

                Console.WriteLine("1: NewUser");
                Console.WriteLine("2: Login");
                Console.WriteLine();
                Console.Write("Choose action:");
                val = Console.ReadKey();
                Console.WriteLine();
                choice = ((int)val.Key - (int)ConsoleKey.D1) + 1;
                if (choice == 1)
                {
                    Console.Write("Enter your name: ");
                    var name = Console.ReadLine();
                    Console.WriteLine("Enter login: ");
                    var login = Console.ReadLine();
                    Console.WriteLine("Enter password: ");
                    var password = Console.ReadLine();
                    Console.WriteLine("Enter email: ");
                    var email = Console.ReadLine();

                    user = authorizeService.CreateUser(name, login, password, email);
                    
                    if (user != null)
                    {
                        Console.WriteLine("New user has been added!");
                        Console.WriteLine($"Hello, {user.Name}. Press any key to continue.");
                    }
                    else
                        Console.WriteLine("Error in registration. Try again.");
                }
                if (choice == 2)
                {
                    Console.WriteLine("Enter login: ");
                    var login = Console.ReadLine();
                    Console.WriteLine("Enter password: ");
                    var password = Console.ReadLine();
                    
                    user =authorizeService.LoginUser(login,password);
                    if (user != null)
                        Console.WriteLine($"Hello, {user.Name}. Press any key to continue.");
                    else
                        Console.WriteLine("Wrong Password or Name. Try again. Press any key to continue.");
                }
                Console.ReadKey();
            }
            Console.Clear();
            
            //---------end registration-------
            
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

                    modes[choice].Enter(1);

                }
                Console.WriteLine();

                Console.WriteLine("===========================================");
            }
        }

    }
}
