
using Chotiskazal.Dal.Repo;
using Chotiskazal.Dal.Services;
using Chotiskazal.DAL;

namespace Chotiskazal.Api.ConsoleModes
{
    public interface IConsoleMode
    {
        string Name { get; }
        void Enter(int userId);

    }
}
