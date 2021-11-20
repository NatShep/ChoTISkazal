using SayWhat.Bll.Dto;

namespace Chotiskazal.Bot.InterfaceLang {

public interface IInterfaceTexts {
    string more { get; }
    string[] ShortDayNames { get; }
    string thenClickStartMarkdown { get; }
    string ChooseTheTranslation { get; }
    string translatesAs { get; }
    string ChooseMissingWord { get; }
    string Passed1Markdown { get; }
    string OriginWas { get; }
    string EnterMissingWord { get; }
    string TypoAlmostRight { get; }
    object FailedOriginExampleWasMarkdown { get; }
    object FailedOriginExampleWas2 { get; }
    object FailedOriginExampleWas2Markdown { get; }
    string OutOfScopeTranslationMarkdown { get; }
    string FailedTranslationWasMarkdown { get; }
    string ItIsNotRightTryAgain { get; }
    string SeeTheTranslation { get; }
    string DoYouKnowTranslation { get; }
    string TranslationIs { get; }
    string DidYouGuess { get; }
    string YesButton { get; }
    string NoButton { get; }
    string PassedOpenIHopeYouWereHonestMarkdown { get; }
    string PassedHideousWellMarkdown { get; }
    string PassedHideousWell2 { get; }
    string FailedOpenButYouWereHonestMarkdown { get; }
    string FailedHideousHonestyIsGoldMarkdown { get; }
    string IsItRightTranslation { get; }
    string Mistaken { get; }
    string ChooseWhichWordHasThisTranscription { get; }
    string RetryAlmostRightWithTypo { get; }
    string ShowTheTranslationButton { get; }
    string WriteTheTranslationMarkdown { get; }
    string RightTranslationWas { get; }
    string CorrectTranslationButQuestionWasAbout { get; }
    string LetsTryAgain { get; }
    string ChooseTheTranscription { get; }
    string WordsInPhraseAreShuffledWriteThemInOrderMarkdown { get; }
    string FailedDefaultMarkdown { get; }
    string PassedDefaultMarkdown { get; }
    string IgnoredDefaultMarkdown { get; }
    string FailedHideousDefaultMarkdown { get; }
    string PassedHideousDefaultMarkdown { get; }
    string IgnoredHideousDefault { get; }
    string TranslateButton { get; }
    string ContinueTranslateButton { get; }
    string LearnButton { get; }
    string StatsButton { get; }
    string HelpButton { get; }
    string MainMenuButton { get; }
    string DidYouWriteSomething { get; }
    string EnterWordOrStart { get; }
    string NoTranslationsFound { get; }
    string LearningCarefullyStudyTheListMarkdown { get; }
    string StartButton { get; }
    string CancelButton { get; }
    object OneMoreLearnButton { get; }
    object LearningDone { get; }
    object WordsInTestCount { get; }
    object YouHaveLearnedOneWord { get; }
    object YouForgotOneWord { get; }
    object EarnedScore { get; }
    object TotalScore { get; }
    object DontPeekUpward { get; }
    string NeedToAddMoreWordsBeforeLearning { get; }
    object less { get; }
    string Zen1WeNeedMuchMoreNewWords { get; }
    string Zen2TranslateNewWords { get; }
    string Zen3TranslateNewWordsAndPassExams { get; }
    string Zen3EverythingIsGood { get; }
    string Zen4PassExamsAndTranslateNewWords { get; }
    string Zen5PassExams { get; }
    string Zen6YouNeedToLearn { get; }
    object StatsYourStats { get; }
    object StatsWordsAdded { get; }
    object StatsLearnedWell { get; }
    object StatsScore { get; }
    object StatsExamsPassed { get; }
    object StatsThisMonth { get; }
    object StatsThisDay { get; }
    object StatsActivityForLast7Weeks { get; }
    string HelpMarkdown { get; }
    string MainMenuTextMarkdown { get; }
    string ActionIsNotAllowed { get; }
    string OopsSomethingGoesWrong { get; }
    string InterfaceLanguageSetuped { get; }
    string ShowWellKnownWords { get; }
    string NoWellKnownWords { get; }
    string JustOneLearnedWord { get; }
    string SelectWordInLearningSet { get; }
    string Skip { get;  }
    string ChooseLearningSet { get;  }
    string LearningSetsButton { get; }
    string RussianInputExpected { get; }
    string EnglishInputExpected { get; }
    string OutOfScopeWithCandidateMarkdown(string otherMeaning);
    string YouHaveATypoLetsTryAgainMarkdown(string text);
    string FailedMistakenMarkdown(string text);
    string HereAreTheTranslationMarkdown(string word, string tr);
    string MessageAfterTranslationIsSelected(Translation translation);
    string YouHaveLearnedWords(in int count);
    string YouForgotCountWords(in int forgottenWordsCount);
    string MessageAfterTranslationIsDeselected(Translation allTranslation);
    string LearnMoreWordsMarkdown(in int length);
    string LearnSomeWordsMarkdown(in int length);
    string PageXofYMarkdown(in int number, in int count);
    string XofYMarkdown(in int x, in int y);
    string WordIsAddedForLearning(string word);
    string WordIsSkippedForLearning(string word);
    string LearningSetNotFound(string argument);
    string AllWordsAreLearnedMessage(string setShortName);
}

}