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
        public ChatRoomFlow(Chat chat)
        {
            Chat = chat;
        }

        public AddWordService AddWordSrvc { get; set; }
        public ExamService ExamSrvc { get; set; }
        public GraphStatsService GraphStatsSrvc { get; set; }
        
        public Chat Chat { get;}
        
        public async Task Run(){ 
            string mainMenuCommandOrNull = null;
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
        Task Examinate() => new ExamFlow(Chat, ExamSrvc).Enter(1);
        
        //show stats to user here
        /*
        Task ShowStats()
        {
            var statsFlow = new GraphsStatsFlow(Chat, GraphStatsSrvc);
            return statsFlow.Enter();
        }
*/
        //TODO Откуда взять userId?
        Task EnterWord(int userId, string text = null)
        {
            var mode = new AddingWordsMode(Chat, AddWordSrvc);
            return mode.Enter(userId, text);
        }

      

        Task HandleMainMenu(string command){
            switch (command){
                case "/help": SendHelp(); break;
                case "/add":  return EnterWord(1, null);
                case "/train": return Examinate();
              //  case "/stats": ShowStats(); break;;
                case "/start": break;
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
                        await EnterWord(1, action.Message.Text);
                        return;
                    }

                    if (action.CallbackQuery!=null)
                    {
                        var btn = action.CallbackQuery.Data;
                        if (btn == InlineButtons.EnterWords.CallbackData)
                        {
                            await EnterWord(1);
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