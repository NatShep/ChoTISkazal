using Chotiskazal.Dal.Migrations;
using Chotiskazal.Dal.Repo;
using Chotiskazal.Dal.Services;
using Chotiskazal.DAL;
using Chotiskazal.LogicR.yapi;
using ConsoleTesting.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Chotiskazal.Api.ConsoleModes;

namespace ConsoleTesting
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
            var dicRepo = new DictionaryRepository(dbFileName);
            var examsAndMetricRepo = new ExamsAndMetricsRepo(dbFileName);
            var userRepo = new UserRepo(dbFileName);
            var userWordRepo = new UserWordsRepo(dbFileName);

            var dictionaryService = new DictionaryService(dicRepo);
            var examsAndMetricService = new ExamsAndMetricService(examsAndMetricRepo);
            var userService = new UserService(userRepo);
            var userWordService = new UsersWordService(userWordRepo);

            DoMigration.ApplyMigrations(dbFileName);

            var modes = new IConsoleMode[]
            {
                 //      new ExamMode(userWordService,userService,examsAndMetricService),
                new WordAdditionMode(yapiTransClient, yapiDicClient,userWordService,dictionaryService),
             //   new GraphsStatsMode(),
             //   new RandomizeMode(),
             //   new AddPhraseToWordsMode(yapiDicClient), 
            };


            //  Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
     ____ _  _ ____ ___ _ ____ _  _ ____ ___  ____ _    
     |    |__| |  |  |  | [__  |_/  |__|   /  |__| |    
     |___ |  | |__|  |  | ___] | \_ |  |  /__ |  | |___    (test)                                                
");
            Console.ResetColor();
          

                //Registration user


                ConsoleKeyInfo val;
                int choice;
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
                        user = Autorize.CreateNewUser(userService);
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
                        user = Autorize.LoginUser(userService);
                        if (user != null)
                            Console.WriteLine($"Hello, {user.Name}");
                        else
                            Console.WriteLine("Wrong Password or Name. Try again.");
                    }
                    Console.ReadKey();
                }


                Console.Clear();

            /* var users = userRepo.GetAllUsers();
             foreach(var us in users)
             {
                 Console.WriteLine(us.Name + ":" + us.Login + ":" + us.Password);
             }
             */

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("ESC: Quit");

                for (int i = 0; i < modes.Length; i++)
                {
                    Console.WriteLine($"{i + 1}: {modes[i].Name}");
                }

                Console.WriteLine();
                Console.Write("Choose mode:");

                val = Console.ReadKey();
                Console.WriteLine();

                if (val.Key == ConsoleKey.Escape)
                    return;

                choice = ((int)val.Key - (int)ConsoleKey.D1);
                if (choice > -1 && choice < modes.Length)
                {
                    var selected = modes[choice];
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("======   " + selected.Name + "    ======");
                    Console.ResetColor();

                    modes[choice].Enter(user);

                }
                Console.WriteLine();

                Console.WriteLine("===========================================");
            }
        }

    }

    
}
