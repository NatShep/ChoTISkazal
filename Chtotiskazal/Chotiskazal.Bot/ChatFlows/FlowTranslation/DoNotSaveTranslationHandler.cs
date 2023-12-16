using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows.FlowTranslation;

public class DoNotSaveTranslationHandler : LastTranslationHandlerBase {
    public DoNotSaveTranslationHandler(
        ChatRoom chat,
        AddWordService addWordService,
        ButtonCallbackDataService buttonCallbackDataService,
        Markdown message) : base("", chat, addWordService, buttonCallbackDataService, message) {
    }

    protected override async Task<IList<InlineKeyboardButton[]>> CreateCustomButtons() => new[] { Array.Empty<InlineKeyboardButton>() };
    public override Task HandleButtonClick(Update update, TranslationButtonData buttonData) => Task.CompletedTask;
}