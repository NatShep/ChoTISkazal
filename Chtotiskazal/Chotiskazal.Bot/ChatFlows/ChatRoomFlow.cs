using System;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using Serilog;
using Serilog.Events;
using User = SayWhat.MongoDAL.Users.User;

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

        private User User { get; set; }

        private readonly TelegramUserInfo _userInfo;
        private readonly BotSettings _settings;
        private readonly AddWordService _addWordsService;
        private readonly UsersWordsService _usersWordsService;
        private readonly UserService _userService;
        public ChatIO ChatIo { get;}
        private async Task SayHelloAsync() => await ChatIo.SendMessageAsync(_settings.WelcomeMessage);

        public async Task Run()
        {
            try
            {
                string mainMenuCommandOrNull = null;

                User = await _userService.GetUserOrNull(_userInfo);
                if (User == null)
                {
                    var addUserTask = _userService.AddUser(_userInfo);
                    await SayHelloAsync();
                    User = await addUserTask;
                    Botlog.WriteInfo($"New user {User.TelegramNick}", User.TelegramId.ToString());
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
                        Botlog.WriteError(ChatIo.ChatId.Identifier, $"{ChatIo.ChatId.Username} exception: {e}");
                        await ChatIo.SendMessageAsync("Oops. something goes wrong ;(");
                    }
                }
            }
            catch (Exception e)
            {
                Botlog.WriteError(this.ChatIo?.ChatId?.Identifier, $"Fatal on run: {e}");
                throw;
            }
        }

        private async Task HandleMainScenario(string mainMenuCommandOrNull)
        {
            if (mainMenuCommandOrNull != null)
            {
                await HandleMainMenu(mainMenuCommandOrNull);
            }
            else
            {
                var message = await ChatIo.WaitUserInputAsync();
                if (message.Message?.Text != null)
                    await EnterWord(message.Message?.Text);
                else
                   await ChatIo.SendMessageAsync("Enter your word to translate or /start");
            }
        }

        private Task SendNotAllowedTooltip() => ChatIo.SendTooltip("action is not allowed");
        private Task DoExamine() => new ExamFlow(ChatIo, _usersWordsService,_settings.ExamSettings)
            .EnterAsync(User);
        

        private Task EnterWord(string text = null)
        {
            var mode = new AddingWordsMode(ChatIo, _addWordsService);
            return mode.Enter(User, text);
        }
        private async Task ShowStats()
        {
            var actualInfo =  await _userService.GetOrAddUser(_userInfo);
            await ChatIo.SendMessageAsync("Your stats: \r\n" +
                                          $"Words: {actualInfo.WordsCount}\r\n" +
                                          $"Translations: {actualInfo.PairsCount}\r\n" +
                                          $"Examples: {actualInfo.ExamplesCount}");
        }
        private Task HandleMainMenu(string command){
            switch (command){
                case "/help":   return SendHelp();
                case "/add":    return EnterWord();
                case "/learn":   return DoExamine();
                case "/stats":  return ShowStats();
                case "/start":  return ShowMainMenu();
            }
            return Task.CompletedTask;
        }
        
        private  async Task SendHelp() => 
            await ChatIo.SendMessageAsync(_settings.HelpMessage);

        private async Task ShowMainMenu()
        {
            while (true)
            {
                var _  = ChatIo.SendMessageAsync("I am a translator and teacher. First you use me as a regular translator. Then, when you have time and mood, click on the \"Learn\" button or the /learn command and start learning these words.",
                    new[]{new[]{
                        InlineButtons.Exam, InlineButtons.Stats}, 
                        new[]{ InlineButtons.EnterWords}});

                while (true)
                {
                    var action = await ChatIo.WaitUserInputAsync();

                    if (action.Message!=null)
                    {
                        await EnterWord(action.Message.Text);
                        return;
                    }

                    if (action.CallbackQuery!=null)
                    {
                        var btn = action.CallbackQuery.Data;
                        if (btn == InlineButtons.EnterWords.CallbackData)
                        {
                            await EnterWord();
                            return;
                        }

                        if (btn == InlineButtons.Exam.CallbackData)
                        {
                            await DoExamine();
                            return;
                        }

                        if (btn == InlineButtons.Stats.CallbackData)
                        {
                            await ShowStats();
                            return;
                        }
                    }
                    await SendNotAllowedTooltip();
                }
            }
        }

        
    }
}