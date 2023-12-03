using SayWhat.MongoDAL.Users;

namespace Chotiskazal.Bot.Texts
{
    public static class TextsExtensions {
        public static IInterfaceTexts GetText(this UserModel model) =>
            model.IsEnglishInterface
                ? new EnglishTexts()
                : new RussianTexts();
    }
}