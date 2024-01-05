using Telegram.Bot.Types;

namespace Chotiskazal.Bot;

public class BotCommands {
    public const string Help = "/help";
    public const string Translate = "/translate";
    public const string Chlang = "/chlang";
    public const string AddNewWords = "/addnewwords";
    public const string Learn = "/learn";
    public const string Stats = "/stats";
    public const string Start = "/start";
    public const string Words = "/mywords";
    public const string LearningSetPrefix = "/set";
    public const string Report = "/report";
    public const string Settings = "/settings";
    public const string RemoveWord = "/removeword";


    public static readonly BotCommand[] Descriptions =
    {
        new() { Command = Help, Description = "Help and instructions (Помощь и инстркции)" },
        new() { Command = Start, Description = "Main menu (Главное меню)" },
        new() { Command = Translate, Description = "Translator (Переводчик)" },
        new() { Command = Learn, Description = "Learning translated words (Учить слова)" },
        new() { Command = AddNewWords, Description = "Learn frequent words (Учить новые частотные слова)" },
        new() { Command = Stats, Description = "Your stats (Твоя статистика)" },
        new() { Command = Words, Description = "Your learned words (Твои выученные слова)" },
        new() { Command = Chlang, Description = "Change interface language (Сменить язык интерфейса)" },
        new() { Command = Report, Description = "Report an bug or inaccuracy  (Сообщить об ошибке или неточности)" },
        new() { Command = Settings, Description = "Bot settings  (Настройки бота)" },
        new() { Command = RemoveWord, Description = "Remove word  (Убрать слово из изучения)" },
    };
}