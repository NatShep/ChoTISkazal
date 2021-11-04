﻿using System;
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
    private LearnBotCommandHandler _learnCommandHandler;
    private AddBotCommandHandler _addWordCommandHandler;
    public ChatIO ChatIo { get; }

    public async Task Run() {
        try
        {
            await Initialize();

            (string Command, IBotCommandHandler CommandHandler)? mainMenuCommandOrNull = null;

            while (true)
            {
                try
                {
                    //run main scenario or mainMenuCommand
                    await HandleMainScenario(mainMenuCommandOrNull);
                    mainMenuCommandOrNull = null;
                }
                catch (UserAFKException)
                {
                    return;
                }
                catch (ProcessInterruptedWithMenuCommand e)
                {
                    //main scenario may be interrupted with main menu command
                    mainMenuCommandOrNull = (e.Command, e.CommandHandler);
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

    private async Task Initialize() {
        //Initialize user
        var user = await _userService.GetUserOrNull(_userInfo);
        if (user == null)
        {
            var addUserTask = _userService.AddUserFromTelegram(_userInfo);
            await ChatIo.SendMessageAsync(_settings.WelcomeMessage);
            user = await addUserTask;
            Botlog.WriteInfo($"New user {user.TelegramNick}", user.TelegramId.ToString(), true);
        }

        Chat = new ChatRoom(ChatIo, user);
        // Initialize update hooks
        _translationSelectedUpdateHook = new TranslationSelectedUpdateHook(_addWordsService, Chat);
        _wellKnownWordsUpdateHook = new LeafWellKnownWordsUpdateHook(Chat);

        ChatIo.AddUpdateHook(_translationSelectedUpdateHook);
        ChatIo.AddUpdateHook(_wellKnownWordsUpdateHook);

        // Initialize  command handlers
        _learnCommandHandler = new LearnBotCommandHandler(_userService, _usersWordsService, _settings.ExamSettings);
        _addWordCommandHandler = new AddBotCommandHandler(_addWordsService, _translationSelectedUpdateHook);

        ChatIo.CommandHandlers = new[] {
            HelpBotCommandHandler.Instance,
            StatsBotCommandHandler.Instance,
            _learnCommandHandler,
            _addWordCommandHandler,
            new NewBotCommandHandler(
                _localDictionaryService, _learningSetService, _userService, _usersWordsService, _addWordsService),
            new StartBotCommandHandler(ShowMainMenu),
        };
    }

    private ChatRoom Chat { get; set; }

    private async Task HandleMainScenario((string Command, IBotCommandHandler CommandHandler)? command) {
        Chat.User.OnAnyActivity();
        if (command.HasValue)
        {
            // if main scenario was interrupted by command - then handle the command
            await command.Value.CommandHandler.Execute(command.Value.Command, Chat);
        }
        else
        {
            // handle user input as "translate" handler
            var message = await ChatIo.WaitUserInputAsync();
            if (message.Message?.Text != null)
                await _addWordCommandHandler.Execute(message.Message?.Text, Chat);
            else
                await ChatIo.SendMessageAsync(Chat.Texts.EnterWordOrStart);
        }
    }

    private Task SendNotAllowedTooltip() => ChatIo.SendTooltip(Chat.Texts.ActionIsNotAllowed);

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
                    await _addWordCommandHandler.Execute(action.Message.Text, Chat);
                    return;
                }

                if (action.CallbackQuery != null)
                {
                    var btn = action.CallbackQuery.Data;
                    if (btn == translationBtn.CallbackData)
                    {
                        await _addWordCommandHandler.Execute(String.Empty, Chat);
                        return;
                    }
                    else if (btn == examBtn.CallbackData)
                    {
                        await _learnCommandHandler.Execute(string.Empty, Chat);
                        return;
                    }
                    else if (btn == statsBtn.CallbackData)
                    {
                        await StatsBotCommandHandler.Instance.Execute(string.Empty, Chat);
                        return;
                    }
                }

                await SendNotAllowedTooltip();
            }
        }
    }
}

}