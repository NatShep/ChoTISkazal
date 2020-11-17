using System;

namespace Chotiskazal.Bot
{
    internal class ProcessInterruptedWithMenuCommand : Exception
    {
        public ProcessInterruptedWithMenuCommand(string command) 
        {
            Command = command;
        }

        public string Command { get; }
    }
}