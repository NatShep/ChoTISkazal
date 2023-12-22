using System;
using System.Linq;
using System.Threading.Tasks;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class RemoveWordCommandHandler : IBotCommandHandler {
    private readonly UsersWordsService _usersWordsService;

    public RemoveWordCommandHandler(UsersWordsService usersWordsService) {
        _usersWordsService = usersWordsService;
    }

    public bool Acceptable(string text) => text.StartsWith(BotCommands.RemoveWord);

    public string ParseArgument(string text) => text.Substring(BotCommands.RemoveWord.Length).Trim();

    public async Task Execute(string argument, ChatRoom chat) {
        if (argument.Length == 0) {
            await chat.SendMessageAsync(chat.Texts.EnterWordToRemove);
            var text = await chat.WaitUserTextInputAsync();
            await RemoveWord(text.Trim(), chat);
        }
        else {
            await RemoveWord(argument.Trim(), chat);
        }
    }

    private async Task RemoveWord(string wordOrTranslation, ChatRoom chat) {
        var allWords = await _usersWordsService.GetAllWords(chat.User);
        var originWords = allWords.Where(w =>
            w.Word.Equals(wordOrTranslation, StringComparison.InvariantCultureIgnoreCase)
            || w.HasTranslation(wordOrTranslation)
        ).ToArray();
        if (!originWords.Any()) {
            await chat.SendMarkdownMessageAsync(chat.Texts.WordNotFound(wordOrTranslation),
                InlineButtons.MainMenu(chat.Texts));
        }
        else {
            foreach (var wordModel in originWords) {
                await _usersWordsService.RemoveWord(wordModel);
            }

            await chat.SendMarkdownMessageAsync(chat.Texts.WordRemoved(wordOrTranslation),
                InlineButtons.MainMenu(chat.Texts));
        }
    }
}