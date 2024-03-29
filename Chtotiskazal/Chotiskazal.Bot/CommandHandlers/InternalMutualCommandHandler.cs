using System.Threading.Tasks;
using SayWhat.Bll.Services;

namespace Chotiskazal.Bot.CommandHandlers;

public class InternalMutualCommandHandler : IBotCommandHandler {
    private readonly MutualPhrasesService _mutualPhrasesService;

    public InternalMutualCommandHandler(MutualPhrasesService mutualPhrasesService) {
        _mutualPhrasesService = mutualPhrasesService;
    }

    public bool Acceptable(string text) => text == "/mutual";
    
    public string ParseArgument(string text) => null;

    public async Task Execute(string argument, ChatRoom chat) {
        await chat.SendMessageAsync("Load samples...");
        var samples = await _mutualPhrasesService.GetAllExamples();
        await chat.SendMessageAsync("Samples loaded. Find phrases");
        var phrases = await _mutualPhrasesService.FindMutualPhrases(chat.User, samples);
        await chat.SendMessageAsync($"{phrases.Count} phrases found. Add phrases");
        var count = await _mutualPhrasesService.AddMutualPhrasesToUser(chat.User, phrases, 1);
        await chat.SendMessageAsync($"{count} new phrases added. Job finished");
    }
}