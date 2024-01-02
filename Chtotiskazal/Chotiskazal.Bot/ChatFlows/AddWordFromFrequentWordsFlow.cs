using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.FrequentWords;
using SayWhat.MongoDAL.Users;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows;

public class AddWordFromFrequentWordsFlow
{
    public ChatRoom Chat { get; }
    private readonly FrequentWordService _frequentWordService;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;
    private readonly LocalDictionaryService _localDictionaryService;
    private const string SelectLearningSetData = "/lsselect";
    private const string MoveNextData = "/ls>>";
    private const string MovePrevData = "/ls<<";
    private readonly FreqWordsSelector _selector;
    private FrequentWord _currentFrequentWord;
    public AddWordFromFrequentWordsFlow(
        ChatRoom chat,
        FrequentWordService frequentWordService,
        UserService userService,
        UsersWordsService usersWordsService,
        AddWordService addWordService,
        LocalDictionaryService localDictionaryService)
    {
        Chat = chat;
        _frequentWordService = frequentWordService;
        _userService = userService;
        _usersWordsService = usersWordsService;
        _addWordService = addWordService;
        _localDictionaryService = localDictionaryService;
    }

    public async Task EnterAsync()
    {
        await SaveCurrent();
        var (word, translations) =
            await _localDictionaryService.GetTranslationWithExamplesByEnWord(_currentFrequentWord.Word);
        await Chat.SendMarkdownMessageAsync(GetWordMarkdown(word, translations, _currentFrequentWord), GetWordKeyboard());

        while (true)
        {
            var userInput = await Chat.WaitUserInputAsync();
            if (userInput.CallbackQuery != null)
            {
                await HandleWordButton(userInput);
            } //todo - Это должно сразу переводиться. Значит нужно выносить обработчики страниц в обработчики ???
            else
                await Chat.SendMessageAsync(Chat.Texts.PressTranslateToMoveStartTranslation);
        }
    }

    async Task HandleWordButton(Update update)
    {
        if (update.CallbackQuery.Data == SelectLearningSetData)
        {
            await AddWordToUser(_currentFrequentWord);
            //Todo почему то не работает
            await Chat.AnswerCallbackQueryWithTooltip(
                update.CallbackQuery.Id, Chat.Texts.WordIsAddedForLearning(_currentFrequentWord.Word));
            await SaveCurrent(FreqWordResult.Learning);
        }
        else if (update.CallbackQuery.Data == MoveNextData)
        {
            await Chat.AnswerCallbackQueryWithTooltip(
                update.CallbackQuery.Id, Chat.Texts.WordIsSkippedForLearning(_currentFrequentWord.Word));
            await SaveCurrent(FreqWordResult.Known);
        }
        else
            await Chat.ConfirmCallback(update.CallbackQuery.Id);

        DictionaryWord word = null;
        IReadOnlyList<Translation> translations = null;
        while (word == null)
        {
            (word, translations) =
                await _localDictionaryService.GetTranslationWithExamplesByEnWord(_currentFrequentWord.Word);
            if (word == null) //it is an error
            {
                await SaveCurrent();
                break;
            }
            
            var alreadyLearningWord = await _usersWordsService.GetWordNullByEngWord(Chat.User, _currentFrequentWord.Word);
            if (alreadyLearningWord != null)
            {
                if (alreadyLearningWord.AbsoluteScore > WordLeaningGlobalSettings.WellLearnedWordAbsScore)
                    await SaveCurrent(FreqWordResult.AlreadyLearned);
                else
                    await SaveCurrent(FreqWordResult.AlreadyLearning);
            }
        }

        await Chat.EditMessageTextMarkdown(
            update.CallbackQuery.Message.MessageId,
            GetWordMarkdown(word, translations, _currentFrequentWord),
            GetWordKeyboard());
    }

    private async Task SaveCurrent(FreqWordResult? learning = null)
    {
        throw new NotImplementedException();
    }


    private Task<bool> HasUserWord(FrequentWord current) =>
        _usersWordsService.Contains(Chat.User, current.Word.ToLower());


    private async Task AddWordToUser(FrequentWord word)
    {
        //todo
    }


    private InlineKeyboardButton[][] GetWordKeyboard() =>
        new[]
        {
            new[]
            {
                InlineButtons.Button($"{Emojis.HeavyPlus} {Chat.Texts.SelectWordInLearningSet}", SelectLearningSetData)
            },
            new[]
            {
                InlineButtons.Button($"{Emojis.SoftNext}  {Chat.Texts.Skip}", MoveNextData)
            },
            new[]
            {
                InlineButtons.MainMenu(Chat.Texts),
            }
        };

    private Markdown GetWordMarkdown(
        DictionaryWord dictionaryWord,
        IReadOnlyList<Translation> translations,
        FrequentWord wordInLearningSet)
    {
        var engWord =  dictionaryWord.Word;
        var transcription = dictionaryWord.Transcription;
        var allowedTranslations = SearchForAllowedTranslations(translations, wordInLearningSet);
        var example = GetExampleOrNull(wordInLearningSet, allowedTranslations);

        var msgWithMarkdownFormatted = new StringBuilder();

        msgWithMarkdownFormatted.AppendLine($"*{Markdown.Escaped(engWord.Capitalize()).GetMarkdownString()}*");
        if (!string.IsNullOrWhiteSpace(transcription))
            msgWithMarkdownFormatted.Append($"```\r\n[{Markdown.Escaped(transcription).GetMarkdownString()}]\r\n```");
        msgWithMarkdownFormatted.AppendLine(
            $"\r\n*{string.Join("\r\n", allowedTranslations.Select(a => Markdown.Escaped(a.TranslatedText.Capitalize()).GetMarkdownString()))}*");
        if (example != null)
            msgWithMarkdownFormatted.Append(
                $"```\r\n\r\n" +
                $"{Emojis.OpenQuote}{Markdown.Escaped(example.OriginPhrase).GetMarkdownString()}{Emojis.CloseQuote}\r\n" +
                $"{Emojis.OpenQuote}{Markdown.Escaped(example.TranslatedPhrase).GetMarkdownString()}{Emojis.CloseQuote}" +
                $"\r\n```");

        return Markdown.Bypassed(msgWithMarkdownFormatted.ToString());
    }

    private static Example GetExampleOrNull(
        FrequentWord wordInLearningSet, Translation[] allowedTranslations)
    {
        var allowed = allowedTranslations.SelectMany(t => t.Examples)
            .Where(e => wordInLearningSet.AllowedExamples.Contains(e.Id));
        var bestFit = allowed.Where(
                a => a.TranslatedPhrase.Contains(
                    a.TranslatedWord, StringComparison.InvariantCultureIgnoreCase))
            .MinBy(e => e.TranslatedPhrase.Length);
        return bestFit ?? allowed.MinBy(e => e.TranslatedPhrase.Length);
    }

    private static Translation[] SearchForAllowedTranslations(
        IReadOnlyList<Translation> translations, FrequentWord wordInLearningSet)
    {
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