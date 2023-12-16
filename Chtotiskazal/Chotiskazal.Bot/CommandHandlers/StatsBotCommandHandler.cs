using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;

namespace Chotiskazal.Bot.CommandHandlers;

public class StatsBotCommandHandler : IBotCommandHandler {
    private readonly ExamSettings _settings;

    public StatsBotCommandHandler(ExamSettings settings) {
        _settings = settings;
    }

    public bool Acceptable(string text) => text == BotCommands.Stats;
    public string ParseArgument(string text) => null;

    public Task Execute(string argument, ChatRoom chat) =>
        chat.SendMarkdownMessageAsync(
            StatsRenderer.GetStatsTextMarkdown(_settings, chat),
            new[]
            {
                new[]
                {
                    InlineButtons.MainMenu(chat.Texts),
                    InlineButtons.Exam(chat.Texts),
                },
                new[] { InlineButtons.Translation(chat.Texts) },
                new[]
                {
                    InlineButtons.WellLearnedWords(
                        $"{chat.Texts.ShowWellKnownWords} ({chat.User.CountOf((int)WordLeaningGlobalSettings.WellDoneWordMinScore / 2, 10)}) {Emojis.SoftMark}")
                }
            });
}