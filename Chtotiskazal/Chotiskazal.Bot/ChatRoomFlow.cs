using System;
using System.Threading.Tasks;
using Chotiskazal.Bot.ChatFlows;
using Chotiskazal.Bot.Services;
using SayWhat.MongoDAL.Users;

namespace Chotiskazal.Bot
{
    public class ChatRoomFlow
    {
        public ChatRoomFlow(ChatIO chatIo,string firstName)
        {
            ChatIo = chatIo;
            _userFirstName = firstName;
        }

        private User User { get; set; }
            
        private readonly string _userFirstName;
        public AddWordService AddWordSrvc { get; set; }
        public ExamService ExamSrvc { get; set; }
        public AuthorizeService AuthorizeSrvc { get; set; }
        public YaService YaSrvc { get; set; }
        
        public ChatIO ChatIo { get;}

        private async Task SayHelloAsync() => await ChatIo.SendMessageAsync($"Hello, {_userFirstName}! I am ChoTiSkazal.");

        public async Task SayGoodByeAsync() => await ChatIo.SendMessageAsync($"Bye-Bye, {_userFirstName}! I am OFFLINE now.");

        public async Task Run(){ 
            string mainMenuCommandOrNull = null;
            
            User = await AuthorizeSrvc.AuthorizeAsync(ChatIo.ChatId.Identifier, _userFirstName);
            
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
                    await ChatIo.SendMessageAsync("bybye");
                    return;		
                }
                catch(ProcessInterruptedWithMenuCommand e){
                    mainMenuCommandOrNull = e.Command;
                }
                catch(Exception e){
                    Console.WriteLine($"Error: {e}, from {ChatIo}");
                    await ChatIo.SendMessageAsync("Oops. something goes wrong ;(");
                }
            }
        }

        private Task SendNotAllowedTooltip() => ChatIo.SendTooltip("action is not allowed");
        private Task DoExamine() => new ExamFlow(ChatIo, ExamSrvc).EnterAsync(User);
        
        //TODO show stats to user here
        /*
        Task ShowStats()
        {
            var statsFlow = new GraphsStatsFlow(Chat, GraphStatsSrvc);
            return statsFlow.Enter();
        }
        */

        private Task EnterWord(string text = null)
        {
            var mode = new AddingWordsMode(ChatIo, AddWordSrvc,YaSrvc);
            return mode.Enter(User, text);
        }

        private Task HandleMainMenu(string command){
            switch (command){
                case "/help": SendHelp(); break;
                case "/add":  return EnterWord(null);
                case "/train": return DoExamine();
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
                        else if (btn == InlineButtons.Exam.CallbackData)
                        {
                            await DoExamine();
                            return;
                        }

                        //TODO ShowStats
                        /*   else if (btn == InlineButtons.Stats.CallbackData)
                        {
                            await ShowStats();
                            return;
                        }
                        */
                    }

                    await SendNotAllowedTooltip();
                }
            }
        }
    }
}