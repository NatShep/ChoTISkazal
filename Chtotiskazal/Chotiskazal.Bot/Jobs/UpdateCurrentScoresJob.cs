using System;
using System.Threading.Tasks;
using SayWhat.Bll.Services;
using Serilog;
using Timer = System.Timers.Timer;

namespace Chotiskazal.Bot.Jobs;

public static class UpdateCurrentScoresJob {

    private static DateTime _launchTime;
    private static Timer _timer;

    public static async Task  Launch(ILogger logger, UserService userService, UsersWordsService usersWordsService) {
        _launchTime = DateTime.Today.AddHours(4);
        
        var now = DateTime.Now;
        var delay = now <= _launchTime
            ? (_launchTime - now).TotalMilliseconds
            : TimeSpan.FromDays(1).TotalMilliseconds - (now - _launchTime).TotalMilliseconds;
        logger.Information($"Launch time for {nameof(UpdateCurrentScores)}: {DateTime.Now.AddMilliseconds(delay)}");
       
        await Task.Delay((int)delay);
        logger.Information($"Launched {nameof(UpdateCurrentScores)}");

        await UpdateCurrentScores(userService, usersWordsService, logger);
        _timer = new Timer(TimeSpan.FromDays(1).TotalMilliseconds);
        _timer.Elapsed += async (_, _) => await UpdateCurrentScores(userService, usersWordsService, logger);
        _timer.Enabled = true;
    }

    private static async Task UpdateCurrentScores(UserService userService, UsersWordsService usersWordsService, ILogger logger) {
        var users = userService.GetAllUsers();
        
        logger.Information("Start refreshing current score for users...");
        foreach (var user in users) {
            if (user.TelegramId.ToString() == "326823645" || user.TelegramId.ToString() == "62634148") {
                await usersWordsService.RefreshWordsCurrentScoreAsync(user);
            }
        }
        logger.Information("Refresh current score for users complete");
    }
}