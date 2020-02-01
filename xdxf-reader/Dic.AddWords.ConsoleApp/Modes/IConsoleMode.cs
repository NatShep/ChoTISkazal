using Dic.Logic.Services;

namespace Chotiskazal.App.Modes
{
    public interface IConsoleMode
    {
        string Name { get; }
        void Enter(NewWordsService service);

    }
}
