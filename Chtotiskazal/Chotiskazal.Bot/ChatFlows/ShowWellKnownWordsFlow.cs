using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Hooks;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows;

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
            .Where(u => u.AbsoluteScore >= WordLeaningGlobalSettings.WellDoneWordMinScore)
            .ToArray();
        var paginationForWords = new List<List<UserWordModel>>();
        var i = 0;
        while (i < wellKnownWords.Length)
        {
            paginationForWords.Add(wellKnownWords.Skip(i).Take(10).ToList());
            i += 10;
        }
        
        var message = Markdown.Empty;
        
        if (!wellKnownWords.Any())
            message.AddEscaped(Chat.Texts.NoWellKnownWords);
        else if (wellKnownWords.Length == 1)
            message.AddEscaped(Chat.Texts.JustOneLearnedWord);
        else if (wellKnownWords.Length <= 4)
            message.AddMarkdown(Chat.Texts.LearnSomeWords(wellKnownWords.Length));
        else
            message.AddMarkdown(Chat.Texts.LearnMoreWords(wellKnownWords.Length));

        message = message.ToSemiBold().NewLine().NewLine();

        if (wellKnownWords.Length == 0)
            return;

        var msgWithWords = Markdown.Empty;
        foreach (var word in paginationForWords[0]) {
            msgWithWords += Markdown.Escaped($"{Emojis.SoftMark} ") +
                            Markdown.Escaped($"{word.Word}: ").ToSemiBold() +
                            Markdown.Escaped(word.AllTranslationsAsSingleString)
                                .NewLine();
        }

        if (paginationForWords.Count > 1)
            msgWithWords += Chat.Texts.PageXofY(1, paginationForWords.Count);

        InlineKeyboardButton[][] buttons = null;
        if (paginationForWords.Count > 1)
        {
            _wellKnownWordsUpdateHook.SetWellKnownWords(paginationForWords);
            _wellKnownWordsUpdateHook.SetNumberOfPaginate(0);
            
            buttons = new[] {
                WellKnownWordsHelper.GetPagingKeys(),
                new[] {
                    InlineButtons.MainMenu(Chat.Texts)
                }
            };
        }
        else
            buttons = new[] {
                new[] { InlineButtons.MainMenu($"{Chat.Texts.TranslateButton} {Emojis.Translate}") }
            };

        await Chat.ChatIo.SendMessageAsync(msgWithWords, buttons);
    }
}