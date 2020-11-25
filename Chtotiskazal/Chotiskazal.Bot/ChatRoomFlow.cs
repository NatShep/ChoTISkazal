using System;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using User = SayWhat.MongoDAL.Users.User;

namespace Chotiskazal.Bot
{
    public class ChatRoomFlow
    {
        public ChatRoomFlow(ChatIO chatIo,
            TelegramUserInfo userInfo,
            BotSettings settings,
            AddWordService addWordsService,
            UsersWordsService usersWordsService,
            AuthorizationService authorizationService)
        {
            ChatIo = chatIo;
            _userInfo = userInfo;
            _settings = settings;
            _addWordsService = addWordsService;
            _usersWordsService = usersWordsService;
            _authorizationService = authorizationService;
        }

        private User User { get; set; }
            
        private readonly TelegramUserInfo _userInfo;
        private readonly BotSettings _settings;
        private readonly AddWordService _addWordsService;
        private readonly UsersWordsService _usersWordsService;
        private readonly AuthorizationService _authorizationService;
        public ChatIO ChatIo { get;}
        private async Task SayHelloAsync() => await ChatIo.SendMessageAsync(_settings.WelcomeMessage);
        public async Task Run(){ 
            string mainMenuCommandOrNull = null;

            User = await _authorizationService.AuthorizeAsync(_userInfo);
            
            await SayHelloAsync();
            
            while(true)
            {
                try
                {
                    if(mainMenuCommandOrNull!=null)
                    {
                        await HandleMainMenu(mainMenuCommandOrNull); 
                        mainMenuCommandOrNull = null;
                    }
                    await ModeSelection();	
                }
                catch(UserAFKException){
                    return;		
                }
                catch(ProcessInterruptedWithMenuCommand e){
                    mainMenuCommandOrNull = e.Command;
                }
                catch(Exception e){
                    Botlog.Error(ChatIo.ChatId.Identifier, $"{ChatIo.ChatId.Username} exception: {e}");
                    await ChatIo.SendMessageAsync("Oops. something goes wrong ;(");
                }
            }
        }

        private Task SendNotAllowedTooltip() => ChatIo.SendTooltip("action is not allowed");
        private Task DoExamine() => new ExamFlow(ChatIo, _usersWordsService)
            .EnterAsync(User);
        

        private Task EnterWord(string text = null)
        {
            var mode = new AddingWordsMode(ChatIo, _addWordsService);
            return mode.Enter(User, text);
        }

        private Task HandleMainMenu(string command){
            switch (command){
                case "/help":   return SendHelp();
                case "/add":    return EnterWord();
                case "/exam":  return DoExamine();
                case "/start":  break;
            }
            return Task.CompletedTask;
        }
        
        private  async Task SendHelp() => 
            await ChatIo.SendMessageAsync(_settings.HelpMessage);

        private async Task ModeSelection()
        {
            while (true)
            {
                var _  = ChatIo.SendMessageAsync("Select mode, or enter a word to translate",
                    InlineButtons.EnterWords,
                    InlineButtons.Exam,
                    InlineButtons.Stats);

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
                    }

                    await SendNotAllowedTooltip();
                }
            }
        }
    }
}