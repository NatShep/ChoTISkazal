using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;

namespace Chotiskazal.Bot.CommandHandlers;

public class InternalShowWordsStats : IBotCommandHandler
{
    private readonly FrequentWordService _frequentWordService;

    public InternalShowWordsStats(FrequentWordService frequentWordService)
    {
        _frequentWordService = frequentWordService;
    }

    public bool Acceptable(string text) => text == "/freqstats";

    public string ParseArgument(string text) => null;

    public async Task Execute(string argument, ChatRoom chat)
    {
        var freqWordCount = await _frequentWordService.Count();
        var selector = new FreqWordsSelector(chat.User.OrderedFrequentItems.ToList(), freqWordCount);
        var section = selector.CalcCentralSection();
        var distributionLeft = selector.GetDistribution(section.Left);
        var distributionRight = selector.GetDistribution(section.Right);

        var message = Markdown.Escaped($"User @{chat.User.TelegramNick} freq word stats ").NewLine()
                      + Markdown.Escaped($"Count: {selector.Count}").NewLine()
                      + Markdown.Escaped($"Central selection: [{section.Left}:{section.Right}]").NewLine()
                      + Markdown.Escaped($"Estimated words known: {section.Left + (section.Right - section.Left) / 2}")
                          .NewLine()
                      + Markdown.Escaped($"R <= interval: {distributionLeft.LtRed}").NewLine()
                      + Markdown.Escaped($"G <= interval: {distributionLeft.LtGreen}").NewLine()
                      + Markdown.Escaped($"R >= interval: {distributionRight.GtRed}").NewLine()
                      + Markdown.Escaped($"G >= interval: {distributionRight.GtGreen}").NewLine();
        await chat.SendMarkdownMessageAsync(message.ToQuotationMono());
    }
}