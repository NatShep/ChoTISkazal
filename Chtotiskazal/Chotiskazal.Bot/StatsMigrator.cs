using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using MongoDB.Driver;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot {

public class StatsMigrator {
    public static async Task Do(IMongoDatabase db) {
        try
        {
            var userWordRepo = new UserWordsRepo(db);
            var dictionaryRepo = new DictionaryRepo(db);
            var userRepo = new UsersRepo(db);
            var examplesRepo = new ExamplesRepo(db);
            var allUsers = userRepo.GetAll();
            var usersWordsService = new UsersWordsService(userWordRepo, examplesRepo);


            foreach (var user in allUsers)
            {
                Console.WriteLine($"Checking {user.TelegramNick} {user.TelegramFirstName}");

                if (user.WordsCount != user.CountOf(0, 100))
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    var allWords = await userWordRepo.GetAllWords(user);
                    foreach (var word in allWords)
                    {
                        word._absoluteScore = word.AbsoluteScore / 3;
                        word.UpdateCurrentScore();
                    }

                    user.RecreateStatistic(allWords);
                    foreach (var word in allWords)
                    {
                        await usersWordsService.UpdateWordMetrics(word);
                    }

                    await userRepo.Update(user);
                    sw.Stop();
                    Console.WriteLine($"Done for {user.TelegramNick} {user.TelegramFirstName} with {allWords.Count}");
                }
                else
                {
                    Console.WriteLine($"[skip]");
                }
            }

            Console.WriteLine("Migration done");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}

}