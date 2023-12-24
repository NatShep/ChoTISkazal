using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using Serilog;

namespace Chotiskazal.Bot.Jobs;

public static class MutualPhraseJob {
    private static IList<Example> _allExamples = null;

    public static async Task Launch(MutualPhrasesService mutualPhrasesService, UserService userService,
        ILogger logger, int launchHour) {
        await Task.Delay(TimeSpan.FromMinutes(1));

        while (true) {
            if (DateTime.Now.Hour == launchHour) {
                await Do(mutualPhrasesService, userService, logger);
                await Task.Delay(TimeSpan.FromHours(23.5));
            }

            await Task.Delay(TimeSpan.FromHours(0.5));
        }
    }

    private static async Task Do(MutualPhrasesService mutualPhrasesService, UserService userService,
        ILogger logger) {
        logger.Information($"Launch {nameof(MutualPhraseJob)}");
        var sw = Stopwatch.StartNew();
        var users = userService.GetAllUsers();
        logger.Debug("Load examples");
        _allExamples ??= await mutualPhrasesService.GetAllExamples();
        logger.Debug($"{_allExamples.Count} Examples are loaded");
        var totalCount = 0;
        var userCount = 0;
        foreach (var user in users) {
            var count = await Launch(user, mutualPhrasesService, _allExamples, logger);
            if (count > 0) {
                totalCount += count;
                userCount++;
            }
        }

        sw.Stop();
        logger.Information(
            "Mutual phrase job results: \r\n" +
            $"Phrases added: {totalCount} for {userCount} users from {_allExamples.Count} samples \r\n" +
            $"in {sw.Elapsed.Seconds}");
    }

    private static async Task<int> Launch(UserModel user, MutualPhrasesService mutualPhrasesService,
        IList<Example> examples, ILogger logger) {
        if (DateTime.Now - user.LastActivity > TimeSpan.FromDays(20)) {
            logger.Debug($"Mutual: Skip user {user.TelegramNick} because of inactivity");
            return 0;
        }

        var phrases = await mutualPhrasesService.FindMutualPhrases(user, examples);
        logger.Debug($"Mutual: {phrases.Count} phrases found. Add phrases");
        var count = await mutualPhrasesService.AddMutualPhrasesToUser(user, phrases);
        logger.Debug($"Mutual: {count} new phrases added for user {user.TelegramNick}");
        return count;
    }
}