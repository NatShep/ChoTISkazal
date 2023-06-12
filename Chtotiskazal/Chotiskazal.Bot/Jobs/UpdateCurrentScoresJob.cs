using System;
using System.Threading.Tasks;
using SayWhat.Bll.Services;
using Serilog;
using Timer = System.Timers.Timer;

namespace Chotiskazal.Bot.Jobs {
public static class UpdateCurrentScoresJob {

    private static DateTime _launchTime;
    private static Timer _timer;

    public static async Task  Launch(ILogger logger, UserService userService, UsersWordsService usersWordsService) {
        _launchTime = DateTime.Today.AddHours(14).AddMinutes(53);
        
        var now = DateTime.Now;
        var delay = now <= _launchTime
            ? (_launchTime - now).TotalMilliseconds
            : TimeSpan.FromDays(1).TotalMilliseconds - (now - _launchTime).TotalMilliseconds;
        logger.Information($"Launch time for {nameof(UpdateCurrentScores)}: {DateTime.Now.AddMilliseconds(delay)}");
       
        await Task.Delay((int)delay);
        await UpdateCurrentScores(userService, usersWordsService);

        _timer = new Timer(TimeSpan.FromDays(1).TotalMilliseconds);

        _timer.Elapsed += async (_, _) => await UpdateCurrentScores(userService, usersWordsService);
        _timer.Enabled = true;
        logger.Information($"Launched {nameof(UpdateCurrentScores)}");
    }

    private static async Task UpdateCurrentScores(UserService userService, UsersWordsService usersWordsService) {
        var users = userService.GetAllUsers();

        foreach (var user in users) {
            if (user.TelegramId.ToString() == "326823645") {
                await usersWordsService.RefreshWordsCurrentScoreAsync(user);
            }
        }
    }
}
}