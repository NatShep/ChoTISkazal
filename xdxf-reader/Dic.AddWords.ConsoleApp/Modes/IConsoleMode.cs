using System;
using System.Collections.Generic;
using System.Text;
using Dic.Logic.Services;

namespace Dic.AddWords.ConsoleApp.Modes
{
    public interface IConsoleMode
    {
        string Name { get; }
        void Enter(NewWordsService service);

    }
}
