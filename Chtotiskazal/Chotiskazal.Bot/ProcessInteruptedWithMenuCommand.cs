using System;

namespace Chotiskazal.Bot
{
    internal class ProcessInteruptedWithMenuCommand : Exception
    {
        public ProcessInteruptedWithMenuCommand(string command)
        {
            Command = command;
        }

        public string Command { get; }
    }
}