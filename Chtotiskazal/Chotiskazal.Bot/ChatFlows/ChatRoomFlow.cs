using System;
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

        private UserModel UserModel { get; set; }

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

                UserModel = await _userService.GetUserOrNull(_userInfo);
                if (UserModel == null)
                {
                    var addUserTask = _userService.AddUserFromTelegram(_userInfo);
                    await SayHelloAsync();
                    UserModel = await addUserTask;
                    Botlog.WriteInfo($"New user {UserModel.TelegramNick}", UserModel.TelegramId.ToString(),true);
                }
                _translationSelectedQueryHandler = new TranslationSelectedQueryHandler(
                    _addWordsService, 
                    ChatIo, 
                    UserModel);
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
                        await ChatIo.SendMessageAsync(Texts.Current.OopsSomethingGoesWrong);
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
                   await ChatIo.SendMessageAsync(Texts.Current.EnterWordOrStart);
            }
        }

        private Task SendNotAllowedTooltip() => ChatIo.SendTooltip(Texts.Current.ActionIsNotAllowed);
        private Task StartLearning() => new ExamFlow(
                ChatIo, UserModel, _userService, _usersWordsService, _settings.ExamSettings)
            .EnterAsync();

        private Task StartToAddNewWords(string text = null) 
            => new AddingWordsMode(ChatIo, _addWordsService, _translationSelectedQueryHandler).Enter(UserModel, text);

      
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

            await ChatIo.SendMarkdownMessageAsync(Texts.Current.HelpMarkdown,

                new[]{new[]{
                        InlineButtons.Exam, InlineButtons.Stats},
                    new[]{ InlineButtons.Translation}});
        }

        private async Task ShowMainMenu()
        {
            while (true)
            {
                var _  = ChatIo.SendMessageAsync(Texts.Current.MainMenuText,
                    new[]{
                        new[]{ InlineButtons.Translation},
                        new[]{ InlineButtons.Exam},
                        new[]{InlineButtons.Stats, InlineButtons.HowToUse}
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
                        if (btn == InlineButtons.Translation.CallbackData) {
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