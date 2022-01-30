using SayWhat.Bll;
using SayWhat.Bll.Dto;

// ReSharper disable InconsistentNaming

namespace Chotiskazal.Bot.Interface.InterfaceTexts
{
    public class EnglishTexts : IInterfaceTexts
    {
        public string more => "more";
        public string thenClickStart => "then click start";
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

        public string OutOfScopeWithCandidate(string otherMeaning)
            => $"Chosen translation is out of scope (did you mean *\"{otherMeaning}\"*?)." +
               $"\r\nExpected translations are";
        public string OutOfScopeTranslation
            => "Chosen translation is out of scope (but it is correct)." +
               "\r\nExpected translations are";
        public string FailedTranslationWas => "The translation was";
        public string ItIsNotRightTryAgain => "No. It is not right. Try again";
        public string SeeTheTranslation => "See the translation";
        public string DoYouKnowTranslation { get; } = "Do you know the translation?";
        public string TranslationIs => "Translation is";
        public string DidYouGuess => "Did you guess?";
        public string IsItRightTranslation => "Is it right translation?";
        public string Mistaken => "Mistaken";
        public string ChooseWhichWordHasThisTranscription => "Choose which word has this transcription";
        public string RetryAlmostRightWithTypo => "Almost right. But you have a typo. Let's try again";
        public string ShowTheTranslationButton => "Show the translation";
        public string WriteTheTranslation  => "Write the translation... ";
        public string RightTranslationWas => "The right translation was";
        public string CorrectTranslationButQuestionWasAbout => "Your translation was correct, but the question was about the word";
        public string LetsTryAgain => "Let's try again";
        public string ChooseTheTranscription => "Choose the transcription";
        public MarkdownObject WordsInPhraseAreShuffledWriteThemInOrderMarkdown =>
           MarkdownObject.Escaped("Words in phrase are shuffled. Write them in correct order.");

        public MarkdownObject YouHaveATypoLetsTryAgainMarkdown(string text)
            => MarkdownObject.Escaped("You have a typo. Correct spelling is ") +
               MarkdownObject.Escaped(text).ToSemiBold() +
               MarkdownObject.Escaped(". Let's try again.");
       
        #region questionResult
        public string Passed1 => "Ayeee!";
        public string PassedOpenIHopeYouWereHonest => "Good. I hope you were honest";
        public string PassedHideousWell => "Well";
        public string PassedHideousWell2 => "Good";
        public string FailedOpenButYouWereHonest => "But you were honest...";
        public string FailedHideousHonestyIsGold => "Honesty is gold...";

        public string FailedMistaken(string text)
            => $"Mistaken. Correct spelling is '{text}'";
        public MarkdownObject FailedOriginExampleWasMarkdown => MarkdownObject.Escaped("Wrong. Origin phrase was");

        public string FailedOriginExampleWas2 => "Origin phrase was";
        public MarkdownObject FailedOriginExampleWas2Markdown => MarkdownObject.Escaped("Origin phrase was");

        public MarkdownObject FailedDefaultMarkdown => MarkdownObject.Escaped("Noo...");
        public MarkdownObject PassedDefaultMarkdown => MarkdownObject.Escaped("It's right!");
        public MarkdownObject IgnoredDefaultMarkdown => MarkdownObject.Escaped("So so...");
        public string FailedHideousDefault => "Last answer was wrong";
        public string PassedHideousDefault => "Last answer was right";

        public string IgnoredHideousDefault => "Not really";

        #endregion

        public string DidYouWriteSomething => "Did you write something? I was asleep the whole time...";

        public string EnterWordOrStart => "Enter english or russian word to translate or /start to open main menu ";

        public string NoTranslationsFound => "No translations found. Check the word and try again";

        //todo cr - here and there. No need to have postfix 'Markdown' in properties.
        // Imagine - if all int-properties has postfix int, like "user.ageInt" - it is just redundant information
        public MarkdownObject LearningCarefullyStudyTheListMarkdown =>
            MarkdownObject.Escaped("Learning").ToSemiBold()
                .AddNewLine()
                .AddEscaped("Carefully study the words in the list below:");

        public string LearningDone => "Learning done";
        public string WordsInTestCount => "Words in test";
        public string YouHaveLearnedOneWord => "You have learned one word";
        public string YouForgotOneWord => "You forgot one word";
        public string EarnedScore => "Earned score";
        public string TotalScore => "Total score";
        public string DontPeekUpward => "Now try to answer without hints. Don't peek upward!";

        public string NeedToAddMoreWordsBeforeLearning => "You need to add some more words before examination";

        public string less => "less";
        public string ZenRecomendationAfterExamWeNeedMoreNewWords => $"Your words are well learned! It is time to press {BotCommands.New} and add 10-15 new words from word sets to learn";

        public MarkdownObject HelpMarkdown { get; } = MarkdownObject.Escaped("*Hello! I am a translator and teacher.*\r\n\r\n" +
                                                       "1⃣ You can use me as a regular translator. " +
                                                       $"Just write the word for translation or use {BotCommands.Translate} command to begin translate.\r\n\r\n" +
                                                       "2⃣ Then, when you have time and mood, click on the _\"Learn\"_ button or " +
                                                       $"write {BotCommands.Learn} and start learning this words.\r\n\r\n" +
                                                       $"3⃣ Earn scores for your action and watch your progress using {BotCommands.Stats} command.\r\n\r\n" +
                                                       $"4⃣ Use {BotCommands.Help} command to see info how it works.\r\n\r\n" +
                                                       "\uD83D\uDE09Yes, it's free. We have done this bot for us and our friends. " +
                                                       "And we hope it makes you a little bit happy and gonna learn billion of words. We ve checked it!");
        
        //todo cr - there is big difference between => and {get;} = for markdown object
        // For strings there was no difference - to return constant or... to return constant
        // But for markdown - you will calculate the markdown every time client code request it
        public MarkdownObject MainMenuTextMarkdown =>
            MarkdownObject.Escaped("I am a translator and teacher.\r\n" +
                                   "First you can use me as a regular translator." +
                                   "After that " +
                                   "learn this words and it helps you to speak English easily.");

        public string ActionIsNotAllowed => "action is not allowed";
        public string OopsSomethingGoesWrong => "Oops. Something goes wrong ;( \r\nWrite /start to go to main menu.";

        public MarkdownObject HereAreTheTranslationMarkdown(string word, string tr)
            => MarkdownObject.Escaped("Here are the translations.").ToItalic().AddNewLine() +
               MarkdownObject.Escaped("Choose one of them to learn them in the future").ToItalic()
                   .AddNewLine()
                   .AddNewLine() +
               MarkdownObject.Escaped(word.Capitalize()).ToSemiBold() +
               MarkdownObject.ByPassed($"{(tr == null ? "\r\n" : $"\r\n```\r\n[{MarkdownObject.Escaped(tr).GetOrdinalString()}]\r\n```")}");

        public string MessageAfterTranslationIsSelected(Translation translation)

            => $"Translation  '{translation.TranslatedText} - {translation.OriginText}' is saved";
        public string MessageAfterTranslationIsDeselected(Translation translation)
            => $"Translation  '{translation.TranslatedText} - {translation.OriginText}' is removed";

        public MarkdownObject LearnMoreWordsMarkdown(in int length)
            => MarkdownObject.Escaped($"Good job! You have learned {length} words!");

        public MarkdownObject LearnSomeWordsMarkdown(in int length)
            => MarkdownObject.Escaped($"You have learned {length} words. Let's do more!");

        public MarkdownObject PageXofYMarkdown(in int number,in int count)
            => MarkdownObject.Escaped($"\r\nPage {number} of {count}...").ToMono();

        public MarkdownObject XofYMarkdown(in int x,in int y)
            => MarkdownObject.Escaped($"{x} of {y}").ToMono();

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
        public string OneMoreLearnButton => "One more learn";
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
        public string Zen2TranslateNewWords => "Add new words";
        public string Zen3TranslateNewWordsAndPassExams => "Add new words and pass exams.";

        public string Zen3EverythingIsGood { get; } = $"Everything is perfect! " +
                                                      $"\r\nTranslate new words and pass exams.";

        public string Zen4PassExamsAndTranslateNewWords => "Pass exams and translate new words.";
        public string Zen5PassExams => "I recommend you to pass exams";
        public string Zen6YouNeedToLearn { get; } = $"Learning learning learning!";
        public string StatsYourStats => "Your stats";
        public string StatsWordsAdded => "Words added";
        public string StatsLearnedWell => "Learned well";
        public string StatsScore => "Score";
        public string StatsExamsPassed => "Exams passed";
        public string StatsThisMonth => "This month";
        public string StatsThisDay => "This day";
        public string StatsActivityForLast7Weeks => "Activity during last 7 weeks";

        #endregion
    }
}
