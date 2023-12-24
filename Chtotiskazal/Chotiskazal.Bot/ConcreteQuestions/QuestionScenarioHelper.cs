using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public static class QuestionScenarioHelper {
    public const string IDontKnownSubcommand = "/idontknow";

    public static string[] GetEngVariants(this IEnumerable<UserWordModel> list, string englishWord, int count)
        => list
            .Where(p => p.Word != englishWord && p.IsWord)
            .Select(e => e.Word)
            .Shuffle()
            .Take(count)
            .Append(englishWord)
            .Shuffle()
            .ToArray();

    public static string[] GetRuVariants(this IEnumerable<UserWordModel> list, UserWordTranslation translation,
        int count)
        => list
            .Where(l => l.IsWord)
            .SelectMany(e => e.TextTranslations)
            .Where(e => e != translation.Word)
            .Distinct()
            .Shuffle()
            .Take(count)
            .Append(translation.Word)
            .Shuffle()
            .ToArray();

    public static string[] GetRuPhraseVariants(this IEnumerable<UserWordModel> list, int count, string originRuPhrase,
        int maxLength = int.MaxValue) {
        var otherPhrases = list
            .Where(l => l.IsPhrase)
            .Select(o => o.RuTranslations.First().Word);
        var samples = list.SelectMany(l => l.Examples).Select(s => s.TranslatedPhrase);
        var all = otherPhrases.Concat(samples).Distinct().Where(s => s.Length < maxLength).Shuffle();
        if (count > 1)
            all = all.Take(count - 1);
        return all.Append(originRuPhrase).Shuffle().ToArray();
    }   

    public static string[] GetEnPhraseVariants(this IEnumerable<UserWordModel> list, int count,
        string originEnPhrase = null, int maxLength = int.MaxValue) {
        var otherPhrases = list
            .Where(l => l.IsPhrase && (originEnPhrase==null || !l.Word.Equals(originEnPhrase, StringComparison.InvariantCultureIgnoreCase)))
            .Select(o => o.Word);
        var samples = list.SelectMany(l => l.Examples).Select(s => s.OriginPhrase);
        var all = otherPhrases.Concat(samples).Distinct().Where(s => s.Length < maxLength).Shuffle();
        if (originEnPhrase != null) {
            if (count == 1)
                return new[] { originEnPhrase };
            all = all.Take(count - 1).Append(originEnPhrase).Shuffle();
        }
        else
            all = all.Take(count);
            
        return all.ToArray();
    }


    public static async Task<string> ChooseVariantsFlow(ChatRoom chat, string target, string[] variants) {
        if (variants.Any(c => c.Length < 38))
            await PassForShortVariants(chat, target, variants);
        else
            await PassForLongVariants(chat, target, variants);

        var choice = await chat.TryWaitInlineIntKeyboardInput();
        if (choice == null)
            return null;

        return variants[choice.Value];
    }

    private static Task PassForShortVariants(ChatRoom chat, string target, string[] variants) {
        var msg = QuestionMarkups.TranslateTemplate(target, chat.Texts.ChooseTheTranslation);
        return chat.SendMarkdownMessageAsync(msg, InlineButtons.CreateVariants(variants));
    }

    private static Task PassForLongVariants(ChatRoom chat, string target, string[] variants) {
        var msg = target
            .ToSemiBoldMarkdown()
            .NewLine()
            .NewLine()
            .AddEscaped(chat.Texts.ChooseTheTranslation)
            .NewLine();

        int num = 0;
        foreach (var variant in variants) {
            num++;
            msg = msg.NewLine()
                .AddMarkdown((num + ". ").ToSemiBoldMarkdown())
                .AddEscaped(variant);
        }

        var markup = QuestionMarkups.FreeTemplateMarkdown(msg);
        return chat.SendMarkdownMessageAsync(markup, InlineButtons.CreateVariants(
            variants.Select((_, i) => (i + 1).ToString())));
    }

    public static async Task<(OptionalUserInputResult, string)> GetEnglishUserInputOrIDontKnow(ChatRoom chat,
        Markdown message) {
        var (result, input) = await GetUserInputOrIDontKnow(chat, message);
        if (result == OptionalUserInputResult.NotAnInput ||
            (result == OptionalUserInputResult.Input && input.IsRussian()))
            await chat.SendMessageAsync(chat.Texts.EnglishInputExpected);

        return (result, input);
    }

    public static async Task<(OptionalUserInputResult, string)> GetRussianUserInputOrIDontKnow(ChatRoom chat,
        Markdown message) {
        var (result, input) = await GetUserInputOrIDontKnow(chat, message);
        if (result == OptionalUserInputResult.NotAnInput ||
            (result == OptionalUserInputResult.Input && !input.IsRussian()))
            await chat.SendMessageAsync(chat.Texts.RussianInputExpected);

        return (result, input);
    }

    private static async Task<(OptionalUserInputResult, string)> GetUserInputOrIDontKnow(ChatRoom chat,
        Markdown message) {
        await chat.SendMarkdownMessageAsync(message);
        var update = await chat.WaitUserTextInputAsync();
        if (update.Trim().Equals(IDontKnownSubcommand, StringComparison.InvariantCultureIgnoreCase))
            return (OptionalUserInputResult.IDontKnow, null);

        return string.IsNullOrEmpty(update)
            ? (OptionalUserInputResult.NotAnInput, "")
            : (OptionalUserInputResult.Input, update);
    }
}

public enum OptionalUserInputResult {
    IDontKnow,
    Input,
    NotAnInput
}