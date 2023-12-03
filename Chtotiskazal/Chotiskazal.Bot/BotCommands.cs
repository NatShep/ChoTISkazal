using Telegram.Bot.Types;

namespace Chotiskazal.Bot {

public class BotCommands {
    public const string Help = "/help";
    public const string Translate = "/translate";
    public const string Chlang = "/chlang";
    public const string New   = "/addnewwords";
    public const string Learn = "/learn";
    public const string Stats = "/stats";
    public const string Start = "/start";
    public const string Words = "/mywords";
    public const string LearningSetPrefix = "/set";
    public const string Report = "/report";
    public const string Settings = "/settings";


    public static readonly BotCommand[] Descriptions = {
        new BotCommand { Command = Help, Description = "Help and instructions (Помощь и инстркции)" },
        new BotCommand { Command = Start, Description = "Main menu (Главное меню)" },
        new BotCommand { Command = Translate, Description = "Translator (Переводчик)" },
        new BotCommand { Command = Learn, Description = "Learning translated words (Учить слова)" },
        new BotCommand
        { Command = New, Description = "Show learning sets (Показать наборы для изучения)" },
        new BotCommand { Command = Stats, Description = "Your stats (Твоя статистика)" },
        new BotCommand { Command = Words, Description = "Your learned words (Твои выученные слова)" },
        new BotCommand
        { Command = Chlang, Description = "Change interface language (Сменить язык интерфейса)" },
        new BotCommand
        {
            Command = Report,
            Description = "Report an bug or inaccuracy  (Сообщить об ошибке или неточности)"
        },
        new BotCommand { Command = Settings, Description = "Bot settings  (Настройки бота)" },
    };
}

}