using SayWhat.Bll;
using SayWhat.Bll.Dto;

// ReSharper disable InconsistentNaming

namespace Chotiskazal.Bot.InterfaceLang {

public class RussianTexts : IInterfaceTexts {
    public string more => "больше";
    public object less => "меньше";

    public string thenClickStartMarkdown => "затем нажмите 'старт'";
    public string ChooseTheTranslation => "Выберите перевод";
    public string translatesAs => "переводится как";
    public string ChooseMissingWord => "Выберите пропущенное слово";
    public string OriginWas => "Правильный ответ";
    public string EnterMissingWord => "Введите пропущенное слово";
    public string TypoAlmostRight => "Очепяточка. Попробуем еще разок";

    public string InterfaceLanguageSetuped => "Язык интрфейса - русский.";

    public string NoWellKnownWords => "Выученных слов нет\\! Нажми \"учить\" и проходи тесты\\.";
    public string JustOneLearnedWord => "Выучено только одно слово\\! Какая жалость\\.";
    public string SelectWordInLearningSet => "Хочу учить";
    public string Skip => "Пропустить";
    public string ChooseLearningSet => "Выберите набор слов для изучения";

    public string OutOfScopeWithCandidateMarkdown(string otherMeaning)
        => $"Перевод-то правильный, но учим мы не его \\(имелось ввиду *\"{otherMeaning}\"*?)\\. Ожидаемые переводы";

    public string OutOfScopeTranslationMarkdown { get; }
        = $"Перевод то правильный, но учим мы не его\\. Ожидаемые переводы";

    public string FailedTranslationWasMarkdown => "Правильный перевод";
    public string ItIsNotRightTryAgain => "Неа. Давай попробуем еще раз";
    public string SeeTheTranslation => "Посмотреть перевод";
    public string DoYouKnowTranslation { get; } = $"Вы знаете перевод?";
    public string TranslationIs => "Правильный перевод";
    public string DidYouGuess => "Вы угадали?";
    public string IsItRightTranslation => "Это правильный перевод?";
    public string Mistaken => "Ошибочка";
    public string ChooseWhichWordHasThisTranscription => "Выберите слово к которому подходит эта транскрипция";
    public string RetryAlmostRightWithTypo => "Опечатка. Давайте заново.";
    public string ShowTheTranslationButton => "Показать перевод";
    public string WriteTheTranslationMarkdown { get; } = $"Напишите перевод\\.\\.\\. ";
    public string RightTranslationWas => "А правильный перевод это";

    public string CorrectTranslationButQuestionWasAbout => "Перевод то верный, но вопрос был о слове";

    public string LetsTryAgain => "Давайте еще разок";
    public string ChooseTheTranscription => "Выберите транскрипцию";
    public string WordsInPhraseAreShuffledWriteThemInOrderMarkdown =>
        "Слава во фразе перепутаны местами\\. Напишите эту фразу";


    public string YouHaveATypoLetsTryAgainMarkdown(string text)
        => $"Ошибочка\\. Правильно будет *\"{text}\"*\\. Давайте еще разок\\.";


    #region questionResult

    public string Passed1Markdown => "Дыа\\!";
    public string PassedOpenIHopeYouWereHonestMarkdown => "Надеюсь вы были честны с собой";
    public string PassedHideousWellMarkdown => "Неплохо";
    public string PassedHideousWell2 => "Отлично";
    public string FailedOpenButYouWereHonestMarkdown => "Вы хотя бы были честны\\.\\.\\.";
    public string FailedHideousHonestyIsGoldMarkdown => "Искренность \\- золото";

    public string FailedMistakenMarkdown(string text)
        => $"Ой ой ой\\. Правильно будет \\- *\"{text}\"*";

    public object FailedOriginExampleWas2Markdown => "Неа\\. Фраза была";
    public object FailedOriginExampleWasMarkdown => "Фраза была";
    public object FailedOriginExampleWas2 => "Фраза была";
    public string FailedDefaultMarkdown => "Ой не\\.\\.\\.";
    public string PassedDefaultMarkdown => "И это правильный ответ";
    public string IgnoredDefaultMarkdown => "Ну такое \\.\\.\\.";

    public string FailedHideousDefaultMarkdown => "Последний ответ был не верен";
    public string PassedHideousDefaultMarkdown => "Последний ответ был верен";

    public string IgnoredHideousDefault => "Ну не совсем";

    #endregion


    public string DidYouWriteSomething => "Вы что то писали? Всё это время я спал...";

    public string EnterWordOrStart =>
        "Введите английское или русское слово для перевода или жмякните /start что бы перейти в главное меню";

    public string NoTranslationsFound => "Я не нашел переводов для этого слова. Оно точно существует?";

    public string LearningCarefullyStudyTheListMarkdown =>
        "*Ботаем*\r\n\r\n" +
        "Внимательно посмотрите слова из списка\\:";

    public object LearningDone => "Ботанье завершено";
    public object WordsInTestCount => "Слов в тренировке";
    public object YouHaveLearnedOneWord => "Вы выботали одно слово";
    public object YouForgotOneWord => "Одно слово у вас позабылось";
    public object EarnedScore => "Заработано очков";
    public object TotalScore => "Всего очков";
    public object DontPeekUpward => "Попробуйте ответить не подглядывая на верх!";

    public string NeedToAddMoreWordsBeforeLearning => "Нужно перевести больше слов дабы начать ботать";


    public string HelpMarkdown { get; } = "*Привет\\! Я переводчик и учитель\\.*\r\n\r\n" +
                                          "1⃣ Можешь использовать меня как русско\\-английский переводчик\\. " +
                                          $"Просто напиши мне слово на любом языке или нажми команду {BotCommands.Add} для перевода\\.\r\n\r\n" +
                                          $"2⃣ Затем, когда будет времечко нажми на кнопку *\"Ботать {Emojis.Learning}\"* или " +
                                          $"набери команду {BotCommands.Learn} что бы начать учить переведенные ранее слова\\.\r\n\r\n" +
                                          $"3⃣ Зарабатывай очки и следи за своими успехами при помощи команды {BotCommands.Stats}\\.\r\n\r\n" +
                                          $"4⃣ Жмякай команду {BotCommands.Help} что бы увидеть это сообщение\\.\r\n\r\n" +
                                          "\uD83D\uDE09Да, я бесплатен\\. Меня сделали для себя и для друзей\\. " +
                                          "Надеюсь это порадует вас и вы выучите миллион слов\\. Мои создатели проверили \\- это работает\\!";

    public string MainMenuTextMarkdown { get; } = "Я переводчик и учитель " +
                                                  "Можешь использовать меня как русско\\-английский переводчик\\.\r\n\r\n" +
                                                  $"Затем, когда будет свободная минутка, нажми на кнопку *\"Ботать {Emojis.Learning}\"* или " +
                                                  $"набери команду {BotCommands.Learn} что бы начать учить переведенные ранее слова";

    public string ActionIsNotAllowed => "Действие не разрешено";
    public string OopsSomethingGoesWrong =>
        "Ойойой. Что то сломалось во мне. Но вы не обращайте внимания. Нужные люди уже оповещены ;(";

    public string HereAreTheTranslationMarkdown(string word, string tr)
        => $"_Вот что я перевел\\._ \r\n" +
           $"_Выберите один или несколько переводов, дабы заботать их в будущем_\r\n\r\n" +
           $"*{word.EscapeForMarkdown().Capitalize()}*" +
           $"{(tr == null ? "\r\n" : $"\r\n```\r\n[{tr.EscapeForMarkdown()}]\r\n```")}";

    public string MessageAfterTranslationIsSelected(DictionaryTranslation translation)
        => $"Перевод  '{translation.TranslatedText} - {translation.OriginText}' сохранен для вас";

    public string MessageAfterTranslationIsDeselected(DictionaryTranslation translation)
        => $"Будто ничего и не было";

    public string LearnMoreWordsMarkdown(in int length)
        => $"Молодец\\! Ты выучил {length} слов\\! Давай еще\\!";

    public string LearnSomeWordsMarkdown(in int length)
        => $"Выучено слов: {length}\\. Давай еще\\!";

    public string PageXofYMarkdown(in int number, in int count)
        => $"\r\n`Страница {number} из {count}\\.\\.\\.`";

    public string XofYMarkdown(in int number, in int y) 
        => $"\r\n`Слово {number} из {y}\\.\\.\\.`";

    public string WordIsAddedForLearning(string word) =>
        $"{Emojis.SoftMark} Слово {Emojis.OpenQuote}{word}{Emojis.CloseQuote} добавлено для изучения";

    public string WordIsSkippedForLearning(string word) =>
        $"{Emojis.Failed} Слово {Emojis.OpenQuote}{word}{Emojis.CloseQuote} не будет изучено";

    public string LearningSetNotFound(string argument) => $"Набор слов '{argument}' не найден";

    public string YouHaveLearnedWords(in int count)
        => $"Выучено слов: {count}";

    public string YouForgotCountWords(in int forgottenWordsCount)
        => $"Позабыто слов: {forgottenWordsCount}";
    
    public string AllWordsAreLearnedMessage(string setShortName) =>
        $"Все слова из набора '{setShortName}' были добавлены";

    #region buttons

    public string YesButton => "Да";
    public string NoButton => "Нет";
    public string StartButton => "Start";
    public string CancelButton => "Отмена";
    public object OneMoreLearnButton => "Еще разок";
    public string TranslateButton => "Перевод";
    public string ContinueTranslateButton => "Продолжить";

    public string LearnButton => "Ботать";
    public string StatsButton => "Статы";
    public string HelpButton => "Помощь";
    public string MainMenuButton => "Глав меню";
    public string LearningSetsButton => "Наборы слов";
    public string ShowWellKnownWords => "Посмотреть, что уже выучил";

    #endregion


    #region stats

    public string[] ShortDayNames { get; } = {
        "пон",
        "вт ",
        "срд",
        "чт ",
        "птн",
        "суб",
        "вск"
    };

    public string Zen1WeNeedMuchMoreNewWords => "Нам нужно больше переводов!";
    public string Zen2TranslateNewWords => "Лучше б вам слова переводить";
    public string Zen3TranslateNewWordsAndPassExams => "Перводите и ботайте.";

    public string Zen3EverythingIsGood { get; } = $"Хорошо идёте! " +
                                                  $"\r\nПереводите и ботайте.";

    public string Zen4PassExamsAndTranslateNewWords => "Тренируйтесь и переводите.";
    public string Zen5PassExams => "Вам бы поботать";
    public string Zen6YouNeedToLearn { get; } = $"Только ботать! Только хардкор";
    public object StatsYourStats => "Ваши статы";
    public object StatsWordsAdded => "Добавлено слов";
    public object StatsLearnedWell => "Выучено";
    public object StatsScore => "Очки";
    public object StatsExamsPassed => "Проведено тернировок";
    public object StatsThisMonth => "В это месяце";
    public object StatsThisDay => "Сегодня";
    public object StatsActivityForLast7Weeks => "Активность за последние 7 недель";

    #endregion
}

}
