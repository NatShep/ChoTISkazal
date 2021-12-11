using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Hooks;
using Chotiskazal.Bot.Interface;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows {

public class WellKnownWordsHelper {
    public const string PrevData = "/wk<<";
    public const string NextData = "/wk>>";

    public static InlineKeyboardButton[] GetPagingKeys() => new[] {
        new InlineKeyboardButton { CallbackData = PrevData, Text = "<<" },
        new InlineKeyboardButton { CallbackData = NextData, Text = ">>" },
    };
}

public class ShowWellKnownWordsFlow {
    private readonly UsersWordsService _usersWordsService;
    private readonly LeafWellKnownWordsUpdateHook _wellKnownWordsUpdateHook;

    public ShowWellKnownWordsFlow(
        ChatRoom chat,
        UsersWordsService usersWordsService,
        LeafWellKnownWordsUpdateHook wellKnownWordsUpdateHook) {
        Chat = chat;
        _usersWordsService = usersWordsService;
        _wellKnownWordsUpdateHook = wellKnownWordsUpdateHook;
    }

    private ChatRoom Chat { get; }

    public async Task EnterAsync() {
        var wellKnownWords = (await _usersWordsService.GetAllWords(Chat.User))
                             .Where(u => u._absoluteScore >= 4)
                             .ToArray();
        var paginationForWords = new List<List<UserWordModel>>();
        var i = 0;
        while (i < wellKnownWords.Length)
        {
            paginationForWords.Add(wellKnownWords.Skip(i).Take(10).ToList());
            i += 10;
        }

        var msg = new StringBuilder();
        msg.Append("*");

        if (!wellKnownWords.Any())
            msg.Append(Chat.Texts.NoWellKnownWords);
        else if (wellKnownWords.Length == 1)
            msg.Append(Chat.Texts.JustOneLearnedWord);
        else if (wellKnownWords.Length <= 4)
            msg.Append(Chat.Texts.LearnSomeWordsMarkdown(wellKnownWords.Length));
        else
            msg.Append(Chat.Texts.LearnMoreWordsMarkdown(wellKnownWords.Length));

        msg.Append("*\r\n\r\n");

        if (wellKnownWords.Length == 0)
            return;

        var msgWithWords = MarkdownObject.Empty();
        foreach (var word in paginationForWords[0]) {
            msgWithWords += MarkdownObject.Escaped($"{Emojis.SoftMark} ") +
                              MarkdownObject.Escaped($"{word.Word}: ").ToSemiBold() +
                              MarkdownObject.Escaped(word.AllTranslationsAsSingleString)
                                  .AddNewLine();
        }

        if (paginationForWords.Count > 1)
            msgWithWords += Chat.Texts.PageXofYMarkdown(1, paginationForWords.Count);

        InlineKeyboardButton[][] buttons = null;
        if (paginationForWords.Count > 1)
        {
            _wellKnownWordsUpdateHook.SetWellKnownWords(paginationForWords);
            _wellKnownWordsUpdateHook.SetNumberOfPaginate(0);
            
            //TODO зачем это? Выше формируется строка с маркдаун форматированием, но где она применяется? 
            //_wellKnownWordsUpdateHook.SetBeginningMessage(msg.ToString());

            buttons = new[] {
                WellKnownWordsHelper.GetPagingKeys(),
                new[] {
                    InlineButtons.MainMenu($"{Emojis.MainMenu} {Chat.Texts.MainMenuButton}")
                }
            };
        }
        else
            buttons = new[] {
                new[] { InlineButtons.MainMenu($"{Chat.Texts.TranslateButton} {Emojis.Translate}") }
            };

        await Chat.ChatIo.SendMarkdownMessageAsync(msgWithWords, buttons);
    }
}

}