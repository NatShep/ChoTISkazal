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
        public ChatRoomFlow(ChatIO chatIo) =>
          ChatIo = chatIo;
        
        public int UserId { get; set; }        
        public AddWordService AddWordSrvc { get; set; }
        public ExamService ExamSrvc { get; set; }
        public AuthorizeService AuthorizeSrvc { get; set; }
        public GraphStatsService GraphStatsSrvc { get; set; }
        
        public ChatIO ChatIo { get;}


        public void Greeting()
        {
            ChatIo.SendMessage($"Hello, {ChatIo.UserFirstName}! I am ChoTiSkazal.");
        }
        public void GoodBye()
        {
            ChatIo.SendMessage($"Bye-Bye, {ChatIo.UserFirstName}! I am OFFLINE now.");
        }
        
        public async Task Run(){ 
            string mainMenuCommandOrNull = null;
            
            var user = await AuthorizeSrvc.AuthorizeAsync(ChatIo.ChatId.Identifier, ChatIo.UserFirstName);
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
                    await ChatIo.SendMessage("bybye");
                    return;		
                }
                catch(ProcessInteruptedWithMenuCommand e){
                    mainMenuCommandOrNull = e.Command;
                }
                catch(Exception e){
                    Console.WriteLine($"Error: {e}, from {ChatIo}");
                    await ChatIo.SendMessage("Oops. something goes wrong ;(");
                }
            }
        }
        
        Task SendNotAllowedTooltip() => ChatIo.SendTooltip("action is not allowed");
        Task Examinate() => new ExamFlow(ChatIo, ExamSrvc).EnterAsync(UserId);
        
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
            var mode = new AddingWordsMode(ChatIo, AddWordSrvc);
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

        private Task SendHelp() => ChatIo.SendMessage("Call 112 for help");

        async Task ModeSelection()
        {
            while (true)
            {
                var _  = ChatIo.SendMessage("Select mode, or enter a word to translate",
                    InlineButtons.EnterWords,
                    InlineButtons.Exam,
                    InlineButtons.Stats);

                while (true)
                {
                    var action = await ChatIo.WaitUserInput();

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