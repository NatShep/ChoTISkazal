
using Chotiskazal.Dal.Repo;
using Chotiskazal.Dal.Services;
using Chotiskazal.DAL;

namespace ConsoleTesting.Modes
{
    public interface IConsoleMode
    {
        string Name { get; }
        void Enter(User user);

    }
}
