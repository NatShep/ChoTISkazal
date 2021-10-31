using System;
using System.Threading.Tasks;
using Chotiskazal.Bot.Hooks;
using SayWhat.Bll;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.ChatFlows {

public class MainFlow {
    public MainFlow(
        ChatIO chatIo,
        TelegramUserInfo userInfo,
        BotSettings settings,
        AddWordService addWordsService,
        UsersWordsService usersWordsService,
        UserService userService, 
        LocalDictionaryService localDictionaryService, 
        LearningSetService learningSetService) {
        ChatIo = chatIo;
        _userInfo = userInfo;
        _settings = settings;
        _addWordsService = addWordsService;
        _usersWordsService = usersWordsService;
        _userService = userService;
        _localDictionaryService = localDictionaryService;
        _learningSetService = learningSetService;
    }

    private readonly AddWordService _addWordsService;
    private readonly UsersWordsService _usersWordsService;
    private readonly UserService _userService;
    private readonly LocalDictionaryService _localDictionaryService;
    private readonly LearningSetService _learningSetService;
    private readonly TelegramUserInfo _userInfo;

    private readonly BotSettings _settings;

    private TranslationSelectedUpdateHook _translationSelectedUpdateHook;
    private LeafWellKnownWordsUpdateHook _wellKnownWordsUpdateHook;
    public ChatIO ChatIo { get; }
    private async Task SayHelloAsync() => await ChatIo.SendMessageAsync(_settings.WelcomeMessage);

    public async Task Run() {
        try
        {
            string mainMenuCommandOrNull = null;

            var user = await _userService.GetUserOrNull(_userInfo);
            if (user == null)
            {
                var addUserTask = _userService.AddUserFromTelegram(_userInfo);
                await SayHelloAsync();
                user = await addUserTask;
                Botlog.WriteInfo($"New user {user.TelegramNick}", user.TelegramId.ToString(), true);
            }

            Chat = new ChatRoom(ChatIo, user);

            _translationSelectedUpdateHook = new TranslationSelectedUpdateHook(_addWordsService, Chat);
            _wellKnownWordsUpdateHook = new LeafWellKnownWordsUpdateHook(Chat);
            ChatIo.AddUpdateHooks(_translationSelectedUpdateHook);
            ChatIo.AddUpdateHooks(_wellKnownWordsUpdateHook);

            while (true)
            {
                try
                {
                    await HandleMainScenario(mainMenuCommandOrNull);
                    mainMenuCommandOrNull = null;
                }
                catch (UserAFKException)
                {
                    return;
                }
                catch (ProcessInterruptedWithMenuCommand e)
                {
                    mainMenuCommandOrNull = e.Command;
                }
                catch (Exception e)
                {
                    Botlog.WriteError(ChatIo.ChatId.Identifier, $"{ChatIo.ChatId.Username} exception: {e}", true);
                    await ChatIo.SendMessageAsync(Chat.Texts.OopsSomethingGoesWrong);
                    throw;
                }
            }
        }
        catch (Exception e)
        {
            Botlog.WriteError(ChatIo?.ChatId?.Identifier, $"Fatal on run: {e}", true);
            throw;
        }
    }

    private ChatRoom Chat { get; set; }

    private async Task HandleMainScenario(string mainMenuCommandOrNull) {
        Chat.User.OnAnyActivity();
        if (mainMenuCommandOrNull != null)
        {
            await HandleMainMenu(mainMenuCommandOrNull);
        }
        else
        {
            var message = await ChatIo.WaitUserInputAsync();
            if (message.Message?.Text != null)
                await StartToAddNewWords(message.Message?.Text);
            else
                await ChatIo.SendMessageAsync(Chat.Texts.EnterWordOrStart);
        }
    }

    private Task SendNotAllowedTooltip() => ChatIo.SendTooltip(Chat.Texts.ActionIsNotAllowed);

    private Task StartSelectLearningSets() => new SelectLearningSetsFlow(
            Chat,
            _localDictionaryService,
            _learningSetService,
            _userService,
            _usersWordsService,
            _addWordsService)
        .EnterAsync();

    private Task StartLearning() => new ExamFlow(Chat, _userService, _usersWordsService, _settings.ExamSettings)
        .EnterAsync();

    private Task StartToAddNewWords(string text = null)
        => new TranslateWordsFlow(Chat, _addWordsService, _translationSelectedUpdateHook).Enter(text);

    private Task ShowWellKnownWords() =>
        new ShowWellKnownWordsFlow(Chat, _usersWordsService, _wellKnownWordsUpdateHook).EnterAsync();

    private Task HandleMainMenu(string command) =>
        command switch {
            BotCommands.Help  => SendHelp(),
            BotCommands.Add   => StartToAddNewWords(),
            BotCommands.New   => StartSelectLearningSets(),
            BotCommands.Learn => StartLearning(),
            BotCommands.Stats => ChatProcedures.ShowStats(Chat),
            BotCommands.Start => ShowMainMenu(),
            BotCommands.Words => ShowWellKnownWords(),
            _                 => Task.CompletedTask
        };


    private async Task SendHelp() {
        await ChatIo.SendMarkdownMessageAsync(
            Chat.Texts.HelpMarkdown,
            new[] {
                new[] {
                    InlineButtons.MainMenu($"{Emojis.MainMenu} {Chat.Texts.MainMenuButton}")
                }
            });
    }

    private async Task ShowMainMenu() {
        while (true)
        {
            var translationBtn = InlineButtons.Translation($"{Chat.Texts.TranslateButton} {Emojis.Translate}");
            var examBtn = InlineButtons.Exam($"{Chat.Texts.LearnButton} {Emojis.Learning}");
            var statsBtn = InlineButtons.Stats(Chat.Texts);
            var helpBtn = InlineButtons.HowToUse(Chat.Texts);
            await ChatIo.SendMarkdownMessageAsync(
                $"{Emojis.MainMenu} {Chat.Texts.MainMenuTextMarkdown}",
                new[] {
                    new[] { translationBtn },
                    new[] { examBtn },
                    new[] { statsBtn, helpBtn }
                });

            while (true)
            {
                var action = await ChatIo.WaitUserInputAsync();

                if (action.Message != null)
                {
                    await StartToAddNewWords(action.Message.Text);
                    return;
                }

                if (action.CallbackQuery != null)
                {
                    var btn = action.CallbackQuery.Data;

                    if (btn == translationBtn.CallbackData)
                    {
                        await StartToAddNewWords();
                        return;
                    }
                    else if (btn == examBtn.CallbackData)
                    {
                        await StartLearning();
                        return;
                    }
                    else if (btn == statsBtn.CallbackData)
                    {
                        await ChatProcedures.ShowStats(Chat);
                        return;
                    }
                }

                await SendNotAllowedTooltip();
            }
        }
    }
}

}