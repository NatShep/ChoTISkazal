using System;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using Telegram.Bot.Types;
using User = SayWhat.MongoDAL.Users.User;

namespace Chotiskazal.Bot
{
    public class ChatRoomFlow
    {
        public ChatRoomFlow(
            ChatIO chatIo,
            TelegramUserInfo userInfo,
            AddWordService addWordsService, 
            UsersWordsService usersWordsService, 
            MetricService metricService, 
            AuthorizationService authorizationService)
        {
            ChatIo = chatIo;
            _userInfo = userInfo;
            _addWordsService = addWordsService;
            _usersWordsService = usersWordsService;
            _metricService = metricService;
            _authorizationService = authorizationService;
        }

        private User User { get; set; }
            
        private readonly MetricService _metricService;
        private readonly TelegramUserInfo _userInfo;
        private readonly AddWordService _addWordsService;
        private readonly UsersWordsService _usersWordsService;
        private readonly AuthorizationService _authorizationService;
        public ChatIO ChatIo { get;}

        private async Task SayHelloAsync() => await ChatIo.SendMessageAsync($"Hello, {_userInfo.FirstName}! I am ChoTiSkazal.");

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
        private Task DoExamine() => new ExamFlow(ChatIo,_metricService, _usersWordsService)
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
                case "/train":  return DoExamine();
                case "/start":  break;
            }
            return Task.CompletedTask;
        }
        
        private  async Task SendHelp() => 
            await ChatIo.SendMessageAsync("Call 112 for help");

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