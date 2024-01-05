using System;
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
        if(Chat.User.ExamsInARow<0) 
            //пользователь не проходил изучение с момента обновления бота. Покажем ему новую возможность
            return LearnType.Addition;
        if(wordsCount<5) 
            // Если слов не достаточно - то добавление
            return LearnType.Addition;
        if(Chat.User.LearningDone<=2) //первые два раза - добавление
            return LearnType.Addition;
        if (Chat.User.LearningDone == 3) //на третьем - экзамен
            return LearnType.Exam;
        
        if (wordsCount < 10)
            return LearnType.Addition;
        if(wordsCount - Chat.User.WordsLearned<10)
            return LearnType.Addition;
       
        if (wordsCount < 30)
            return Chat.User.ExamsInARow >= 2 ? LearnType.Addition : LearnType.Exam;
        if (wordsCount < 40)
            return Chat.User.ExamsInARow >= 3 ? LearnType.Addition : LearnType.Exam;
        else
            return Chat.User.ExamsInARow >= 20 ? LearnType.Addition : LearnType.Exam;

    }
    
    enum LearnType
    {
        Addition,
        Exam
    }
}

