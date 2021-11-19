using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Users;
using SayWhat.MongoDAL.WordKits;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows {

public class AddFromLearningSetFlow {
    private ChatRoom Chat { get; }
    private readonly LocalDictionaryService _localDictionaryService;
    private readonly LearningSet _set;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;
    private string selectLearningSetData = "/lsselect";
    private string moveNextData => "/ls>>";
    private string movePrevData => "/ls<<";

    public AddFromLearningSetFlow(
        ChatRoom chat,
        LocalDictionaryService localDictionaryService,
        LearningSet set,
        UserService userService,
        UsersWordsService usersWordsService,
        AddWordService addWordService) {
        Chat = chat;
        _localDictionaryService = localDictionaryService;
        _set = set;
        _userService = userService;
        _usersWordsService = usersWordsService;
        _addWordService = addWordService;
    }

    public async Task EnterAsync() {
        var selector = new PaginationCollection<WordInLearningSet>(_set.Words);

        var currentSet = Chat.User.TrainingSets?.FirstOrDefault(f => f.SetId == _set.Id);

        if (currentSet == null)
            await SaveTrainingSetOffset(0);
        else
            selector.Page = currentSet.LastSeenWordOffset;

        var (word, translations) = await _localDictionaryService.GetTranslationWithExamplesByEnWord(selector.Current.Word);

        await Chat.SendMarkdownMessageAsync(
            GetWordText(selector, word, translations, selector.Current),
            GetWordKeyboard());

        var continueAddWords = true;
        while (continueAddWords)
        {
            var userInput = await Chat.WaitUserInputAsync();
            continueAddWords = await HandleWordButton(userInput, selector);
            var _ = SaveTrainingSetOffset(selector.Page);
        }
    }

    private async Task SaveTrainingSetOffset(int offset) {
        var currentSet = Chat.User.TrainingSets?.FirstOrDefault(f => f.SetId == _set.Id);
        if (currentSet == null)
        {
            Chat.User.TrainingSets ??= new List<UserTrainSet>();
            Chat.User.TrainingSets.Add(new UserTrainSet { SetId = _set.Id, LastSeenWordOffset = offset });
        }
        else
        {
            if (currentSet.LastSeenWordOffset == offset)
                return;
            currentSet.LastSeenWordOffset = offset;
        }

        await _userService.Update(Chat.User);
    }

    async Task<bool> HandleWordButton(Update update, PaginationCollection<WordInLearningSet> selector) {
        if (selector.Count == 0)
        {
            await Chat.ConfirmCallback(update.CallbackQuery.Id);
            return false;
        }

        var moveResult = false;
        if (update.CallbackQuery.Data == selectLearningSetData)
        {
            await AddWordToUser(selector.Current);
            //Todo почему то не работает
            await Chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id, Chat.Texts.WordIsAddedForLearning(selector.Current.Word));
            moveResult = await MoveOnNextWord(selector, true);
        }
        else if (update.CallbackQuery.Data == moveNextData)
        {
            await Chat.AnswerCallbackQueryWithTooltip(update.CallbackQuery.Id, Chat.Texts.WordIsSkippedForLearning(selector.Current.Word));
            moveResult = await MoveOnNextWord(selector, true);
        }
        else
            await Chat.ConfirmCallback(update.CallbackQuery.Id);

        DictionaryWord word = null;
        IReadOnlyList<Translation> translations = null;
        while (word == null)
        {
            if (!moveResult)
            {
                await SendAllWordsAreLearnedMessage(update.CallbackQuery.Message.MessageId);
                return false;
            }

            (word, translations) = await _localDictionaryService.GetTranslationWithExamplesByEnWord(selector.Current.Word);
            if (word == null)
                moveResult = await MoveOnNextWord(selector, true);
        }

        await Chat.EditMessageTextMarkdown(
            update.CallbackQuery.Message.MessageId,
            GetWordText(selector, word, translations, selector.Current),
            GetWordKeyboard());
        return true;
    }

    private async Task SendAllWordsAreLearnedMessage(int messageId) => 
        await Chat.EditMessageTextMarkdown(messageId, Chat.Texts.AllWordsAreLearnedMessage(_set.ShortName));

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

    private Task<bool> HasUserWord(WordInLearningSet current) => _usersWordsService.Contains(Chat.User, current.Word.ToLower());


    private async Task AddWordToUser(WordInLearningSet word) {
        var tranlsations = await _addWordService.GetOrDownloadTranslation(word.Word);
        var allowedTranlsations = tranlsations.Where(
            translation => word.AllowedTranslations.Any(
                allowed => allowed.Equals(translation.TranslatedText, StringComparison.InvariantCultureIgnoreCase)));
        foreach (var tranlsation in allowedTranlsations)
            await _addWordService.AddTranslationToUser(Chat.User, tranlsation);
    }

    private InlineKeyboardButton[] GetWordKeyboard() =>
        new[] {
            new InlineKeyboardButton {
                CallbackData = selectLearningSetData,
                Text = $"{Emojis.HeavyPlus} {Chat.Texts.SelectWordInLearningSet}"
            },
            new InlineKeyboardButton {
                CallbackData = moveNextData,
                Text = $"{Emojis.SoftNext}  {Chat.Texts.Skip}"
            },
            InlineButtons.MainMenu($"{Emojis.MainMenu} {Chat.Texts.MainMenuButton}"),
        };

    private string GetWordText(
        PaginationCollection<WordInLearningSet> collection,
        DictionaryWord dictionaryWord,
        IReadOnlyList<Translation> translations,
        WordInLearningSet wordInLearningSet) {
        var engWord = dictionaryWord.Word;
        var transcription = dictionaryWord.Transcription;
        var allowedTranslations = SearchForAllowedTranslations(translations, wordInLearningSet);
        var example = GetExampleOrNull(wordInLearningSet, allowedTranslations);

        var msg = new StringBuilder();
        msg.AppendLine($"*{engWord.EscapeForMarkdown().Capitalize()}*");
        if (!string.IsNullOrWhiteSpace(transcription))
            msg.Append($"```\r\n[{transcription.EscapeForMarkdown()}]\r\n```");
        msg.AppendLine(
            $"\r\n*{string.Join("\r\n", allowedTranslations.Select(a => a.TranslatedText.EscapeForMarkdown().Capitalize()))}*");
        if (example != null)
            msg.Append(
                $"```\r\n\r\n" +
                $"{Emojis.OpenQuote}{example.OriginPhrase.EscapeForMarkdown()}{Emojis.CloseQuote}\r\n" +
                $"{Emojis.OpenQuote}{example.TranslatedPhrase.EscapeForMarkdown()}{Emojis.CloseQuote}" +
                $"\r\n```");
        msg.AppendLine($"\r\n{Chat.Texts.XofYMarkdown(collection.Page + 1, collection.Count)}");
        return msg.ToString();
    }

    private static Example GetExampleOrNull(
        WordInLearningSet wordInLearningSet, Translation[] allowedTranslations) {
        var allowed = allowedTranslations.SelectMany(t => t.Examples)
                                         .Where(e => wordInLearningSet.AllowedExamples.Contains(e.Id));
        var bestFit = allowed.Where(
                                 a => a.TranslatedPhrase.Contains(
                                     a.TranslatedWord, StringComparison.InvariantCultureIgnoreCase))
                             .OrderBy(e => e.TranslatedPhrase.Length)
                             .FirstOrDefault();
        return bestFit ??
               allowed
                   .OrderBy(e => e.TranslatedPhrase.Length)
                   .FirstOrDefault();
    }

    private static Translation[] SearchForAllowedTranslations(
        IReadOnlyList<Translation> translations, WordInLearningSet wordInLearningSet) {
        var allowedTranslations = translations.Where(
                                                  t =>
                                                      wordInLearningSet.AllowedTranslations.Any(
                                                          a => a.Equals(
                                                              t.TranslatedText,
                                                              StringComparison.InvariantCultureIgnoreCase))
                                              )
                                              .Take(3)
                                              .ToArray();

        if (!allowedTranslations.Any())
            allowedTranslations = new[] { translations.First() };
        return allowedTranslations;
    }
}

}