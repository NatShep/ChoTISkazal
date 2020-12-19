using SayWhat.Bll;
using SayWhat.Bll.Dto;
// ReSharper disable InconsistentNaming

namespace Chotiskazal.Bot.InterfaceLang
{
    public class EnglishTexts : IInterfaceTexts
    {
        public string more { get; } = "more";
        public string thenClickStartMarkdown { get; }="then click start";
        public string ChooseTheTranslation { get; } = "Choose the translation";
        public string translatesAs { get; } = "translates as";
        public string ChooseMissingWord { get; } = "Choose missing word";
        public string OriginWas { get; } = "Origin was";
        public string EnterMissingWord { get; } = "Enter missing word";
        public string TypoAlmostRight { get; } = "Almost right. But you have a typo. Let's try again";

        public string OutOfScopeWithCandidate(string otherMeaning)
            => $"Chosen translation is out of scope (did you mean '{otherMeaning}'?). Expected translations are";
        public string OutOfScopeTranslation { get; } =
            "Chosen translation is out of scope (but it is correct). Expected translations are";
        public string FailedTranslationWas { get; } = "The translation was";
        public string ItIsNotRightTryAgain { get; } = "No. It is not right. Try again";
        public string SeeTheTranslation { get; } = "See the translation";
        public string DoYouKnowTranslation { get; } = $"Do you know the translation?";
        public string TranslationIs { get; } = "Translation is";
        public string DidYouGuess { get; } = "Did you guess?";
        public string IsItRightTranslation { get; } = "Is it right translation?";
        public string Mistaken { get; } = "Mistaken";
        public string ChooseWhichWordHasThisTranscription { get; } = "Choose which word has this transcription";
        public string RetryAlmostRightWithTypo { get; } = "Almost right. But you have a typo. Let's try again";
        public string ShowTheTranslationButton { get; } = "Show the translation";
        public string WriteTheTranslation { get; } = $"Write the translation... ";
        public string RightTranslationWas { get; } = "The right translation was";

        public string CorrectTranslationButQuestionWasAbout { get; } =
            "Your translation was correct, but the question was about the word";

        public string LetsTryAgain { get; } = "Let's try again";
        public string ChooseTheTranscription { get; } = "Choose the transcription";
        public string WordsInPhraseAreShufledWriteThemInOrder { get; } =
            "Words in phrase are shuffled. Write them in correct order";

     
        public string YouHaveATypoLetsTryAgain(string text)
            => $"You have a typo. Correct spelling is '{text}'. Let's try again.";
    
        #region questionResult
        public string Passed1 { get; } = "Ayeee!";
        public string PassedOpenIHopeYouWereHonest { get; } = "Good. I hope you were honest";
        public string PassedHideousWell { get; } = "Well";
        public string PassedHideousWell2 { get; } = "Good";
        public string FailedOpenButYouWereHonest { get; } = "But you were honest...";
        public string FailedHideousHonestyIsGold { get; } = "Honesty is gold";
        public string FailedMistaken(string text)
            => $"Mistaken. Correct spelling is '{text}'";
        public object FailedOriginExampleWas { get; } = "Wrong. Origin phrase was";
        public object FailedOriginExampleWas2 { get; } = "Origin phrase was";
        public string FailedDefault { get; }= "Noo...";
        public string PassedDefault { get; }= "It's right!";
        public string IgnoredDefault { get; }= "So so ...";

        public string FailedHideousDefault { get; }= "Last answer was wrong";
        public string PassedHideousDefault { get; }= "Last answer was right";
        
        public string IgnoredHideousDefault { get; }= "Not really";
         #endregion

        public string DidYouWriteSomething { get; } = "Did you write something? I was asleep the whole time...";

        public string EnterWordOrStart { get; } =
            "Enter english or russian word to translate or /start to open main menu ";

        public string NoTranslationsFound { get; } = "No translations found. Check the word and try again";

        public string LearningCarefullyStudyTheListMarkdown { get; } = "*Learning*\r\n\r\n" +
                                                                    "Carefully study the words in the list below:";
        
        public object LearningDone { get; } = "Learning done";
        public object WordsInTestCount { get; } = "Words in test";
        public object YouHaveLearnedOneWord { get; } = "You have learned one word";
        public object YouForgotOneWord { get; } ="You forgot one word";
        public object EarnedScore { get; } = "Earned score";
        public object TotalScore { get; } = "Total score";
        public object DontPeekUpward { get; } = "Now try to answer without hints. Don't peek upward!";

        public string NeedToAddMoreWordsBeforeLearning { get; } =
            "You need to add some more words before examination";

        public object less { get; } = "less";
   
        public string HelpMarkdown { get; } = "*Hello\\! I am a translator and teacher\\.*\r\n\r\n" +
                                                   "1⃣ You can use me as a regular translator\\. " +
                                                   "Just write the word for translation or use /add command to begin translate\\.\r\n\r\n" +
                                                   "2⃣ Then, when you have time and mood, click on the *\"Learn\"* button or " +
                                                   "write /learn and start learning this words\\.\r\n\r\n" +
                                                   "3⃣ Earn scores for your action and watch your progress using /stats command\\.\r\n\r\n" +
                                                   "4⃣ Use /help command to see info how it works\\.\r\n\r\n" +
                                                   "\uD83D\uDE09Yes, it's free\\. We have done this bot for us and our friends\\. " +
                                                   "And we hope it makes you a little bit happy and gonna learn billion of words\\. We ve checked it\\!";

        public string MainMenuText { get; } = "I am a translator and teacher. " +
                                                   "First you use me as a regular translator. " +
                                                   "Then, when you have time, " +
                                                   "click on the 'Learn' button or /learn command to start training translated words.";

        public string ActionIsNotAllowed { get;  } = "action is not allowed";
        public string OopsSomethingGoesWrong { get;  } = "Oops. something goes wrong ;(";

        public string HereAreTheTranslationMarkdown(string word, string? tr)
            => $"_Here are the translations\\._ \r\n" +
               $"_Choose one of them to learn them in the future_\r\n\r\n" +
               $"*{word.EscapeForMarkdown().Capitalize()}*" +
               $"{(tr == null ? "\r\n" : $"\r\n```\r\n[{tr.EscapeForMarkdown()}]\r\n```")}";

        public string MessageAfterTranslationIsSelected(DictionaryTranslation translation)
            => $"Translation  '{translation.TranslatedText} - {translation.OriginText}' is saved";

        public string YouHaveLearnedWords(in int count)
        => $"You have learned {count} words";

        public string YouForgotCountWords(in int forgottenWordsCount)
            =>$"You forgot {forgottenWordsCount} words";
        
        
        #region buttons
        public string YesButton { get; } = "Yes";
        public string NoButton { get; } = "No";
        public string StartButton { get; } = "Start";
        public string CancelButton { get; } = "Cancel";
        public object OneMoreLearnButton { get; } = "One more learn";
        public string TranslateButton { get; } = "Translate";
        public string LearnButton { get; } = "Learn";
        public string StatsButton { get; } = "Stats";
        public string HelpButton { get; } = "Help";
        public string MainMenuButton { get; } = "Main menu";
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

        public string Zen1WeNeedMuchMoreNewWords { get; } = "We need much more new words!";
        public string Zen2TranslateNewWords { get; } = "Translate new words";
        public string Zen3TranslateNewWordsAndPassExams { get; } = "Translate new words and pass exams.";

        public string Zen3EverythingIsGood { get; } = $"Everything is perfect! " +
                                                      $"\r\nTranslate new words and pass exams.";

        public string Zen4PassExamsAndTranslateNewWords { get; } = "Pass exams and translate new words.";
        public string Zen5PassExams { get; } = "PassExams";
        public string Zen6YouNeedToLearn { get; } = $"Learning learning learning!";
        public object StatsYourStats { get; } = "Your stats";
        public object StatsWordsAdded { get; } = "Words added";
        public object StatsLearnedWell { get; } = "Learned well";
        public object StatsScore { get; } = "Score";
        public object StatsExamsPassed { get; } = "Exams passed";
        public object StatsThisMonth { get; } = "This month";
        public object StatsThisDay { get; } = "This day";
        public object StatsActivityForLast7Weeks { get; } = "Activity during last 7 weeks";
        #endregion
    }
}