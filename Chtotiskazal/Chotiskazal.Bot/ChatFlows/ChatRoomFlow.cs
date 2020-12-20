﻿using System;
using System.Threading.Tasks;
using Chotiskazal.Bot.InterfaceLang;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Users;

namespace Chotiskazal.Bot.ChatFlows
{
    public class ChatRoomFlow
    {
        public ChatRoomFlow(ChatIO chatIo,
            TelegramUserInfo userInfo,
            BotSettings settings,
            AddWordService addWordsService,
            UsersWordsService usersWordsService,
            UserService userService
            )
        {
            ChatIo = chatIo;
            _userInfo = userInfo;
            _settings = settings;
            _addWordsService = addWordsService;
            _usersWordsService = usersWordsService;
            _userService = userService;
        }

        private readonly BotSettings _settings;
        private readonly AddWordService _addWordsService;
        private readonly UsersWordsService _usersWordsService;
        private readonly UserService _userService;
        private readonly TelegramUserInfo _userInfo;
        private TranslationSelectedQueryHandler _translationSelectedQueryHandler;
        public ChatIO ChatIo { get;}
        private async Task SayHelloAsync() => await ChatIo.SendMessageAsync(_settings.WelcomeMessage);

        public async Task Run()
        {
            try
            {
                string mainMenuCommandOrNull = null;

                var user = await _userService.GetUserOrNull(_userInfo);
                if (user == null)
                {
                    var addUserTask = _userService.AddUserFromTelegram(_userInfo);
                    await SayHelloAsync();
                    user = await addUserTask;
                    Botlog.WriteInfo($"New user {user.TelegramNick}", user.TelegramId.ToString(),true);
                }
                Chat = new ChatRoom(ChatIo, user);
                _translationSelectedQueryHandler = new TranslationSelectedQueryHandler(
                    _addWordsService, 
                    Chat);
                ChatIo.RegistrateCallbackQueryHandler(_translationSelectedQueryHandler);

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
                        Botlog.WriteError(ChatIo.ChatId.Identifier, $"{ChatIo.ChatId.Username} exception: {e}",true);
                        await ChatIo.SendMessageAsync(Chat.Texts.OopsSomethingGoesWrong);
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Botlog.WriteError(this.ChatIo?.ChatId?.Identifier, $"Fatal on run: {e}",true);
                throw;
            }
        }

        private ChatRoom Chat { get; set; }

        private async Task HandleMainScenario(string mainMenuCommandOrNull)
        {
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
        private Task StartLearning() => new ExamFlow(
                Chat, _userService, _usersWordsService, _settings.ExamSettings)
            .EnterAsync();

        private Task StartToAddNewWords(string text = null) 
            => new AddingWordsMode(Chat, _addWordsService, _translationSelectedQueryHandler).Enter(text);

      
        private Task HandleMainMenu(string command){
            switch (command){
                case "/help":   return SendHelp();
                case "/add":    return StartToAddNewWords();
                case "/learn":  return StartLearning();
                case "/stats":  return ChatProcedures.ShowStats(Chat);
                case "/start":  return ShowMainMenu();
            }
            return Task.CompletedTask;
        }

        private async Task SendHelp()
        {

            await ChatIo.SendMarkdownMessageAsync(Chat.Texts.HelpMarkdown,

                new[]{new[]{
                        InlineButtons.Exam(Chat.Texts), InlineButtons.Stats(Chat.Texts)},
                    new[]{ InlineButtons.Translation(Chat.Texts)}});
        }

        private async Task ShowMainMenu()
        {
            while (true)
            {
                var translationBtn = InlineButtons.Translation(Chat.Texts);
                var examBtn = InlineButtons.Exam(Chat.Texts);
                var statsBtn = InlineButtons.Stats(Chat.Texts);
                var helpBtn = InlineButtons.HowToUse(Chat.Texts);
                var _  = ChatIo.SendMessageAsync(Chat.Texts.MainMenuText,
                    new[]{
                        new[]{translationBtn },
                        new[]{examBtn },
                        new[]{statsBtn,helpBtn }
                    });

                while (true)
                {
                    var action = await ChatIo.WaitUserInputAsync();

                    if (action.Message!=null)
                    {
                        await StartToAddNewWords(action.Message.Text);
                        return;
                    }

                    if (action.CallbackQuery!=null)
                    {
                        var btn = action.CallbackQuery.Data;
                        if (btn == translationBtn.CallbackData) {
                            await StartToAddNewWords();
                            return;
                        }
                        if (btn == examBtn.CallbackData) {
                            await StartLearning();
                            return;
                        }
                        if (btn == statsBtn.CallbackData) {
                            await ChatProcedures.ShowStats(Chat);;
                            return;
                        }
                    }
                    await SendNotAllowedTooltip();
                }
            }
        }
    }
}