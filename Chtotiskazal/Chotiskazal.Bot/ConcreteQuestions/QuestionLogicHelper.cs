using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public static class QuestionLogicHelper {
    public static string[] GetEngVariants(this IEnumerable<UserWordModel> list, string englishWord, int count)
        => list
            .Where(p => p.Word != englishWord)
            .Select(e => e.Word)
            .Shuffle()
            .Take(count)
            .Append(englishWord)
            .Shuffle()
            .ToArray();

    public static string[] GetRuVariants(this IEnumerable<UserWordModel> list, UserWordTranslation translation,
        int count)
        => list
            .SelectMany(e => e.TextTranslations)
            .Where(e => e != translation.Word)
            .Distinct()
            .Shuffle()
            .Take(count)
            .Append(translation.Word)
            .Shuffle()
            .ToArray();

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
}