using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using Chotiskazal.Bot.ChatFlows.FlowLearning;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL.Users;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.CommandHandlers;

public class AddFrequentWordsCommandHandler : IBotCommandHandler
{
    private readonly FrequentWordService _frequentWordService;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;
    private readonly LocalDictionaryService _localDictionary;
    private readonly ExamSettings _examSettings;
    private readonly QuestionSelector _questionSelector;

    public AddFrequentWordsCommandHandler(
        FrequentWordService frequentWordService,
        UserService userService,
        UsersWordsService usersWordsService,
        AddWordService addWordService,
        LocalDictionaryService localDictionary,
        QuestionSelector questionSelector,
        ExamSettings examSettings)
    {
        _frequentWordService = frequentWordService;
        _userService = userService;
        _usersWordsService = usersWordsService;
        _addWordService = addWordService;
        _localDictionary = localDictionary;
        _questionSelector = questionSelector;
        _examSettings = examSettings;
    }

    public bool Acceptable(string text) => text == BotCommands.AddNewWords;

    public string ParseArgument(string text) => null;

    public async Task Execute(string argument, ChatRoom chat)
    {
        //Проводим добавление слов
        var frequentWordFlow = new AddWordFromFrequentWordsFlow(
            chat, _frequentWordService, _userService,
            _usersWordsService, _addWordService, _localDictionary);
        var additionResult =
            await frequentWordFlow.EnterAsync(maxWordSelection: 4, minWordsSelection: 2, preferedQuestionSize: 12);

        //Проводим легкий экзамен
        await CollectWordsForExam(chat.User, additionResult);
        var learnFlow = new ExamFlow(chat, _userService, _usersWordsService, _examSettings, _questionSelector);
        var newWordsMessage =
            chat.Texts.GoToLearnAfterAddition.NewLine().NewLine() +
            ExamHelper.GetCarefullyLearnTheseWordsMessage(chat.Texts, ExamType.NoInput, additionResult.AddedWords);
        await chat.EditMessageTextMarkdown(
            additionResult.messageId, newWordsMessage, ExamHelper.GetStartExaminationButtons(chat.Texts));

        var needToContinue = await chat.WaitInlineKeyboardInput();
        if (needToContinue != InlineButtons.StartExaminationButtonData)
            return;

        var results = await learnFlow.DoExam(
            ExamType.NoInput, additionResult.AddedWords.DistinctBy(d => d.Word).ToArray(), 12);

        //Печатаем результаты экзамена
        await ExamHelper.SendMotivationMessages(chat, _examSettings, results);
        var message = Markdown.Escaped($"{chat.Texts.LearningDone}").ToSemiBold()
                          .NewLine()
                      + chat.Texts.YourNewWords
                          .NewLine()
                          .NewLine();

        foreach (var word in additionResult.AddedWords)
        {
            message = message
                .AddEscaped($"{Emojis.HeavyPlus} ")
                .AddEscaped(word.Word)
                .NewLine();
        }

        message += ExamHelper.GetGoalStreakMessage(chat, _examSettings);
        await chat.SendMarkdownMessageAsync(message, ExamHelper.GetButtonsForExamResultMessage(chat.Texts));
    }

    private async Task CollectWordsForExam(UserModel user, AddFreqWordResults additionResult)
    {
        var newWords = additionResult.AddedWords.ToList();

        if (newWords.Count < 5)
        {
            var wellDone =
                await _usersWordsService.GetWellDoneWords(user, 2, _examSettings.MaxTranslationsInOneExam);
            newWords.AddRange(wellDone);
        }

        if (newWords.Count < 5)
        {
            var learning =
                await _usersWordsService.GetLearningWords(user, 2, _examSettings.MaxTranslationsInOneExam);
            newWords.AddRange(learning);
        }

        if (newWords.Count < 5)
        {
            var beginnerWords =
                await _usersWordsService.GetBeginnerWords(user, 2, _examSettings.MaxTranslationsInOneExam);
            newWords.AddRange(beginnerWords);
        }
    }
}