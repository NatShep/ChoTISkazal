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
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows.FlowLearning;

public class AddWordFromFrequentWordsFlow
{
    public ChatRoom Chat { get; }
    private readonly FrequentWordService _frequentWordService;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;
    private readonly LocalDictionaryService _localDictionaryService;
    private const string SelectToLearnData = "/lsselect>>";
    private const string SelectToSkipData = "/lsskip>>";
    private const string SelectKnownData = "/lsknown>>";
    private FreqWordsSelector _selector;
    private CurrentFrequentWord _current;
    private readonly List<UserWordModel> _addedWordModels = new();

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

    public async Task<AddFreqWordResults> EnterAsync(
        int minWordsSelection,
        int maxWordSelection,
        int preferedQuestionSize)
    {
        var typing = Chat.SendTyping();

        var count = await _frequentWordService.Count();

        _selector = new FreqWordsSelector(Chat.User.OrderedFrequentItems.ToList(), count);
        await MoveToNextFrequentWord();
        await typing;
        var messageId = await Chat.SendMarkdownMessageAsync(GetCurrentMessage(), GetCurrentKeyboard());
        var wordsSelected = 0;
        var wordsShowed = 0;
        var wordsSkipped = 0;
        while (true)
        {
            var update = await Chat.WaitUserInputAsync();
            var userInput = update.CallbackQuery;
            if (userInput == null)
            {
                await Chat.SendMessageAsync(Chat.Texts.PressTranslateToMoveStartTranslation);
                continue;
            }

            if (userInput.Data == SelectToLearnData)
            {
                //Todo почему то не работает
                await Chat.AnswerCallbackQueryWithTooltip(
                    userInput.Id, Chat.Texts.WordIsAddedForLearning(_current.FrequentWord.Word));
                await SaveCurrent(FreqWordResult.UserSelectToLearn);
                wordsSelected++;
            }
            else if (userInput.Data == SelectToSkipData)
            {
                await Chat.AnswerCallbackQueryWithTooltip(
                    userInput.Id, Chat.Texts.WordIsSkippedForLearning(_current.FrequentWord.Word));
                await SaveCurrent(FreqWordResult.UserSelectToSkip);
                wordsSkipped++;
            }
            else if (userInput.Data == SelectKnownData)
            {
                await Chat.AnswerCallbackQueryWithTooltip(
                    userInput.Id, Chat.Texts.WordIsSkippedForLearning(_current.FrequentWord.Word));
                await SaveCurrent(FreqWordResult.UserSelectThatItIsKnown);
            }
            else
                await Chat.ConfirmCallback(userInput.Id);

            wordsShowed++;
            if (wordsSelected >= maxWordSelection ||
                (wordsSelected >= minWordsSelection && wordsShowed >= preferedQuestionSize))
                break;

            await MoveToNextFrequentWord();
            messageId = userInput.Message.MessageId;
            await Chat.EditMessageTextMarkdown(messageId,
                GetCurrentMessage(), GetCurrentKeyboard());
        }

        var newWords = _addedWordModels.DistinctBy(i => i.Word).ToArray();
        return new AddFreqWordResults(
            AddedWords: newWords,
            WordShowedCount: wordsShowed,
            WordSkippedCount: wordsSkipped,
            WordsKnownCount: wordsShowed - newWords.Length - wordsSkipped,
            messageId: messageId);
    }

    private Markdown GetCurrentMessage()
    {
        var engWord = _current.DictionaryWord.Word;
        var transcription = _current.DictionaryWord.Transcription;
        var allowedTranslations = SearchForAllowedTranslations(_current.Translations, _current.FrequentWord);
        var example = GetExampleOrNull(_current.FrequentWord, allowedTranslations);

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

    private async Task MoveToNextFrequentWord()
    {
        for (int i = 0; i < 20; i++)
        {
            var word = await SearchNextFrequentWord();
            if (word != null)
            {
                _current = word;
                return;
            }
        }

        // за 20 попыток мы не смогли найти ни одного подходящего слова. Либо пользователь выучил вообще все, любо у нас ошибка
        throw new UserCannotFindFrequentWordException();
    }

    private async Task<CurrentFrequentWord> SearchNextFrequentWord()
    {
        var foundNumber = GetNextNumber();

        var freqWord = await _frequentWordService.GetWord(foundNumber);
        if (string.IsNullOrEmpty(freqWord?.Word))
        {
            //Слово не найдено, или слово зарезервировано - начинаем заново
            return null;
        }

        var (dictionaryWord, translations) =
            await _localDictionaryService.GetTranslationWithExamplesByEnWord(freqWord.Word);
        if (dictionaryWord == null)
        {
            //Почему то не найден перевод для указанного слова. Это сомнительно но окэй
            return null;
        }

        var userWord = await _usersWordsService.GetWordNullByEngWord(Chat.User, dictionaryWord.Word.ToLower());
        if (userWord != null)
        {
            // Указанное слово уже изучается пользователем
            if (userWord.AbsoluteScore > WordLeaningGlobalSettings.LearnedWordMinScore)
                _selector.Add(foundNumber, FreqWordResult.AlreadyLearned);
            else
                _selector.Add(foundNumber, FreqWordResult.AlreadyLearning);
            return null;
        }

        return new CurrentFrequentWord(freqWord, dictionaryWord, translations.ToArray());
    }

    private int GetNextNumber()
    {
        int order = -1;
        int foundNumber;
        if (_selector.Count <= 10)
        {
            //если слов меньше 10, то берем рандомные слова
            //далее - чем больше слов известно, тем уменшается дисперсия
            order = Rand.Rnd.Next(0, _selector.MaxSize - 1);
        }
        else
        {
            var section = _selector.CalcCentralSection();
            var middle = (section.Left + section.Right) / 2;
            var distribution = _selector.GetDistribution(middle);
            var redCount = distribution.GtRed + distribution.LtRed;
            var greenCount = distribution.LtGreen + distribution.LtGreen;
            if (greenCount > redCount * 1.5)
            {
                //there is much more green than red
                // so we need to take word no less then right order
                return _selector.GetFreeRight(section.Right)
                       ?? _selector.GetFreeLeft(section.Right)
                       ?? middle;
            }

            if (redCount > greenCount * 1.5)
            {
                //there is much more red on the left than red one on the right
                return _selector.GetFreeLeft(section.Left)
                       ?? _selector.GetFreeRight(section.Left)
                       ?? middle;
            }

            while (order < 0 || order > _selector.MaxSize - 1)
            {
                //relatively same amount of red and green for user
                var dispersion = _selector.Count switch
                {
                    < 15 => 60,
                    < 20 => 50,
                    < 40 => 15,
                    _ => 10
                };
                order = (int)Rand.RandomNormal(middle, (middle * dispersion) / 100);
            }
        }

        return _selector.GetFreeLeft(order)
               ?? _selector.GetFreeRight(order)
               ?? throw new Exception("There are no free numbers");
    }

    private async Task SaveCurrent(FreqWordResult status)
    {
        _selector.Add(_current.FrequentWord.OrderNumber, status);
        if (status == FreqWordResult.UserSelectToLearn)
        {
            // add word to user learning
            foreach (var translation in _current.Translations)
            {
                var word = await _addWordService.AddTranslationToUser(Chat.User, translation);
                if (word != null)
                    _addedWordModels.Add(word);
            }
        }

        // If user skip the word than we dont save the word, to user.
        // It may appears after bot is restarted 
        if (status == FreqWordResult.UserSelectToSkip)
            return;
        // save user freq word progress
        Chat.User.AddFrequentWord(_current.FrequentWord.OrderNumber, status);
        await _userService.Update(Chat.User);
    }


    private InlineKeyboardButton[][] GetCurrentKeyboard() =>
        new[]
        {
            new[]
            {
                InlineButtons.Button(Chat.Texts.SelectWordIsKnownInLearningSet, SelectKnownData),
                InlineButtons.Button($"{Emojis.HeavyPlus}  {Chat.Texts.SelectToLearnWordInLearningSet}",
                    SelectToLearnData)
            },
            new[]
            {
                InlineButtons.Button($"{Emojis.SoftNext}  {Chat.Texts.Skip}", SelectToSkipData)
            },
            new[]
            {
                InlineButtons.MainMenu(Chat.Texts),
            }
        };

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

    record CurrentFrequentWord(FrequentWord FrequentWord, DictionaryWord DictionaryWord, Translation[] Translations);
}

public record AddFreqWordResults(
    UserWordModel[] AddedWords,
    int WordShowedCount,
    int WordSkippedCount,
    int WordsKnownCount,
    int messageId);

public class UserCannotFindFrequentWordException : Exception
{
}