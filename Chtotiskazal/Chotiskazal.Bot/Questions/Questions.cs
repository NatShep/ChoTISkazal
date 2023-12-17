using Chotiskazal.Bot.ConcreteQuestions;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;

namespace Chotiskazal.Bot.Questions;

public static class Questions {
    public static readonly Question EngChoose = new("Eng Choose", new EngChooseLogic(),
        Frequency: 7,
        ExpectedScore: 0.6,
        PassScore: 0.4,
        FailScore: 1
    );

    public static readonly Question RuChoose = new("RuChoose", new RuChooseLogic(),
        Frequency: 7,
        ExpectedScore: 0.6,
        PassScore: 0.4,
        FailScore: 1
    );

    public static readonly Question EngTrust = new("Eng trust", new EnTrustLogic(),
        Frequency: 10,
        ExpectedScore: 2,
        PassScore: 0.4,
        FailScore: 0.4
    );

    public static readonly Question RuTrust = new("Ru trust", new RuTrustLogic(),
        Frequency: 10,
        ExpectedScore: 2,
        PassScore: 0.4,
        FailScore: 0.4
    );

    public static readonly Question RuTrustSingle = new("Ru trust single translation",
        new RuTrustSingleTranslationLogic(),
        Frequency: 10,
        ExpectedScore: 2,
        PassScore: 1.3,
        FailScore: 0.7
    );

    public static readonly Question EngPhraseChoose = new("Eng Choose Phrase", new EngChoosePhraseLogic(),
        Frequency: 4,
        ExpectedScore: 6,
        PassScore: 0.4,
        FailScore: 0.6
    );

    public static readonly Question RuPhraseChoose = new("Ru Choose Phrase", new RuChoosePhraseLogic(),
        Frequency: 4,
        ExpectedScore: 6,
        PassScore: 0.9,
        FailScore: 0.6
    );


    public static readonly Question EngEasyWriteMissingLetter = new("Eng write mising",
        new EngWriteMissingLettersLogic(StarredHardness.Easy),
        Frequency: 7,
        ExpectedScore: 2.1,
        PassScore: 1.3,
        FailScore: 0.52
    );

    public static readonly Question RuEasyWriteMissingLetter = new("Ru write mising",
        new RuWriteMissingLettersLogic(StarredHardness.Easy),
        Frequency: 7,
        ExpectedScore: 2.1,
        PassScore: 1.1,
        FailScore: 0.56
    );

    public static readonly Question EngHardWriteMissingLetter = new("Eng write mising",
        new EngWriteMissingLettersLogic(StarredHardness.Hard),
        Frequency: 7,
        ExpectedScore: 2.6,
        PassScore: 1.3,
        FailScore: 0.52
    );

    public static readonly Question RuHardWriteMissingLetter = new("Ru write mising",
        new RuWriteMissingLettersLogic(StarredHardness.Hard),
        Frequency: 7,
        ExpectedScore: 2.6,
        PassScore: 1.1,
        FailScore: 0.56
    );

    public static readonly Question EngChooseWordInPhrase = new("Eng Choose word in phrase",
        Scenario: new EngChooseWordInPhraseLogic(),
        Frequency: 20,
        ExpectedScore: 4,
        PassScore: 0.6,
        FailScore: 0.8
    );

    public static readonly Question ClearEngChooseWordInPhrase = new Question("Eng Choose word in phrase",
        new EngChooseWordInPhraseLogic(),
        Frequency: 20,
        ExpectedScore: 2.3,
        PassScore: 0.6,
        FailScore: 0.8
    ).Clear();

    public static readonly Question EngPhraseSubstitute = new("Eng phrase substitute",
        new EngPhraseSubstituteLogic(),
        Frequency: 12,
        ExpectedScore: 4,
        PassScore: 0.8,
        FailScore: 0.4
    );

    public static readonly Question RuPhraseSubstitute = new("Ru phrase substitute",
        new RuPhraseSubstituteLogic(),
        Frequency: 12,
        ExpectedScore: 4,
        PassScore: 0.73,
        FailScore: 0.4
    );

    public static readonly Question AssemblePhraseExam = new("Assemble phrase", new AssemblePhraseLogic(),
        Frequency: 7,
        ExpectedScore: 2.3,
        PassScore: 1.47,
        FailScore: 0.6
    );

    public static readonly Question ClearEngPhraseSubstitute = new Question("Eng phrase substitute",
        Scenario: new EngPhraseSubstituteLogic(),
        Frequency: 12,
        ExpectedScore: 6,
        PassScore: 0.8,
        FailScore: 0.4
    ).Clear();

    public static readonly Question ClearRuPhraseSubstitute = new("Ru phrase substitute",
        Scenario: new RuPhraseSubstituteLogic(),
        Frequency: 12,
        ExpectedScore: 6,
        PassScore: 0.73,
        FailScore: 0.4
    );


    public static Question EngWrite(LocalDictionaryService service) => new("Eng Write",
        Scenario: new EngWriteLogic(service),
        Frequency: 14,
        ExpectedScore: 2.6,
        PassScore: 1.8,
        FailScore: 0.7
    );

    public static Question RuWrite(LocalDictionaryService service) => new("Ru Write",
        Scenario: new RuWriteLogic(service),
        Frequency: 14,
        ExpectedScore: 2.6,
        PassScore: 1.8,
        FailScore: 0.7
    );

    public static readonly Question TranscriptionExam = new("Trans Choose",
        Scenario: new TranscriptionChooseLogic(),
        Frequency: 10,
        ExpectedScore: 1.6,
        PassScore: 0.4,
        FailScore: 0.7
    );

    public static readonly Question EngChooseByTranscriptionExam = new("Choose Eng By Transcription",
        Scenario: new TranscriptionChooseEngLogic(),
        Frequency: 7,
        ExpectedScore: 2.7,
        PassScore: 0.9,
        FailScore: 0.6
    );

    public static readonly Question RuChooseByTranscriptionExam = new("Choose Ru By Transcription",
        Scenario: new TranscriptionChooseRuLogic(),
        Frequency: 7,
        ExpectedScore: 3.3,
        PassScore: 1.0,
        FailScore: 0.6
    );

    public static readonly Question IsItRightTranslationExam = new("Eng is it right translation",
        Scenario: new IsItRightTranslationLogic(),
        Frequency: 7,
        ExpectedScore: 1.6,
        PassScore: 0.4,
        FailScore: 1.0
    );

    public static readonly Question EngChooseMultipleTranslationExam = new("Eng Choose",
        Scenario: new EngChooseMultipleTranslationsLogic(),
        Frequency: 10,
        ExpectedScore: 1.6,
        PassScore: 0.4,
        FailScore: 1.0
    );

    public static Question RuWriteSingleTranslationExam(LocalDictionaryService service) =>
        new("Ru Write Single Translation",
            Scenario: new RuWriteSingleTranslationLogic(service),
            Frequency: 10,
            ExpectedScore: 1.6,
            PassScore: 2.0,
            FailScore: 0.7
        );
}