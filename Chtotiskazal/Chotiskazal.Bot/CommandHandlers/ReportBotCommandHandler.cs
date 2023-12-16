using System.Threading.Tasks;
using Microsoft.VisualBasic;
using SayWhat.Bll;
using SayWhat.Bll.Strings;

namespace Chotiskazal.Bot.CommandHandlers;

public class ReportBotCommandHandler : IBotCommandHandler {
    public static readonly ReportBotCommandHandler Instance = new();
    private ReportBotCommandHandler() { }
    public bool Acceptable(string text) => text == BotCommands.Report;
    public string ParseArgument(string text) => null;

    public async Task Execute(string argument, ChatRoom chat) {
        var history = chat.ChatIo.TryGetChatHistory();
        var header = $"sent by @{chat.User.TelegramNick}\r\n:" +
                     $"{chat.User.TelegramId}:{chat.User.TelegramFirstName}-{chat.User.TelegramLastName}\r\n";
        var message = Markdown.Escaped($"Report {header}")
            .NewLine()
            .AddMarkdown(Markdown.Escaped(Strings.Join(history, "\r\n")).ToQuotationMono());
        Reporter.ReportUserIssue(message.GetMarkdownString());
        await chat.SendMessageAsync(chat.Texts.ReportWasSentEnterAdditionalInformationAboutTheReport);

        var userComments = await chat.WaitUserTextInputAsync();
        Reporter.ReportUserIssue($"Comment: {header}\r\n '\r\n{userComments}\r\n'");
        await chat.SendMessageAsync(chat.Texts.ThankYouForYourCommentInReport);
    }
}