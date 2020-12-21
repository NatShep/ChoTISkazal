using SayWhat.Bll;
using SayWhat.Bll.Dto;
// ReSharper disable InconsistentNaming

namespace Chotiskazal.Bot.InterfaceLang
{
    public class RussianTexts : IInterfaceTexts
    {
        public string more { get; } = "больше";
        public object less { get; } = "меньше";

        public string thenClickStartMarkdown { get; }="затем нажмите 'старт'";
        public string ChooseTheTranslation { get; } = "Выберите перевод";
        public string translatesAs { get; } = "переводится как";
        public string ChooseMissingWord { get; } = "Выберите пропущенное слово";
        public string OriginWas { get; } = "Правильный ответ";
        public string EnterMissingWord { get; } = "Введите пропущенное слово";
        public string TypoAlmostRight { get; } = "Очепяточка. Попробуем еще разок";
        
        public string InterfaceLanguageSetupped { get; } = "Язык интрфейса - русский.";

        public string OutOfScopeWithCandidateMarkdown(string otherMeaning)
            => $"Перевод то правильный, но учим мы не его (имелось ввиду '{otherMeaning}'?). Ожидаемые переводы";


        public string OutOfScopeTranslation { get; } 
            = $"Перевод то правильный, но учим мы не его. Ожидаемые переводы";

        public string FailedTranslationWasMarkdown { get; } = "Правильный перевод";
        public string ItIsNotRightTryAgain { get; } = "Неа. Давай попробуем еще раз";
        public string SeeTheTranslation { get; } = "Посмотреть перевод";
        public string DoYouKnowTranslation { get; } = $"Вы знаете перевод?";
        public string TranslationIs { get; } = "Правильный перевод";
        public string DidYouGuess { get; } = "Вы угадали?";
        public string IsItRightTranslation { get; } = "Это правильный перевод?";
        public string Mistaken { get; } = "Ошибочка";
        public string ChooseWhichWordHasThisTranscription { get; } 
            = "Выберите слово к которому подходит эта транскрипция";
        public string RetryAlmostRightWithTypo { get; } 
            = "Опечатка. Давайте заново.";
        public string ShowTheTranslationButton { get; } = "Показать перевод";
        public string WriteTheTranslationMarkdown { get; } = $"Напишите перевод... ";
        public string RightTranslationWas { get; } = "А правильный перевод это";

        public string CorrectTranslationButQuestionWasAbout { get; } =
            "Перевод то верный, но вопрос был о слове";

        public string LetsTryAgain { get; } = "Давайте еще разок";
        public string ChooseTheTranscription { get; } = "Выберите транскрипцию";
        public string WordsInPhraseAreShuffledWriteThemInOrderMarkdown { get; } =
            "Слава во фразе перепутаны местами. Напишите эту фразу";

     
        public string YouHaveATypoLetsTryAgainMarkdown(string text)
            => $"Ошибочка. Правильно будет '{text}'. Давайте еще разок.";
    
        #region questionResult
        public string Passed1Markdown { get; } = "Дыа!";
        public string PassedOpenIHopeYouWereHonestMarkdown { get; } = "Надеюсь вы были честны с собой";
        public string PassedHideousWellMarkdown { get; } = "Неплохо";
        public string PassedHideousWell2 { get; } = "Отлично";
        public string FailedOpenButYouWereHonestMarkdown { get; } = "Вы хотя бы были честны...";
        public string FailedHideousHonestyIsGoldMarkdown { get; } = "Искренность - золото";
        public string FailedMistakenMarkdown(string text)
            => $"Ой ой ой. Правильно будет - '{text}'";
        public object FailedOriginExampleWas2Markdown { get; }= "Неа. Фраза была";
        public object FailedOriginExampleWasMarkdown { get; } = "Фраза была";
        public object FailedOriginExampleWas2 { get; } = "Фраза была";
        public string FailedDefaultMarkdown { get; }= "Ой не...";
        public string PassedDefaultMarkdown { get; }= "И это правильный ответ";
        public string IgnoredDefaultMarkdown { get; }= "Ну такое ...";

        public string FailedHideousDefaultMarkdown { get; }= "Последний ответ был не верен";
        public string PassedHideousDefaultMarkdown { get; }= "Последний ответ был верен";
        
        public string IgnoredHideousDefault { get; }= "Ну не совсем";
         #endregion

        public string DidYouWriteSomething { get; } = "Вы что то писали? Всё это время я спал...";

        public string EnterWordOrStart { get; } =
            "Введите английское или русское слово для перевода или жмякните /start что бы перейти в главное меню";

        public string NoTranslationsFound { get; } 
            = "Я не нашел переводов для этого слова. Оно точно существует?";

        public string LearningCarefullyStudyTheListMarkdown { get; } = "*Ботаем*\r\n\r\n" +
                                                                    "Внимательно посмотрите слова из списка:";
        
        public object LearningDone { get; } = "Ботанье завершено";
        public object WordsInTestCount { get; } = "Слов в тренировке";
        public object YouHaveLearnedOneWord { get; } = "Вы выботали одно слово";
        public object YouForgotOneWord { get; } ="Одно слово у вас позабылось";
        public object EarnedScore { get; } = "Заработано очков";
        public object TotalScore { get; } = "Всего очков";
        public object DontPeekUpward { get; } = "Попробуйте ответить не подглядывая на верх!";

        public string NeedToAddMoreWordsBeforeLearning { get; } =
            "Нужно перевести больше слов дабы начать ботать";

   
        public string HelpMarkdown { get; } = "*Привет\\! Я переводчик и учитель\\.*\r\n\r\n" +
                                                   "1⃣ Можешь использовать меня как русско\\-английский переводчик\\. " +
                                                   "Просто напиши мне слово на любом языке или нажми команду /add для перевода\\.\r\n\r\n" +
                                                   "2⃣ Затем, когда будет времечко нажми на кнопку *\"Ботать\\!\"* или " +
                                                   "набери команду /learn что бы начать учить переведенные ранее слова\\.\r\n\r\n" +
                                                   "3⃣ Зарабатывай очки и следи за своими успехами при помощи команды /stats\\.\r\n\r\n" +
                                                   "4⃣ Жмякай команду /help что бы увидеть это сообщение\\.\r\n\r\n" +
                                                   "\uD83D\uDE09Да, я бесплатен\\. Меня сделали для себя и для друзей\\. " +
                                                   "Надеюсь это порадует вас и вы выучите миллион слов\\. Мои создатели проверили \\- это работает\\!";

        public string MainMenuTextMarkdown { get; } = "Я переводчик и учитель " +
                                              "Можешь использовать меня как русско-английский переводчик. " +
                                              "Затем, когда будет свободная минутка, нажми на кнопку *\"Ботать\\!\"* или " +
                                              "набери команду /learn что бы начать учить переведенные ранее слова";

        public string ActionIsNotAllowed { get;  } = "Действие не разрешено";
        public string OopsSomethingGoesWrong { get;  } = "Ойойой. Что то сломалось во мне. Но вы не обращайте внимания. Нужные люди уже оповещены ;(";

        public string HereAreTheTranslationMarkdown(string word, string? tr)
            => $"_Вот что я перевел\\._ \r\n" +
               $"_Выберите один или несколько переводов, дабы заботать их в будущем_\r\n\r\n" +
               $"*{word.EscapeForMarkdown().Capitalize()}*" +
               $"{(tr == null ? "\r\n" : $"\r\n```\r\n[{tr.EscapeForMarkdown()}]\r\n```")}";

        public string MessageAfterTranslationIsSelected(DictionaryTranslation translation)

            => $"Перевод  '{translation.TranslatedText} - {translation.OriginText}' сохранен для вас";
        
        public string MessageAfterTranslationIsDeselected(DictionaryTranslation translation)
            => $"Будто ничего и не было";

        public string YouHaveLearnedWords(in int count)
        => $"Выучено слов: {count}";

        public string YouForgotCountWords(in int forgottenWordsCount)
            =>$"Позабыто слов: {forgottenWordsCount}";
        
        
        #region buttons
        public string YesButton { get; } = "Да";
        public string NoButton { get; } = "Нет";
        public string StartButton { get; } = "Start";
        public string CancelButton { get; } = "Отмена";
        public object OneMoreLearnButton { get; } = "Еще разок";
        public string TranslateButton { get; } = "Перевод";
        public string LearnButton { get; } = "Ботать!";
        public string StatsButton { get; } = "Статы";
        public string HelpButton { get; } = "Помощь";
        public string MainMenuButton { get; } = "Глав меню";
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

        public string Zen1WeNeedMuchMoreNewWords { get; } = "Нам нужно больше переводов!";
        public string Zen2TranslateNewWords { get; } = "Лучше б вам слова переводить";
        public string Zen3TranslateNewWordsAndPassExams { get; } = "Перводите и ботайте.";

        public string Zen3EverythingIsGood { get; } = $"Хорошо идёте! " +
                                                      $"\r\nПереводите и ботайте.";

        public string Zen4PassExamsAndTranslateNewWords { get; } = "Тренируйтесь и переводите.";
        public string Zen5PassExams { get; } = "Вам бы поботать";
        public string Zen6YouNeedToLearn { get; } = $"Только ботать! Только хардкор";
        public object StatsYourStats { get; } = "Ваши статы";
        public object StatsWordsAdded { get; } = "Добавлено слов";
        public object StatsLearnedWell { get; } = "Выучено";
        public object StatsScore { get; } = "Очки";
        public object StatsExamsPassed { get; } = "Проведено тернировок";
        public object StatsThisMonth { get; } = "В это месяце";
        public object StatsThisDay { get; } = "Сегодня";
        public object StatsActivityForLast7Weeks { get; } = "Активность за последние 7 недель";
        #endregion
    }
}