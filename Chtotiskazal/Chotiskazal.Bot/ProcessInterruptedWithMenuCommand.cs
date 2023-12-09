using System;

namespace Chotiskazal.Bot;

internal class ProcessInterruptedWithMenuCommand : Exception {
    public ProcessInterruptedWithMenuCommand(string command, IBotCommandHandler commandHandler) {
        Command = command;
        CommandHandler = commandHandler;
    }

    public string Command { get; }
    public IBotCommandHandler CommandHandler { get; }
}