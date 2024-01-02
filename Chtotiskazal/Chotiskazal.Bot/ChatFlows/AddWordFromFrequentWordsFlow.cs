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
    private FreqWordsSelector _selector;
    private CurrentFrequentWord _current;
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
        var typing = Chat.SendTyping();
        
        var count = await _frequentWordService.Count();

        _selector = new FreqWordsSelector(Chat.User.OrderedFrequentItems.ToList() ?? new List<UserFreqItem>(), count);
        await MoveToNextFrequentWord();
        await typing;
        await Chat.SendMarkdownMessageAsync(GetCurrentMessage(), GetCurrentKeyboard());
        while (true)
        {
            var update = await Chat.WaitUserInputAsync();
            var userInput = update.CallbackQuery;
            if (userInput == null)
            {
                await Chat.SendMessageAsync(Chat.Texts.PressTranslateToMoveStartTranslation);
                continue;
            }

            if (userInput.Data == SelectLearningSetData)
            {
                //Todo почему то не работает
                await Chat.AnswerCallbackQueryWithTooltip(
                    userInput.Id, Chat.Texts.WordIsAddedForLearning(_current.FrequentWord.Word));
                await SaveCurrent(FreqWordResult.ToLearn);
            }
            else if (userInput.Data == MoveNextData)
            {
                await Chat.AnswerCallbackQueryWithTooltip(
                    userInput.Id, Chat.Texts.WordIsSkippedForLearning(_current.FrequentWord.Word));
                await SaveCurrent(FreqWordResult.Skip);
            }
            else
                await Chat.ConfirmCallback(userInput.Id);

            await MoveToNextFrequentWord();
            await Chat.EditMessageTextMarkdown(userInput.Message.MessageId,
                GetCurrentMessage(), GetCurrentKeyboard());
            // await HandleWordButton(userInput);
            //todo - Это должно сразу переводиться. Значит нужно выносить обработчики страниц в обработчики ???
        }
    }

    private Markdown GetCurrentMessage()
    {
        var engWord =  _current.DictionaryWord.Word;
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
        var order = -1;
        //если слов меньше 10, то берем рандомные слова
        //далее - чем больше слов, тем увеличивается дисперсия
        if (_selector.Count <= 10)
            order = Rand.Rnd.Next(0, _selector.MaxSize - 1);
        else
        {
            while (order < 0 || order > _selector.MaxSize - 1)
            {
                var dispersion = _selector.Count switch
                {
                    < 15 => 60,
                    < 20 => 50,
                    < 40 => 15,
                    _ => 10
                };
                var section = _selector.CalcCentralSection();
                var middle = (section.Left + section.Right) / 2;
                order = (int)Rand.RandomNormal(middle, (middle * dispersion) / 100);
            }
        }
        //Console.WriteLine("Order: "+ order);    

        var foundNumber = _selector.GetFreeLeft(order) 
                         ?? _selector.GetFreeRight(order) 
                         ?? throw new Exception("There are no free numbers");
        
        var freqWord = await _frequentWordService.GetWord(foundNumber);
        if (string.IsNullOrEmpty(freqWord?.Word))
        {
            //Слово не найдено, или слово зарезервировано - начинаем заново
            return null;
        }

        var (dictionaryWord, translations) = await _localDictionaryService.GetTranslationWithExamplesByEnWord(freqWord.Word);
        if (dictionaryWord == null)
        {
            //Почему то не найден перевод для указанного слова. Это странно но окэй
            return null;
        }

        var userWord = await _usersWordsService.GetWordNullByEngWord(Chat.User, dictionaryWord.Word.ToLower());
        if (userWord != null)
        {
            // Указанное слово уже изучается пользователем
            if(userWord.AbsoluteScore> WordLeaningGlobalSettings.LearnedWordMinScore)
                _selector.Add(foundNumber, FreqWordResult.AlreadyLearned);
            else
                _selector.Add(foundNumber, FreqWordResult.AlreadyLearning);
            return null;
        }
        
        return new CurrentFrequentWord(freqWord, dictionaryWord, translations.ToArray());
    }
        

    
    private async Task SaveCurrent(FreqWordResult status)
    {
        _selector.Add(_current.FrequentWord.OrderNumber, status);
        if (status == FreqWordResult.ToLearn)
        {
            // add word to user learning
            foreach (var translation in _current.Translations) {
                await _addWordService.AddTranslationToUser(Chat.User, translation);
            }
        }
        // save user freq word progress
        Chat.User.AddFrequentWord(_current.FrequentWord.OrderNumber, status);
        await _userService.Update(Chat.User);
    }


    private InlineKeyboardButton[][] GetCurrentKeyboard() =>
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

public class UserCannotFindFrequentWordException : Exception{}

