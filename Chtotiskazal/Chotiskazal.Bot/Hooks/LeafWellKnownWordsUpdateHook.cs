using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using Chotiskazal.Bot.Interface;
using SayWhat.Bll;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types;

namespace Chotiskazal.Bot.Hooks {


public class LeafWellKnownWordsUpdateHook : IChatUpdateHook {
    private PaginationCollection<List<UserWordModel>> _pages = new PaginationCollection<List<UserWordModel>>();
    private ChatRoom Chat { get; }

    public LeafWellKnownWordsUpdateHook(ChatRoom chat) { Chat = chat; }

    public void SetWellKnownWords(List<List<UserWordModel>> wellKnownWords) => _pages.Set(wellKnownWords);
    public void SetNumberOfPaginate(int i) => _pages.Page = i;

    public bool CanBeHandled(Update update) {
        var text = update.CallbackQuery?.Data;
        return text == WellKnownWordsHelper.NextData || text == WellKnownWordsHelper.PrevData;
    }

    public async Task Handle(Update update) {
        if (_pages.Count == 0)
        {
            await Chat.ConfirmCallback(update.CallbackQuery.Id);
            return;
        }

        if (update.CallbackQuery.Data == WellKnownWordsHelper.PrevData)
            _pages.MovePrev();
        else
            _pages.MoveNext();

        var msg = Markdown.Empty;

        foreach (var word in _pages.Current) {
            msg = msg.AddEscaped(Emojis.SoftMark) +
                Markdown.Escaped($"{word.Word}:").ToSemiBold()
                    .AddEscaped(word.AllTranslationsAsSingleString)
                    .NewLine();
        }

        msg += (Chat.Texts.PageXofY(_pages.Page + 1, _pages.Count));

        await Chat.EditMessageTextMarkdown(
            update.CallbackQuery.Message.MessageId,
            msg,
            new[] {
                WellKnownWordsHelper.GetPagingKeys(),
                new[] {
                    InlineButtons.MainMenu($"{Emojis.MainMenu} {Chat.Texts.MainMenuButton}"),
                }
            });

        await Chat.ConfirmCallback(update.CallbackQuery.Id);
    }
}

}