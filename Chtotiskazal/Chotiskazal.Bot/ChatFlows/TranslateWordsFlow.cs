﻿#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Hooks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ChatFlows {

internal class TranslateWordsFlow {
    private ChatRoom Chat { get; }
    private readonly AddWordService _addWordService;
    private readonly TranslationSelectedUpdateHook _translationSelectedUpdateHook;

    public TranslateWordsFlow(
        ChatRoom chat,
        AddWordService addWordService,
        TranslationSelectedUpdateHook translationUpdateHookHandler
    ) {
        Chat = chat;
        _addWordService = addWordService;
        _translationSelectedUpdateHook = translationUpdateHookHandler;
    }

    public async Task Enter(string? word = null) {
        do
        {
            word = await EnterSingleWordAsync(word);
        } while (!string.IsNullOrWhiteSpace(word));
    }

    private async Task<string?> EnterSingleWordAsync(string? word = null) {
        if (string.IsNullOrWhiteSpace(word))
        {
            await Chat.SendMessageAsync($"{Emojis.Translate} {Chat.Texts.EnterWordOrStart}");
            word = await Chat.WaitUserTextInputAsync();
        }

        Chat.User.OnAnyActivity();

        // Search translations in local dictionary

        UserWordModel alreadyExistUserWord = null;

        if (!word.IsRussian())
            alreadyExistUserWord = await _addWordService.GetWordNullByEngWord(Chat.User, word);

        var translations = await _addWordService.FindInLocalDictionaryWithExamples(word);
        if (!translations.Any()) // if not, search it in Ya dictionary
            translations = await _addWordService.TranslateAndAddToDictionary(word);

        if (translations?.Any() != true)
        {
            await Chat.SendMessageAsync(Chat.Texts.NoTranslationsFound);
            return null;
        }
        
        // get button selection marks. It works only for english words!
        bool[] selectionMarks = GetSelectionMarks(translations, alreadyExistUserWord);

        if (!selectionMarks[0])
        {
            // Automaticly select first translation if it was not selected before
            await _addWordService.AddTranslationToUser(Chat.User, translations[0].GetEnRu());
            selectionMarks[0] = true;
        }

        // getting first transcription
        var transcription = translations.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.EnTranscription))
                                        ?.EnTranscription;

        var handler = new LastTranslationHandler(
            translations: translations,
            chat: Chat,
            addWordService: _addWordService);

        _translationSelectedUpdateHook.SetLastTranslationHandler(handler);

        await handler.SendTranslationMessage(word, transcription, selectionMarks);

        if (word.IsRussian()) Chat.User.OnRussianWordTranslationRequest();
        else Chat.User.OnEnglishWordTranslationRequest();

        try
        {
            // user request next word
            var res = await Chat.WaitUserTextInputAsync();
            // notify handler, that we leave the flow
            return res;
        }
        finally
        {
            handler.OnNextTranslationRequest();
        }
    }

    private bool[] GetSelectionMarks(
        IReadOnlyList<Translation> translations, UserWordModel? alreadyExistUserWord) {
        bool[] ans = new bool[translations.Count];
        if (alreadyExistUserWord == null)
            return ans;
        for (int i = 0; i < translations.Count; i++)
            ans[i] = alreadyExistUserWord.HasTranslation(translations[i].TranslatedText);
        return ans;
    }
}

}