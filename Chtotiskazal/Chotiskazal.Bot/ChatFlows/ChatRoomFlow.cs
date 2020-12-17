﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

        private UserModel UserModel { get; set; }

        private readonly BotSettings _settings;
        private readonly AddWordService _addWordsService;
        private readonly UsersWordsService _usersWordsService;
        private readonly UserService _userService;
        private readonly TelegramUserInfo _userInfo;
        public ChatIO ChatIo { get;}
        private async Task SayHelloAsync() => await ChatIo.SendMessageAsync(_settings.WelcomeMessage);

        public async Task Run()
        {
            try
            {
                string mainMenuCommandOrNull = null;

                UserModel = await _userService.GetUserOrNull(_userInfo);
                if (UserModel == null)
                {
                    var addUserTask = _userService.AddUserFromTelegram(_userInfo);
                    await SayHelloAsync();
                    UserModel = await addUserTask;
                    Botlog.WriteInfo($"New user {UserModel.TelegramNick}", UserModel.TelegramId.ToString(),true);
                }
                
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
                        await ChatIo.SendMessageAsync("Oops. something goes wrong ;(");
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

        private async Task HandleMainScenario(string mainMenuCommandOrNull)
        {
            UserModel.OnAnyActivity();
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
                   await ChatIo.SendMessageAsync("Enter your word to translate or /start");
            }
        }

        private Task SendNotAllowedTooltip() => ChatIo.SendTooltip("action is not allowed");
        private Task StartLearning() => new ExamFlow(ChatIo, _userService,_usersWordsService, _settings.ExamSettings)
            .EnterAsync(UserModel);

        private Task StartToAddNewWords(string text = null) 
            => new AddingWordsMode(ChatIo, _addWordsService).Enter(UserModel, text);

      
        private Task HandleMainMenu(string command){
            switch (command){
                case "/help":   return SendHelp();
                case "/add":    return StartToAddNewWords();
                case "/learn":  return StartLearning();
                case "/stats":  return ChatProcedures.ShowStats(ChatIo, UserModel);
                case "/start":  return ShowMainMenu();
            }
            return Task.CompletedTask;
        }

        private async Task SendHelp()
        {
            await ChatIo.SendMarkdownMessageAsync(_settings.HelpMessage,
                new[]{new[]{
                        InlineButtons.Exam, InlineButtons.Stats},
                    new[]{ InlineButtons.EnterWords}});
        }

        private async Task ShowMainMenu()
        {
            while (true)
            {
                var _  = ChatIo.SendMessageAsync("Write any word for translate or press button you need",
                    new[]{new[]{
                        InlineButtons.Exam, InlineButtons.Stats}, 
                        new[]{ InlineButtons.EnterWords},
                        new[]{ InlineButtons.HowToUse}});

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
                        if (btn == InlineButtons.EnterWords.CallbackData) {
                            await StartToAddNewWords();
                            return;
                        }
                        if (btn == InlineButtons.Exam.CallbackData) {
                            await StartLearning();
                            return;
                        }
                        if (btn == InlineButtons.Stats.CallbackData) {
                            await ChatProcedures.ShowStats(ChatIo, UserModel);;
                            return;
                        }
                    }
                    await SendNotAllowedTooltip();
                }
            }
        }
    }
}