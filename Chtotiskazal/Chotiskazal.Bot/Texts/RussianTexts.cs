using Chotiskazal.Bot.Interface;
using SayWhat.Bll;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Strings;

// ReSharper disable InconsistentNaming

namespace Chotiskazal.Bot.InterfaceTexts {

public class RussianTexts : IInterfaceTexts {
    public string more => "больше";
    public string less => "меньше";

    public string thenClickStart => "then click start";
    public string ChooseTheTranslation => "Выберите перевод";
    public string translatesAs => "переводится как";
    public string ChooseMissingWord => "Выберите пропущенное слово";
    public string OriginWas => "Правильный ответ";
    public string EnterMissingWord => "Введите пропущенное слово";
    public string TypoAlmostRight => "Очепяточка. Попробуем еще разок";
    public string InterfaceLanguageSetuped => "Язык интрфейса - русский.";
    public string NoWellKnownWords => "Выученных слов нет! Нажми \"учить\" и проходи тесты.";
    public string JustOneLearnedWord => "Выучено только одно слово! Какая жалость.";
    public string SelectWordInLearningSet => "Хочу учить";
    public string Skip => "Пропустить";
    public string ChooseLearningSet => "Выберите набор слов для изучения";
    public string RussianInputExpected => "Ожидался ответ на русском";
    public string EnglishInputExpected => "Ожидался ответ на английском";
    public string TodaysGoal => "Цель на день";
    public string Exams => "экзаменов";
    public string TodayGoalReached => "Цель на день - достигнута!";
    public Markdown OutOfScopeWithCandidate(string otherMeaning) {
        return Markdown.Escaped("Перевод-то правильный, но учим мы не его (имелось ввиду ")
                       .AddMarkdown($"\"{otherMeaning}\"".ToSemiBoldMarkdown())
                       .AddEscaped("?).\r\nОжидаемые переводы");
    }
    public string OutOfScopeTranslation { get; }
        = "Перевод то правильный, но учим мы не его. " +
          "\r\nОжидаемые переводы";
    public string FailedTranslationWas => "Правильный перевод";
    public string ItIsNotRightTryAgain => "Неа. Давай попробуем еще раз";
    public string SeeTheTranslation => "Посмотреть перевод";
    public string DoYouKnowTranslation => "Вы знаете перевод?";
    public string TranslationIs => "Правильный перевод";
    public string DidYouGuess => "Вы угадали?";
    public string IsItRightTranslation => "Это правильный перевод?";
    public string WriteMissingLettersOrTheWholeWord => "Напиши пропущенные буквы или все слово";
    public string Mistaken => "Ошибочка";
    public string ChooseWhichWordHasThisTranscription => "Выберите слово к которому подходит эта транскрипция";
    public string RetryAlmostRightWithTypo => "Опечатка. Давайте заново.";
    public string ShowTheTranslationButton => "Показать перевод";
    public string WriteTheTranslation => "Напишите перевод... ";
    public string RightTranslationWas => "А правильный перевод это";
    public string CorrectTranslationButQuestionWasAbout => "Перевод то верный, но вопрос был о слове";
    public string LetsTryAgain => "Давайте еще разок";
    public string ChooseTheTranscription => "Выберите транскрипцию";

    public Markdown WordsInPhraseAreShuffledWriteThemInOrder =>
        Markdown.Escaped("Слава во фразе перепутаны местами. Напишите эту фразу");
    public Markdown YouHaveATypoLetsTryAgain(string text)
        => Markdown.Escaped("Ошибочка\\. Правильно будет ")
                   .AddMarkdown(text.ToSemiBoldMarkdown())
                   .AddEscaped(". Давайте еще разок.");

    #region questionResult

    public string Passed1 => "Дыа!";
    public string PassedOpenIHopeYouWereHonest => "Надеюсь вы были честны с собой";
    public string PassedHideousWell => "Неплохо";
    public string PassedHideousWell2 => "Отлично";
    public string FailedOpenButYouWereHonest => "Вы хотя бы были честны...";
    public string FailedHideousHonestyIsGold => "Искренность - золото";
    public string FailedMistaken(string text) => $"Ой ой ой. Правильно будет - '{text}'";
    public string ThankYouForYourCommentInReport => "Спасибо :) Без обратной связи было бы тяжко!";
    public Markdown FailedOriginExampleWas => Markdown.Escaped("Неа. Фраза была");
    public Markdown FailedOriginExampleWas2 => Markdown.Escaped("Фраза была");
    public Markdown FailedDefault => Markdown.Escaped("Ой не...");
    public Markdown PassedDefault => Markdown.Escaped("И это правильный ответ!");
    public Markdown IgnoredDefault => Markdown.Escaped("Ну такое...");
    public string FailedHideousDefault => "Последний ответ был не верен";
    public string PassedHideousDefault => "Последний ответ был верен";
    public string IgnoredHideousDefault => "Ну не совсем";

    #endregion

    public string DidYouWriteSomething => "Вы что то писали? Всё это время я спал...";

    public string EnterWordOrStart =>
        "Введите английское или русское слово для перевода или жмякните /start что бы перейти в главное меню";

    public string NoTranslationsFound => "Я не нашел переводов для этого слова. Оно точно существует?";
    
    public string PhraseBecomeSoon => "Сейчас я не умею переводить фразы. Но скоро это станет возможным!";

    public Markdown LearningCarefullyStudyTheList =>
        Markdown.Escaped("Ботаем").ToSemiBold()
                .NewLine()
                .AddEscaped("Внимательно посмотрите слова из списка:");

    public string LearningDone => "Ботанье завершено";
    public string WordsInTestCount => "Слов в тренировке";
    public string YouHaveLearnedOneWord => "Вы выботали одно слово";
    public string YouForgotOneWord => "Одно слово у вас позабылось";
    public string EarnedScore => "Заработано очков";
    public string TotalScore => "Всего очков";
    public string DontPeekUpward => "Попробуйте ответить не подглядывая на верх!";

    public string NeedToAddMoreWordsBeforeLearning => "Нужно перевести больше слов дабы начать ботать";

    public Markdown Help => Markdown
                            .Escaped("Привет! Я переводчик и учитель.*\r\n\r\n").ToSemiBold()
                            .AddEscaped(
                                "1⃣ Можешь использовать меня как русско-английский переводчик. " +
                                $"Просто напиши мне слово на любом языке или нажми команду {BotCommands.Translate} для перевода.\r\n\r\n" +
                                "2⃣ Затем, когда будет времечко нажми на кнопку ")
                            .AddMarkdown($"\"Ботать {Emojis.Learning}\"".ToSemiBoldMarkdown())
                            .AddEscaped($" или набери команду {BotCommands.Learn}, что бы начать учить переведенные ранее слова.\r\n\r\n" +
                                        $"3⃣ Зарабатывай очки и следи за своими успехами при помощи команды {BotCommands.Stats}.\r\n\r\n" +
                                        $"4⃣ Жмякай команду {BotCommands.Help} что бы увидеть это сообщение.\r\n\r\n" +
                                        "\uD83D\uDE09Да, я бесплатен. Меня сделали для себя и для друзей. " +
                                        "Надеюсь это порадует вас и вы выучите миллион слов. Мои создатели проверили - это работает!");

    public Markdown MainMenuText =>
        Markdown.Escaped("Я переводчик и учитель.\r\n" +
                         "Можешь использовать меня как русско-английский переводчик.\r\n\r\n" +
                         $"Затем, когда будет свободная минутка, нажми на кнопку \"Ботать {Emojis.Learning}\" или " +
                         $"набери команду {BotCommands.Learn} что бы начать учить переведенные ранее слова");

    public string ActionIsNotAllowed => "Действие не разрешено";
    public string OopsSomethingGoesWrong =>
        "Ойойой. Что то сломалось во мне. Но вы не обращайте внимания. Нужные люди уже оповещены ;(";

    public Markdown HereAreTheTranslation(string word, string tr)
        => Markdown.Escaped("Вот что я перевел.\r\n" +
                            "Выберите один или несколько переводов, дабы заботать их в будущем").ToItalic()
                   .NewLine()
                   .NewLine()
                   .AddMarkdown($"{word.Capitalize()}".ToSemiBoldMarkdown())
                   .AddMarkdown($"{(tr == null ? "\r\n" : $"\r\n\r\n[{tr}]\r\n")}".ToPreFormattedMonoMarkdown());

    public string MessageAfterTranslationIsSelected(Translation translation)
        => $"Перевод  '{translation.TranslatedText} - {translation.OriginText}' сохранен для вас";

    public string MessageAfterTranslationIsDeselected(Translation translation)
        => $"Будто ничего и не было";

    public Markdown LearnMoreWords(in int length)
        => Markdown.Escaped($"Молодец! Ты выучил {length} слов! Давай еще!");

    public Markdown LearnSomeWords(in int length)
        => Markdown.Escaped($"Выучено слов: {length}. Давай еще!");

    public Markdown PageXofY(in int number, in int count)
        => Markdown.Escaped($"\r\nСтраница {number} из {count}...").ToMono();

    public Markdown XofY(in int number, in int y)
        => Markdown.Escaped($"\r\nСлово {number} из {y}...").ToMono();

    public string WordIsAddedForLearning(string word) =>
        $"{Emojis.SoftMark} Слово {Emojis.OpenQuote}{word}{Emojis.CloseQuote} добавлено для изучения";

    public string WordIsSkippedForLearning(string word) =>
        $"{Emojis.Failed} Слово {Emojis.OpenQuote}{word}{Emojis.CloseQuote} не будет изучено";

    public string LearningSetNotFound(string argument) => $"Набор слов '{argument}' не найден";

    public string AllWordsAreLearnedMessage(string setShortName) =>
        $"Все слова из набора '{setShortName}' были добавлены";
    public string ReportWasSentEnterAdditionalInformationAboutTheReport { get; }
        = "Отчет отправлен разработчикам. Когда они проснуться - они его обязательно посмотрят!\r\n" +
          "\r\n" +
          "Что бы лучше понять что произошло, вы можете отправить ответным сообщением любую дополнительный комментарий:";

    public string YouForgotCountWords(in int forgottenWordsCount)
        => $"Позабыто слов: {forgottenWordsCount}";

    #region buttons

    public string YesButton => "Да";
    public string NoButton => "Нет";
    public string StartButton => "Start";
    public string CancelButton => "Отмена";
    public string OneMoreLearnButton => "Еще разок";
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

    public string ZenRecomendationAfterExamWeNeedMoreNewWords =>
        $"Хватит ботать старье. Пора изучать новые слова в команде {BotCommands.New}!";

    public string Zen1WeNeedMuchMoreNewWords => $"Нам нужно сильно больше слов! Срочно нажимай {BotCommands.New}";
    public string Zen2TranslateNewWords => $"Хватит ботать! Пора изучать новые слова в команде {BotCommands.New}";
    public string Zen3TranslateNewWordsAndPassExams => "Отличный баланс новых слов и их выученности";

    public string Zen3EverythingIsGood { get; } = $"Хорошо идёте! " +
                                                  $"\r\nПереводите и ботайте.";

    public string Zen4PassExamsAndTranslateNewWords => "Тренируйтесь и переводите.";
    public string Zen5PassExams => "Вам бы поботать";
    public string Zen6YouNeedToLearn => "Только ботать! Только хардкор";
    public string StatsYourStats => "Ваши статы";
    public string StatsWordsAdded => "Добавлено слов";
    public string StatsLearnedWell => "Выучено";
    public string StatsScore => "Очки";
    public string StatsExamsPassed => "Проведено тернировок";
    public string StatsThisMonth => "В это месяце";
    public string StatsThisDay => "Сегодня";
    public string StatsActivityForLast7Weeks => "Активность за последние 7 недель";

    #endregion
}
}
