using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.ChatFlows.FlowLearning;

public class LearningFlow
{
    private ChatRoom Chat { get; }

    private readonly ExamSettings _regularExamSettings;
    private readonly QuestionSelector _questionSelector;
    private readonly FrequentWordService _frequentWordService;
    private readonly UserService _userService;
    private readonly UsersWordsService _usersWordsService;
    private readonly AddWordService _addWordService;
    private readonly LocalDictionaryService _localDictionaryService;
    
    public LearningFlow(
        ChatRoom chat,
        UserService userService,
        UsersWordsService usersWordsService,
        ExamSettings regularExamSettings,
        QuestionSelector questionSelector, FrequentWordService frequentWordService, AddWordService addWordService, LocalDictionaryService localDictionaryService)
    {
        Chat = chat;
        _userService = userService;
        _usersWordsService = usersWordsService;
        _regularExamSettings = regularExamSettings;
        _questionSelector = questionSelector;
        _frequentWordService = frequentWordService;
        _addWordService = addWordService;
        _localDictionaryService = localDictionaryService;
    }

    public async Task EnterAsync()
    {
        var type = GetLearnType();

        if (type == LearnType.Addition)
        {
            Chat.User.ExamsInARow = 0;
            var flow = new NewWordsFlow(Chat, 
                _frequentWordService, _userService, _usersWordsService,
                _addWordService, 
                _localDictionaryService, 
                _questionSelector, 
                _regularExamSettings);
            await flow.EnterAsync();
        }
        else
        {
            Chat.User.ExamsInARow++;
            var flow = new ExaminationFlow(Chat, _userService, _usersWordsService, _regularExamSettings, _questionSelector);
            await flow.EnterAsync();
        }
    }

    private LearnType GetLearnType()
    {
        // Для новых пользователей:
        //Если пользователь никогда не проходил добавление - то добавление
        //Если слов <5 то добавление
        //Если новых слов у пользователя >8 - то изучение
        //Первые два раза - добавление
        //Третий раз - экзамен
        
        // Для продолжающих
        //Если невыученных слов у пользователя <10 - то добавление
        //Если слов <10 то добавление
        //Если слов <30 то: 2 изучения 1 добавление
        //Если слов <40 то: 3 изучения 1 добавление
        
        //Для профиков
        //Если cлов >40 то: 20 изучений 1 добавление
        
        var wordsCount = Chat.User.WordsCount;
        //Для самых новичков
        if(Chat.User.ExamsInARow<0) 
            //пользователь не проходил изучение с момента обновления бота. Покажем ему новую возможность
            return LearnType.Addition;
        if(wordsCount<5) 
            // Если слов не достаточно - то добавление
            return LearnType.Addition;
        
        
        if (Chat.User.WordsNewby > 8) //Пользователь мог сам надобавлять слова - можно и нужно экзаминовать в таком случае
            return LearnType.Exam;
        
        //Продолжение для новичков
        if(Chat.User.LearningDone<=2) //первые два раза - добавление
            return LearnType.Addition;
        if (Chat.User.LearningDone == 3) //на третьем - экзамен
            return LearnType.Exam;
        if (wordsCount < 10)
            return LearnType.Addition;

        var notLearn = wordsCount - Chat.User.WordsLearned;
        // Для продолжающих
        if(notLearn<10)
            return LearnType.Addition;
        // если всего слов меньше 30 - то добавляем раз в три раза
        if (wordsCount < 30)
            return Chat.User.ExamsInARow >= 2 ? LearnType.Addition : LearnType.Exam;
        // если всего слов меньше 40 - то добавляем раз в четыре раза
        if (wordsCount < 40)
            return Chat.User.ExamsInARow >= 3 ? LearnType.Addition : LearnType.Exam;
        
        //для старожилов
        var examInARow = notLearn switch
        {
            < 20  => 10, // если не выучено менее 20 слов, то добавление случается раз в 10 экзаменов (раз в день)
            < 40  => 20,
            < 60  => 30,
            < 100 => 40,
            _     => 50,  // если не выучено более 100 слов, то добавление случается раз в 50 экзаменов (раз в 5 дней)
        };
        return Chat.User.ExamsInARow >= examInARow 
            ? LearnType.Addition 
            : LearnType.Exam;
    }
    
    enum LearnType
    {
        Addition,
        Exam
    }
}

