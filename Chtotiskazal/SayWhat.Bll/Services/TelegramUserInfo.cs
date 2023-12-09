namespace SayWhat.Bll.Services;

public class TelegramUserInfo {
    public TelegramUserInfo(long telegramId, string firstName, string lastName, string userNick) {
        LastName = lastName;
        FirstName = firstName;
        TelegramId = telegramId;
        UserNick = userNick;
    }

    public long TelegramId { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string UserNick { get; }
}