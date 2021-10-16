using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.WordKits;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using DictionaryTranslation = SayWhat.Bll.Dto.DictionaryTranslation;

namespace Chotiskazal.Bot.ChatFlows {

public class SelectLearningSetsFlow {
    private readonly LocalDictionaryService _localDictionaryService;
    private readonly LearningSetService _learningSetService;
    private readonly UsersWordsService _usersWordsService;
    private string moveNextData => "/ls>>";
    private string movePrevData => "/ls<<";
    private string selectLearningSetData = "/lsselect";

    private readonly PaginationCollection<LearningSet> _learningSetSelector = new PaginationCollection<LearningSet>();
    private readonly AddWordService _addWordService;

    public SelectLearningSetsFlow(
        ChatRoom chat,
        LocalDictionaryService localDictionaryService,
        LearningSetService learningSetService,
        UsersWordsService usersWordsService,
        AddWordService addWordService) {
        _localDictionaryService = localDictionaryService;
        _learningSetService = learningSetService;
        _usersWordsService = usersWordsService;
        _addWordService = addWordService;
        Chat = chat;
    }

    private ChatRoom Chat { get; }

    public async Task EnterAsync() {
        _learningSetSelector.Set(await _learningSetService.GetAllSets());
        await Chat.SendMarkdownMessageAsync(
            GetCurrentLearningSetText(),
            GetSelectLearningSetKeyboard());

        while (true)
        {
            var userInput = await Chat.WaitUserInputAsync();
            var isSelected = await HandleLearningSetButton(userInput);
            await Chat.ConfirmCallback(userInput.CallbackQuery.Id);
            if (isSelected)
                break;
        }

        var selected = _learningSetSelector.Current;
        var wordSelector = new PaginationCollection<WordInLearningSet>(selected.Words);

        var continueAddWords = true;
        while (continueAddWords)
        {
            var userInput = await Chat.WaitUserInputAsync();
            continueAddWords = await HandleWordButton(userInput, wordSelector);
            await Chat.ConfirmCallback(userInput.CallbackQuery.Id);
        }
        // ReSharper disable once FunctionNeverReturns
    }

    async Task<bool> HandleLearningSetButton(Update update) {
        if (_learningSetSelector.Count == 0)
            return false;


        if (update.CallbackQuery.Data == selectLearningSetData)
        {
            //remove keyboard
            await Chat.EditMessageTextMarkdown(
                update.CallbackQuery.Message.MessageId,
                GetCurrentLearningSetText());
            return true;
        }
        else if (update.CallbackQuery.Data == movePrevData)
            _learningSetSelector.MoveNext();
        else
            _learningSetSelector.MovePrev();

        await Chat.EditMessageTextMarkdown(
            update.CallbackQuery.Message.MessageId,
            GetCurrentLearningSetText(),
            GetSelectLearningSetKeyboard());

        return false;
    }

    async Task<bool> HandleWordButton(Update update, PaginationCollection<WordInLearningSet> selector) {
        if (selector.Count == 0)
            return false;
        var moveResult = false;
        if (update.CallbackQuery.Data == selectLearningSetData)
        {
            await AddWordToUser(selector.Current);
            moveResult = await MoveOnNextWord(selector, true);
        }
        else if (update.CallbackQuery.Data == movePrevData)
            moveResult = await MoveOnNextWord(selector, true);
        else
            moveResult = await MoveOnNextWord(selector, true);

        if (!moveResult)
        {
            await SendAllWordsAreLearnedMessage();
            return false;
        }

        var (word, translations) = await _localDictionaryService.GetWordInfo(selector.Current.Word);

        await Chat.EditMessageTextMarkdown(
            update.CallbackQuery.Message.MessageId,
            GetWordText(word, translations, selector.Current),
            GetWordKeyboard());
        return true;
    }

    private async Task SendAllWordsAreLearnedMessage() { throw new NotImplementedException(); }

    private async Task<bool> MoveOnNextWord(PaginationCollection<WordInLearningSet> selector, bool moveNext) {
        for (int i = 0; i < selector.Count; i++)
        {
            if (moveNext)
                selector.MoveNext();
            else
                selector.MovePrev();

            if (!await HasUserWord(selector.Current))
                return true;
        }

        return false;
    }

    private Task<bool> HasUserWord(WordInLearningSet current) => _usersWordsService.Contains(Chat.User, current.Word);


    private async Task AddWordToUser(WordInLearningSet word) {
        var tranlsations = await _addWordService.GetOrDownloadTranslation(word.Word);
        var allowedTranlsations = tranlsations.Where(
            translation => word.AllowedTranslations.Any(
                allowed => allowed.Equals(translation.OriginText, StringComparison.InvariantCultureIgnoreCase)));
        foreach (var tranlsation in allowedTranlsations)
            await _addWordService.AddTranslationToUser(Chat.User, tranlsation);
    }

    private InlineKeyboardButton[][] GetSelectLearningSetKeyboard() => GetKeyboard(Chat.Texts.SelectLearningSet);
    private InlineKeyboardButton[][] GetWordKeyboard() => GetKeyboard(Chat.Texts.SelectWordInLearningSet);

    private InlineKeyboardButton[][] GetKeyboard(string selectText) =>
        new[] {
            new[] {
                new InlineKeyboardButton { CallbackData = movePrevData, Text = Emojis.Next },
                new InlineKeyboardButton { CallbackData = moveNextData, Text = Emojis.Prev },
            },
            new[] {
                new InlineKeyboardButton
                    { CallbackData = selectLearningSetData, Text = selectText },
            },
            new[] {
                InlineButtons.MainMenu($"{Emojis.MainMenu} {Chat.Texts.MainMenuButton}"),
            }
        };

    private string GetWordText(
        DictionaryWord word,
        IReadOnlyList<DictionaryTranslation> translations,
        WordInLearningSet wordInLearningSet
    ) {
        var msg = new StringBuilder();
        //todo Message
        msg.Append($"*{word.Word}:* \r\n");
        msg.Append(Chat.Texts.ShowNumberOfLists(_learningSetSelector.Page + 1, _learningSetSelector.Count));
        return msg.ToString();
    }

    private string GetCurrentLearningSetText() {
        var msg = new StringBuilder();

        var learningSet = _learningSetSelector.Current;
        msg.Append($"{Emojis.ShowWellLearnedWords} *{learningSet.EnName}:* {learningSet.EnDescription}\r\n");
        msg.Append(Chat.Texts.ShowNumberOfLists(_learningSetSelector.Page + 1, _learningSetSelector.Count));
        return msg.ToString();
    }
}

}