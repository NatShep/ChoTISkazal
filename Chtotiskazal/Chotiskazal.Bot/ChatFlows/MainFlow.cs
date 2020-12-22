using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SayWhat.Bll;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows
{
    public class MainFlow
    {
        public MainFlow(ChatIO chatIo,
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
        private TranslationSelectedUpdateHook _translationSelectedUpdateHook;

        private LeafWellKnownWordsUpdateHook _wellKnownWordsUpdateHook;
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
                _translationSelectedUpdateHook = new TranslationSelectedUpdateHook(
                    _addWordsService, 
                    Chat);
                
                _wellKnownWordsUpdateHook =new LeafWellKnownWordsUpdateHook(Chat);
                
                ChatIo.AddUpdateHooks(_translationSelectedUpdateHook);
                ChatIo.AddUpdateHooks(_wellKnownWordsUpdateHook);

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
            => new AddingWordsMode(Chat, _addWordsService, _translationSelectedUpdateHook).Enter(text);

      
        private Task HandleMainMenu(string command){
            switch (command){
                case "/help":   return SendHelp();
                case "/add":    return StartToAddNewWords();
                case "/learn":  return StartLearning();
                case "/stats":  return ChatProcedures.ShowStats(Chat);
                case "/start":  return ShowMainMenu();
                case "/words":  return ShowWellKnownWords();
            }
            return Task.CompletedTask;
        }

        private async Task ShowWellKnownWords()
        {
            var wellKnownWords = (await _usersWordsService.GetAllWords(Chat.User))
                .Where(u => u._absoluteScore > 4).ToArray();
            var paginationForWords = new List<List<UserWordModel>>();
            var i = 0;
            while (i < wellKnownWords.Length)
            {
                paginationForWords.Add(wellKnownWords.Skip(i).Take(10).ToList());
                i += 10;
            }

            var msg = new StringBuilder();
            msg.Append("*");

            if (!wellKnownWords.Any())
                msg.Append(Chat.Texts.NoWellKnownWords);
            else if (wellKnownWords.Length == 1)
                msg.Append(Chat.Texts.JustOneLearnedWord);
            else if (wellKnownWords.Length <= 4)
                msg.Append(Chat.Texts.LearnSomeWordsMarkdown(wellKnownWords.Length));
            else
                msg.Append(Chat.Texts.LearnMoreWordsMarkdown(wellKnownWords.Length));

            msg.Append("*");

            await Chat.SendMarkdownMessageAsync(msg.ToString());
            if (wellKnownWords.Length == 0)
                return;
            msg = new StringBuilder();
            foreach (var word in paginationForWords[0])
            {
                msg.Append(Emojis.ShowWellLearnedWords + " *" + word.Word + ":* " + word.AllTranslationsAsSingleString +
                           "\r\n");
            }

            if (paginationForWords.Count > 1)
                msg.Append(Chat.Texts.ShowNumberOfLists(1,paginationForWords.Count));


            InlineKeyboardButton[][] buttons = null;
            if (paginationForWords.Count > 1)
            {
                _wellKnownWordsUpdateHook.SetWellKnownWords(paginationForWords);
                _wellKnownWordsUpdateHook.SetNumberOfPaginate(0);

                buttons = new[]
                {
                    new[]
                    {
                        new InlineKeyboardButton {CallbackData = "/<<", Text = "<<"},
                        new InlineKeyboardButton {CallbackData = "/>>", Text = ">>"},
                    },
                    new[]
                    {
                        InlineButtons.MainMenu($"{Emojis.MainMenu} {Chat.Texts.MainMenuButton}"),
                        InlineButtons.Exam($"{Chat.Texts.LearnButton} {Emojis.Learning}"),
                    },
                    new[] {InlineButtons.Translation($"{Chat.Texts.TranslateButton} {Emojis.Translate}")}
                };
            }
            else
                buttons = new[]
                {
                    new[]
                    {
                        InlineButtons.MainMenu($"{Emojis.MainMenu} {Chat.Texts.MainMenuButton}"),
                        InlineButtons.Exam($"{Chat.Texts.LearnButton} {Emojis.Learning}"),
                    },
                    new[] {InlineButtons.Translation($"{Chat.Texts.TranslateButton} {Emojis.Translate}")}
                };

            await ChatIo.SendMarkdownMessageAsync(msg.ToString(), buttons);
        }

        private async Task SendHelp()
        {
            await ChatIo.SendMarkdownMessageAsync(Chat.Texts.HelpMarkdown,
                new[]
                {
                    new[]
                    {
                        InlineButtons.Exam($"{Emojis.MainMenu} {Chat.Texts.MainMenuButton}")
                    }
                });
        }

        private async Task ShowMainMenu()
        {
            while (true)
            {
                var translationBtn = InlineButtons.Translation($"{Chat.Texts.TranslateButton} {Emojis.Translate}");
                var examBtn = InlineButtons.Exam($"{Chat.Texts.LearnButton} {Emojis.Learning}");
                var statsBtn = InlineButtons.Stats(Chat.Texts);
                var helpBtn = InlineButtons.HowToUse(Chat.Texts);
                await ChatIo.SendMarkdownMessageAsync($"{Emojis.MainMenu} {Chat.Texts.MainMenuTextMarkdown}",
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