using SayWhat.Bll;
using SayWhat.Bll.Dto;
// ReSharper disable InconsistentNaming

namespace Chotiskazal.Bot.InterfaceLang
{
    public class EnglishTexts : IInterfaceTexts
    {
        public string more => "more";
        public string thenClickStartMarkdown => "then click start";
        public string ChooseTheTranslation => "Choose the translation";
        public string translatesAs => "translates as";
        public string ChooseMissingWord => "Choose missing word";
        public string OriginWas => "Origin was";
        public string EnterMissingWord => "Enter missing word";
        public string TypoAlmostRight => "Almost right. But you have a typo. Let's try again";
        public string InterfaceLanguageSetuped => "Interface language: English";
        public string JustOneLearnedWord => "You have learned just one word\\!";
        public string SelectWordInLearningSet => "Learn it";
        public string Skip => "Skip it";
        public string ChooseLearningSet => "Choose learning set";
        public string RussianInputExpected => "Russian input is expected"; 
        public string EnglishInputExpected => "English input is expected";
        public string TodaysGoal => "Goal for the day";
        public string Exams => "exams";
        public string TodayGoalReached => "You have reached the goal for the day";

        public string OutOfScopeWithCandidateMarkdown(string otherMeaning)
            => $"Chosen translation is out of scope (did you mean *\"{otherMeaning}\"*?)\\. Expected translations are";
        public string OutOfScopeTranslationMarkdown => "Chosen translation is out of scope \\(but it is correct\\)\\. Expected translations are";
        public string FailedTranslationWasMarkdown => "The translation was";
        public string ItIsNotRightTryAgain => "No. It is not right. Try again";
        public string SeeTheTranslation => "See the translation";
        public string DoYouKnowTranslation { get; } = $"Do you know the translation?";
        public string TranslationIs => "Translation is";
        public string DidYouGuess => "Did you guess?";
        public string IsItRightTranslation => "Is it right translation?";
        public string Mistaken => "Mistaken";
        public string ChooseWhichWordHasThisTranscription => "Choose which word has this transcription";
        public string RetryAlmostRightWithTypo => "Almost right. But you have a typo. Let's try again";
        public string ShowTheTranslationButton => "Show the translation";
        public string WriteTheTranslationMarkdown { get; } = $"Write the translation\\.\\.\\. ";
        public string RightTranslationWas => "The right translation was";
        public string CorrectTranslationButQuestionWasAbout => "Your translation was correct, but the question was about the word";
        public string LetsTryAgain => "Let's try again";
        public string ChooseTheTranscription => "Choose the transcription";
        public string WordsInPhraseAreShuffledWriteThemInOrderMarkdown => "Words in phrase are shuffled\\. Write them in correct order";

        public string YouHaveATypoLetsTryAgainMarkdown(string text)
            => $"You have a typo\\. Correct spelling is *\"{text}\"*\\. Let's try again\\.";
       
        #region questionResult
        public string Passed1Markdown => "Ayeee\\!";
        public string PassedOpenIHopeYouWereHonestMarkdown => "Good\\. I hope you were honest";
        public string PassedHideousWellMarkdown => "Well";
        public string PassedHideousWell2 => "Good";
        public string FailedOpenButYouWereHonestMarkdown => "But you were honest\\.\\.\\.";
        public string FailedHideousHonestyIsGoldMarkdown => "Honesty is gold";

        public string FailedMistakenMarkdown(string text)
            => $"Mistaken\\. Correct spelling is '{text}'";
        public object FailedOriginExampleWasMarkdown => "Wrong\\. Origin phrase was";

        public object FailedOriginExampleWas2 => "Origin phrase was";
        public object FailedOriginExampleWas2Markdown => "Origin phrase was";

        public string FailedDefaultMarkdown => "Noo\\.\\.\\.";
        public string PassedDefaultMarkdown => "It's right\\!";
        public string IgnoredDefaultMarkdown => "So so\\.\\.\\.";
        public string FailedHideousDefaultMarkdown => "Last answer was wrong";
        public string PassedHideousDefaultMarkdown => "Last answer was right";

        public string IgnoredHideousDefault => "Not really";

        #endregion

        public string DidYouWriteSomething => "Did you write something? I was asleep the whole time...";

        public string EnterWordOrStart => "Enter english or russian word to translate or /start to open main menu ";

        public string NoTranslationsFound => "No translations found. Check the word and try again";

        public string LearningCarefullyStudyTheListMarkdown =>
            "*Learning*\r\n"+
            "Carefully study the words in the list below:";

        public object LearningDone => "Learning done";
        public object WordsInTestCount => "Words in test";
        public object YouHaveLearnedOneWord => "You have learned one word";
        public object YouForgotOneWord => "You forgot one word";
        public object EarnedScore => "Earned score";
        public object TotalScore => "Total score";
        public object DontPeekUpward => "Now try to answer without hints. Don't peek upward!";

        public string NeedToAddMoreWordsBeforeLearning => "You need to add some more words before examination";

        public object less => "less";

        public string HelpMarkdown { get; } = "*Hello\\! I am a translator and teacher\\.*\r\n\r\n" +
                                              "1⃣ You can use me as a regular translator\\. " +
                                              $"Just write the word for translation or use {BotCommands.Add} command to begin translate\\.\r\n\r\n" +
                                              "2⃣ Then, when you have time and mood, click on the _\"Learn\"_ button or " +
                                              $"write {BotCommands.Learn} and start learning this words\\.\r\n\r\n" +
                                              $"3⃣ Earn scores for your action and watch your progress using {BotCommands.Stats} command\\.\r\n\r\n" +
                                              $"4⃣ Use {BotCommands.Help} command to see info how it works\\.\r\n\r\n" +
                                              "\uD83D\uDE09Yes, it's free\\. We have done this bot for us and our friends\\. " +
                                              "And we hope it makes you a little bit happy and gonna learn billion of words\\. We ve checked it\\!";

        public string MainMenuTextMarkdown =>
            "I am a translator and teacher\\.\r\n" +
            "First you can use me as a regular translator\\." +
            "After that " +
            "learn this words and it helps you to speak English easily\\.";

        public string ActionIsNotAllowed => "action is not allowed";
        public string OopsSomethingGoesWrong => "Oops. Something goes wrong ;( \r\nWrite /start to go to main menu.";

        public string HereAreTheTranslationMarkdown(string word, string tr)
            => $"_Here are the translations\\._ \r\n" +
               $"_Choose one of them to learn them in the future_\r\n\r\n" +
               $"*{word.EscapeForMarkdown().Capitalize()}*" +
               $"{(tr == null ? "\r\n" : $"\r\n```\r\n[{tr.EscapeForMarkdown()}]\r\n```")}";

        public string MessageAfterTranslationIsSelected(Translation translation)

            => $"Translation  '{translation.TranslatedText} - {translation.OriginText}' is saved";
        public string MessageAfterTranslationIsDeselected(Translation translation)
            => $"Translation  '{translation.TranslatedText} - {translation.OriginText}' is removed";

        public string LearnMoreWordsMarkdown(in int length)
            => $"Good job\\! You have learned {length} words\\!";

        public string LearnSomeWordsMarkdown(in int length)
            =>$"You have learned {length} words\\. Let's do more\\!";

        public string PageXofYMarkdown(in int number,in int count)
            => $"\r\n`Page {number} of {count}\\.\\.\\.`";
        
        public string XofYMarkdown(in int x,in int y)
            => $"`{x} of {y}`";

        public string WordIsAddedForLearning(string word) =>
            $"{Emojis.SoftMark} Word {Emojis.OpenQuote}{word}{Emojis.CloseQuote} is added for learning";

        public string WordIsSkippedForLearning(string word) =>
            $"{Emojis.Failed} Word {Emojis.OpenQuote}{word}{Emojis.CloseQuote} will not be studied";

        public string LearningSetNotFound(string argument) =>
            $"Learning set {argument} was not found";

        public string AllWordsAreLearnedMessage(string setShortName) =>
            $"All words from learning set '{setShortName}' were added";

        public string YouHaveLearnedWords(in int count)
            => $"You have learned {count} words";

        public string YouForgotCountWords(in int forgottenWordsCount)
            =>$"You forgot {forgottenWordsCount} words";
        
        
        #region buttons
        public string YesButton => "Yes";
        public string NoButton => "No";
        public string StartButton => "Start";
        public string CancelButton => "Cancel";
        public object OneMoreLearnButton => "One more learn";
        public string TranslateButton => "Translate";
        public string ContinueTranslateButton => "Continue";
        public string LearnButton => "Learn";
        public string StatsButton => "Stats";
        public string LearningSetsButton => "Sets of words";
        public string HelpButton => "Help";
        public string MainMenuButton => "Main menu";

        public string ShowWellKnownWords => "My learned words";
        public string NoWellKnownWords => "You haven't learned words\\!";

        #endregion
        
        #region stats
        public string[] ShortDayNames { get; } = {
            "mon",
            "tue",
            "wed",
            "thu",
            "fri",
            "sat",
            "sun"
        };

        public string Zen1WeNeedMuchMoreNewWords => "We need much more new words!";
        public string Zen2TranslateNewWords => "Translate new words";
        public string Zen3TranslateNewWordsAndPassExams => "Translate new words and pass exams.";

        public string Zen3EverythingIsGood { get; } = $"Everything is perfect! " +
                                                      $"\r\nTranslate new words and pass exams.";

        public string Zen4PassExamsAndTranslateNewWords => "Pass exams and translate new words.";
        public string Zen5PassExams => "I recommend to pass exams";
        public string Zen6YouNeedToLearn { get; } = $"Learning learning learning!";
        public object StatsYourStats => "Your stats";
        public object StatsWordsAdded => "Words added";
        public object StatsLearnedWell => "Learned well";
        public object StatsScore => "Score";
        public object StatsExamsPassed => "Exams passed";
        public object StatsThisMonth => "This month";
        public object StatsThisDay => "This day";
        public object StatsActivityForLast7Weeks => "Activity during last 7 weeks";

        #endregion
    }
}
