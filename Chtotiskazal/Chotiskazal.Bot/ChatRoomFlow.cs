using System;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Api.Services;
using Chotiskazal.Bot.ChatFlows;
using Chotiskazal.ConsoleTesting.Services;

namespace Chotiskazal.Bot
{
    public class ChatRoomFlow
    {
        public ChatRoomFlow(Chat chat) =>
          Chat = chat;
        
        public int UserId { get; set; }        
        public AddWordService AddWordSrvc { get; set; }
        public ExamService ExamSrvc { get; set; }
        public AuthorizeService AuthorizeSrvc { get; set; }
        public GraphStatsService GraphStatsSrvc { get; set; }
        
        public Chat Chat { get;}


        public void Greeting()
        {
            Chat.SendMessage($"Hello, {Chat.UserFirstName}! I am ChoTiSkazal.");
        }
        public void GoodBye()
        {
            Chat.SendMessage($"Bye-Bye, {Chat.UserFirstName}! I am OFFLINE now.");
        }
        
        public async Task Run(){ 
            string mainMenuCommandOrNull = null;
            
            var user = await AuthorizeSrvc.AuthorizeAsync(Chat.ChatId.Identifier, Chat.UserFirstName);
            UserId = user.UserId;
            
            Greeting();
            
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
                    await Chat.SendMessage("bybye");
                    return;		
                }
                catch(ProcessInteruptedWithMenuCommand e){
                    mainMenuCommandOrNull = e.Command;
                }
                catch(Exception e){
                    Console.WriteLine($"Error: {e}, from {Chat}");
                    await Chat.SendMessage("Oops. something goes wrong ;(");
                }
            }
        }
        
        Task SendNotAllowedTooltip() => Chat.SendTooltip("action is not allowed");
        Task Examinate() => new ExamFlow(Chat, ExamSrvc).EnterAsync(UserId);
        
        //show stats to user here
        /*
        Task ShowStats()
        {
            var statsFlow = new GraphsStatsFlow(Chat, GraphStatsSrvc);
            return statsFlow.Enter();
        }
*/
        Task EnterWord(string text = null)
        {
            var mode = new AddingWordsMode(Chat, AddWordSrvc);
            return mode.Enter(UserId, text);
        }
        
        Task HandleMainMenu(string command){
            switch (command){
                case "/help": SendHelp(); break;
                case "/add":  return EnterWord(null);
                case "/train": return Examinate();
              //  case "/stats": ShowStats(); break;;
                case "/start":  break;
            }
            return Task.CompletedTask;
        }

        private Task SendHelp() => Chat.SendMessage("Call 112 for help");

        async Task ModeSelection()
        {
            while (true)
            {
                var _  = Chat.SendMessage("Select mode, or enter a word to translate",
                    InlineButtons.EnterWords,
                    InlineButtons.Exam,
                    InlineButtons.Stats);

                while (true)
                {
                    var action = await Chat.WaitUserInput();

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
                            await Examinate();
                            return;
                        }

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