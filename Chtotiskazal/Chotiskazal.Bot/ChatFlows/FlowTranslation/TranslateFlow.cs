#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Hooks;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ChatFlows.FlowTranslation;

internal class TranslateFlow {
    private const int MaxButtonLength = 20;
    private ChatRoom Chat { get; }
    private readonly AddWordService _addWordService;
    private readonly ButtonCallbackDataService _buttonCallbackDataService;
    private readonly TranslationSelectedUpdateHook _translationSelectedUpdateHook;

    public TranslateFlow(
        ChatRoom chat,
        AddWordService addWordService,
        ButtonCallbackDataService buttonCallbackDataService,
        TranslationSelectedUpdateHook translationUpdateHookHandler
    ) {
        Chat = chat;
        _addWordService = addWordService;
        _buttonCallbackDataService = buttonCallbackDataService;
        _translationSelectedUpdateHook = translationUpdateHookHandler;
    }

    public async Task Enter(string? word = null) {
        do {
            word = await EnterSingleWordAsync(word);
        } while (!string.IsNullOrWhiteSpace(word));
    }

    private async Task<string?> EnterSingleWordAsync(string? input = null) {
        if (string.IsNullOrWhiteSpace(input)) {
            await Chat.SendMessageAsync($"{Emojis.Translate} {Chat.Texts.EnterWordOrStart}");
            input = await Chat.WaitUserTextInputAsync();
        }

        Chat.User.OnAnyActivity();

        // Search translations in local dictionary
        UserWordModel? alreadyExistUserWord = null;
        var isRussian = input.IsRussian();
        // Search word in UserWord Table for English Word
        if (!isRussian)
            alreadyExistUserWord = await _addWordService.GetWordNullByEngWord(Chat.User, input);

        // Search or get translation for word
        var translations = await _addWordService.FindInLocalDictionaryWithExamples(input);
        if (!translations.Any()) // if not, search it in Ya dictionary
            translations = await _addWordService.TranslateWordAndAddToDictionary(input);

        if (translations?.Any() != true) {
            // Translations not found. So try to translate something:
            var smartTranslation = await _addWordService.SmartTranslate(input);
            if (smartTranslation == null) {
                // There is definitely no translations for user's bullshit input (or may be gTranslate is broken)
                await Chat.SendMessageAsync(Chat.Texts.NoTranslationsFound);
                Reporter.ReportTranslationNotFound(Chat.User.TelegramId);
                return null;
            }
            // it is single "smart" translation
            translations = new List<Translation> { smartTranslation };
        }

        var handler = await GetTranslationHandler(input, translations, alreadyExistUserWord);

        if (handler != null) {
            _translationSelectedUpdateHook.SetLastTranslationHandler(handler);
            await handler.SendTranslationMessage();
        }

        Reporter.ReportTranslationRequsted(Chat.User.TelegramId, input.IsRussian());

        if (input.IsRussian()) Chat.User.OnRussianWordTranslationRequest();
        else Chat.User.OnEnglishWordTranslationRequest();

        try {
            // user request next word
            var res = await Chat.WaitUserTextInputAsync();
            // notify handler, that we leave the flow
            return res;
        }
        finally {
            var _ = handler?.OnNextTranslationRequest();
        }
    }

    private async Task<LastTranslationHandlerBase?> GetTranslationHandler(string input, IReadOnlyList<Translation> translations, UserWordModel? alreadyExistUserWord) {
        var firstTranslation = translations[0];
        if (translations.Count != 1 
            || firstTranslation.WordType != UserWordType.Phrase 
            || firstTranslation.TranslatedText.Length <= MaxButtonLength)
            // Simple word translation handler
            return await GetWordTranslationHandler(input, translations, alreadyExistUserWord);
        
        if (!firstTranslation.CanBeSavedToDictionary) {
            // it is too long phrase. Just translate it and not save
            return GetLongPhraseWithoutTranslationSelector(firstTranslation);
        }

        // it is a long phrase. So we will show it differently
        return await GetLongPhraseTranslationHandler(firstTranslation, alreadyExistUserWord != null);

    }

    private LastTranslationHandlerBase GetLongPhraseWithoutTranslationSelector(Translation phraseTranslation) =>
        new DoNotSaveTranslationHandler(Chat, _addWordService, _buttonCallbackDataService,
            Chat.Texts.HereIsThePhraseTranslation(phraseTranslation.TranslatedText));

    private async Task<LastTranslationHandlerBase> GetLongPhraseTranslationHandler(Translation phraseTranslation, bool isAdded) {
        if (!isAdded) {
            // Automaticaly select first translation if it was not selected before
            await _addWordService.AddTranslationToUser(Chat.User, phraseTranslation.GetEnRu());
        }
        return new LastPhraseTranslationHandler(chat: Chat,
            phraseTranslation: phraseTranslation,
            addWordService: _addWordService,
            buttonCallbackDataService: _buttonCallbackDataService,
            isSelected: true);
    }

    private async Task<LastTranslationHandlerBase> GetWordTranslationHandler(
        string input,
        IReadOnlyList<Translation> translations,
        UserWordModel? alreadyExistUserWord) {
        // get button selection marks. It works only for english words!
        bool[] selectionMarks = GetSelectionMarks(translations, alreadyExistUserWord);

        if (!selectionMarks[0]) {
            // Automaticaly select first translation if it was not selected before
            await _addWordService.AddTranslationToUser(Chat.User, translations[0].GetEnRu());
            selectionMarks[0] = true;
        }

        // getting first transcription
        var transcription = translations.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.EnTranscription))
            ?.EnTranscription;

        return new LastWordTranslationHandler(
            translations: translations,
            chat: Chat,
            addWordService: _addWordService,
            buttonCallbackDataService: _buttonCallbackDataService, selectionMarks, input, transcription);
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