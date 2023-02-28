using Chotiskazal.Bot.Interface;
using SayWhat.Bll.Dto;

// ReSharper disable InconsistentNaming

namespace Chotiskazal.Bot.InterfaceTexts {

public interface IInterfaceTexts {
    string more { get; }
    string[] ShortDayNames { get; }
    string thenClickStart { get; }
    string ChooseTheTranslation { get; }
    string translatesAs { get; }
    string ChooseMissingWord { get; }
    string Passed1 { get; }
    string OriginWas { get; }
    string EnterMissingWord { get; }
    string TypoAlmostRight { get; }
    string OutOfScopeTranslation { get; }
    string FailedTranslationWas { get; }
    string ItIsNotRightTryAgain { get; }
    string SeeTheTranslation { get; }
    string DoYouKnowTranslation { get; }
    string TranslationIs { get; }
    string DidYouGuess { get; }
    string YesButton { get; }
    string NoButton { get; }
    string PassedOpenIHopeYouWereHonest { get; }
    string PassedHideousWell { get; }
    string PassedHideousWell2 { get; }
    string FailedOpenButYouWereHonest { get; }
    string FailedHideousHonestyIsGold { get; }
    string IsItRightTranslation { get; }
    string WriteMissingLettersOrTheWholeWord { get; }
    string Mistaken { get; }
    string ChooseWhichWordHasThisTranscription { get; }
    string RetryAlmostRightWithTypo { get; }
    string ShowTheTranslationButton { get; }
    string WriteTheTranslation { get; }
    string RightTranslationWas { get; }
    string CorrectTranslationButQuestionWasAbout { get; }
    string LetsTryAgain { get; }
    string ChooseTheTranscription { get; }
    string FailedHideousDefault { get; }
    string PassedHideousDefault { get; }
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
    string PhraseBecomeSoon { get; }
    string StartButton { get; }
    string CancelButton { get; }
    string OneMoreLearnButton { get; }
    string LearningDone { get; }
    string WordsInTestCount { get; }
    string YouHaveLearnedOneWord { get; }
    string YouForgotOneWord { get; }
    string EarnedScore { get; }
    string TotalScore { get; }
    string DontPeekUpward { get; }
    string NeedToAddMoreWordsBeforeLearning { get; }
    string less { get; }
    string ZenRecomendationAfterExamWeNeedMoreNewWords { get; }
    string Zen1WeNeedMuchMoreNewWords { get; }
    string Zen2TranslateNewWords { get; }
    string Zen3TranslateNewWordsAndPassExams { get; }
    string Zen3EverythingIsGood { get; }
    string Zen4PassExamsAndTranslateNewWords { get; }
    string Zen5PassExams { get; }
    string Zen6YouNeedToLearn { get; }
    string StatsYourStats { get; }
    string StatsWordsAdded { get; }
    string StatsLearnedWell { get; }
    string StatsScore { get; }
    string StatsExamsPassed { get; }
    string StatsThisMonth { get; }
    string StatsThisDay { get; }
    string StatsActivityForLast7Weeks { get; }
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
    string TodaysGoal { get;  }
    string Exams { get; }
    string TodayGoalReached { get; }
    Markdown OutOfScopeWithCandidate(string otherMeaning);
    string FailedMistaken(string text);
    string MessageAfterTranslationIsSelected(Translation translation);
    string YouHaveLearnedWords(in int count);
    string YouForgotCountWords(in int forgottenWordsCount);
    string MessageAfterTranslationIsDeselected(Translation allTranslation);
    string WordIsAddedForLearning(string word);
    string WordIsSkippedForLearning(string word);
    string LearningSetNotFound(string argument);
    string AllWordsAreLearnedMessage(string setShortName);
    string ReportWasSentEnterAdditionalInformationAboutTheReport { get; }
    string ThankYouForYourCommentInReport { get; }
    Markdown FailedOriginExampleWas { get; }
    Markdown FailedOriginExampleWas2 { get; }
    Markdown WordsInPhraseAreShuffledWriteThemInOrder { get; }
    Markdown FailedDefault { get; }
    Markdown PassedDefault { get; }
    Markdown IgnoredDefault { get; }
    Markdown LearningCarefullyStudyTheList { get; }
    Markdown Help { get; }
    Markdown MainMenuText { get; }
    Markdown YouHaveATypoLetsTryAgain(string text);
    Markdown HereAreTheTranslation(string word, string tr);
    Markdown LearnMoreWords(in int length);
    Markdown LearnSomeWords(in int length);
    Markdown PageXofY(in int number, in int count);
    Markdown XofY(in int x, in int y);
}

}