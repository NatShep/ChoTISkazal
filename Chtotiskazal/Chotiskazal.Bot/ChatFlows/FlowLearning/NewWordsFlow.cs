using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL.Users;

namespace Chotiskazal.Bot.ChatFlows.FlowLearning;

public class NewWordsFlow
{
    private ChatRoom Chat { get; }
    private readonly FrequentWordService _frequentWordService;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;
    private readonly LocalDictionaryService _localDictionary;
    private readonly ExamSettings _examSettings;
    private readonly int _maxWordSelection;
    private readonly int _minWordsSelection;
    private readonly int _preferedQuestionSize;
    private readonly QuestionSelector _questionSelector;

    public NewWordsFlow(
        ChatRoom chat,
        FrequentWordService frequentWordService,
        UserService userService,
        UsersWordsService usersWordsService,
        AddWordService addWordService,
        LocalDictionaryService localDictionary,
        QuestionSelector questionSelector,
        ExamSettings examSettings,
        int maxWordSelection = 4,
        int minWordsSelection = 2,
        int preferedQuestionSize = 12
    )
    {
        Chat = chat;
        _frequentWordService = frequentWordService;
        _userService = userService;
        _usersWordsService = usersWordsService;
        _addWordService = addWordService;
        _localDictionary = localDictionary;
        _questionSelector = questionSelector;
        _examSettings = examSettings;
        _maxWordSelection = maxWordSelection;
        _minWordsSelection = minWordsSelection;
        _preferedQuestionSize = preferedQuestionSize;
    }

    public async Task EnterAsync()
    {
        //Проводим добавление слов
        var frequentWordFlow = new AddWordFromFrequentWordsFlow(
            Chat, _frequentWordService, _userService,
            _usersWordsService, _addWordService, _localDictionary);
        var additionResult =
            await frequentWordFlow.EnterAsync(_maxWordSelection, _minWordsSelection, _preferedQuestionSize);

        //Проводим легкий экзамен
        await CollectWordsForExam(Chat.User, additionResult);
        var learnFlow = new ExaminationFlow(Chat, _userService, _usersWordsService, _examSettings, _questionSelector);
        var newWordsMessage =
            Chat.Texts.GoToLearnAfterAddition.NewLine().NewLine() +
            ExamHelper.GetCarefullyLearnTheseWordsMessage(Chat.Texts, ExamType.NoInput, additionResult.AddedWords);
        await Chat.EditMessageTextMarkdown(
            additionResult.messageId, newWordsMessage, ExamHelper.GetStartExaminationButtons(Chat.Texts));

        var needToContinue = await Chat.WaitInlineKeyboardInput();
        if (needToContinue != InlineButtons.StartExaminationButtonData)
            return;

        var results = await learnFlow.DoExam(
            ExamType.NoInput, additionResult.AddedWords.DistinctBy(d => d.Word).ToArray(), 12);

        //Печатаем результаты экзамена
        await ExamHelper.SendMotivationMessages(Chat, _examSettings, results);
        var message = Markdown.Escaped($"{Chat.Texts.LearningDone}").ToSemiBold()
                          .NewLine()
                      + Chat.Texts.YourNewWords
                          .NewLine()
                          .NewLine();

        foreach (var word in additionResult.AddedWords)
        {
            message = message
                .AddEscaped($"{Emojis.HeavyPlus} ")
                .AddEscaped(word.Word)
                .NewLine();
        }

        message += ExamHelper.GetGoalStreakMessage(Chat, _examSettings);
        await Chat.SendMarkdownMessageAsync(message, ExamHelper.GetButtonsForExamResultMessage(Chat.Texts));
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