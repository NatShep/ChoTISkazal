using Chotiskazal.Bot.ConcreteQuestions;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;

namespace Chotiskazal.Bot.Questions;

public static class Questions {
    private const int Low = 4;
    private const int LowMid = 6;
    private const int Mid = 10;
    private const int HiMid = 15;
    private const int Hi = 20;
    
    public static readonly Question EngChoose = new("Eng Choose", new EngChooseLogic(),
        Frequency: LowMid,
        ExpectedScore: 0.6,
        PassScore: 0.4,
        FailScore: 1
    );

    public static readonly Question RuChoose = new("RuChoose", new RuChooseLogic(),
        Frequency: LowMid,
        ExpectedScore: 0.6,
        PassScore: 0.4,
        FailScore: 1
    );

    public static readonly Question EngTrust = new("Eng trust", new EnTrustLogic(),
        Frequency: Mid,
        ExpectedScore: 2,
        PassScore: 0.4,
        FailScore: 0.4
    );

    public static readonly Question RuTrust = new("Ru trust", new RuTrustLogic(),
        Frequency: Mid,
        ExpectedScore: 2,
        PassScore: 0.4,
        FailScore: 0.4
    );

    public static readonly Question RuTrustSingle = new("Ru trust single translation",
        new RuTrustSingleTranslationLogic(),
        Frequency: Mid,
        ExpectedScore: 2,
        PassScore: 1.3,
        FailScore: 0.7
    );

    public static readonly Question EngPhraseChoose = new("Eng Choose Phrase", new EngChoosePhraseLogic(),
        Frequency: Low,
        ExpectedScore: 6,
        PassScore: 0.4,
        FailScore: 0.6
    );

    public static readonly Question RuPhraseChoose = new("Ru Choose Phrase", new RuChoosePhraseLogic(),
        Frequency: Low,
        ExpectedScore: 6,
        PassScore: 0.9,
        FailScore: 0.6
    );


    public static readonly Question EngEasyWriteMissingLetter = new("Eng write mising",
        new EngWriteMissingLettersLogic(StarredHardness.Easy),
        Frequency: LowMid,
        ExpectedScore: 2.1,
        PassScore: 1.3,
        FailScore: 0.52
    );

    public static readonly Question RuEasyWriteMissingLetter = new("Ru write mising",
        new RuWriteMissingLettersLogic(StarredHardness.Easy),
        Frequency: LowMid,
        ExpectedScore: 2.1,
        PassScore: 1.1,
        FailScore: 0.56
    );

    public static readonly Question EngHardWriteMissingLetter = new("Eng write mising",
        new EngWriteMissingLettersLogic(StarredHardness.Hard),
        Frequency: LowMid,
        ExpectedScore: 2.6,
        PassScore: 1.3,
        FailScore: 0.52
    );

    public static readonly Question RuHardWriteMissingLetter = new("Ru write mising",
        new RuWriteMissingLettersLogic(StarredHardness.Hard),
        Frequency: LowMid,
        ExpectedScore: 2.6,
        PassScore: 1.1,
        FailScore: 0.56
    );

    public static readonly Question EngChooseWordInPhrase = new("Eng Choose word in phrase",
        Scenario: new EngChooseWordInPhraseLogic(),
        Frequency: Hi,
        ExpectedScore: 4,
        PassScore: 0.6,
        FailScore: 0.8
    );

    public static readonly Question ClearEngChooseWordInPhrase = new Question("Eng Choose word in phrase",
        new EngChooseWordInPhraseLogic(),
        Frequency: Hi,
        ExpectedScore: 2.3,
        PassScore: 0.6,
        FailScore: 0.8
    ).Clear();

    public static readonly Question EngPhraseSubstitute = new("Eng phrase substitute",
        new EngPhraseSubstituteLogic(),
        Frequency: HiMid,
        ExpectedScore: 4,
        PassScore: 0.8,
        FailScore: 0.4
    );

    public static readonly Question RuPhraseSubstitute = new("Ru phrase substitute",
        new RuPhraseSubstituteLogic(),
        Frequency: HiMid,
        ExpectedScore: 4,
        PassScore: 0.73,
        FailScore: 0.4
    );

    public static readonly Question AssemblePhraseExam = new("Assemble phrase", new AssemblePhraseLogic(),
        Frequency: LowMid,
        ExpectedScore: 2.3,
        PassScore: 1.47,
        FailScore: 0.6
    );

    public static readonly Question ClearEngPhraseSubstitute = new Question("Eng phrase substitute",
        Scenario: new EngPhraseSubstituteLogic(),
        Frequency: HiMid,
        ExpectedScore: 6,
        PassScore: 0.8,
        FailScore: 0.4
    ).Clear();

    public static readonly Question ClearRuPhraseSubstitute = new("Ru phrase substitute",
        Scenario: new RuPhraseSubstituteLogic(),
        Frequency: HiMid,
        ExpectedScore: 6,
        PassScore: 0.73,
        FailScore: 0.4
    );


    public static Question EngWrite(LocalDictionaryService service) => new("Eng Write",
        Scenario: new EngWriteLogic(service),
        Frequency: HiMid,
        ExpectedScore: 2.6,
        PassScore: 1.8,
        FailScore: 0.7
    );

    public static Question RuWrite(LocalDictionaryService service) => new("Ru Write",
        Scenario: new RuWriteLogic(service),
        Frequency: HiMid,
        ExpectedScore: 2.6,
        PassScore: 1.8,
        FailScore: 0.7
    );

    public static readonly Question TranscriptionExam = new("Trans Choose",
        Scenario: new TranscriptionChooseLogic(),
        Frequency: Mid,
        ExpectedScore: 1.6,
        PassScore: 0.4,
        FailScore: 0.7
    );

    public static readonly Question EngChooseByTranscriptionExam = new("Choose Eng By Transcription",
        Scenario: new TranscriptionChooseEngLogic(),
        Frequency: LowMid,
        ExpectedScore: 2.7,
        PassScore: 0.9,
        FailScore: 0.6
    );

    public static readonly Question RuChooseByTranscriptionExam = new("Choose Ru By Transcription",
        Scenario: new TranscriptionChooseRuLogic(),
        Frequency: LowMid,
        ExpectedScore: 3.3,
        PassScore: 1.0,
        FailScore: 0.6
    );

    public static readonly Question IsItRightTranslationExam = new("Eng is it right translation",
        Scenario: new IsItRightTranslationLogic(),
        Frequency: LowMid,
        ExpectedScore: 1.6,
        PassScore: 0.4,
        FailScore: 1.0
    );

    public static readonly Question EngChooseMultipleTranslationExam = new("Eng Choose",
        Scenario: new EngChooseMultipleTranslationsLogic(),
        Frequency: Mid,
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
    
    public static readonly Question[] BeginnerQuestions = {
        EngChoose,
        RuChoose,
        RuPhraseChoose,
        EngPhraseChoose,
        EngChooseWordInPhrase,
        TranscriptionExam,
        IsItRightTranslationExam,
        EngChooseMultipleTranslationExam,
        EngChooseByTranscriptionExam,
    };

    public static readonly Question[] IntermediateQuestions =
    {
        EngEasyWriteMissingLetter,
        RuEasyWriteMissingLetter,
        EngHardWriteMissingLetter,
        RuHardWriteMissingLetter,
        EngChoose,
        RuChoose,
        RuPhraseChoose,
        EngPhraseChoose,
        EngPhraseSubstitute,
        RuPhraseSubstitute,
        AssemblePhraseExam,
        EngTrust,
        RuTrust,
        RuTrustSingle,
        EngChooseByTranscriptionExam,
        RuChooseByTranscriptionExam,
        TranscriptionExam,
        IsItRightTranslationExam,
        EngChooseMultipleTranslationExam,
    };
    
    public static Question[] AdvancedQuestions(LocalDictionaryService localDictionaryService) => new[]
    {
        EngHardWriteMissingLetter,
        RuHardWriteMissingLetter,
        EngChoose,
        RuChoose,
        EngPhraseChoose,
        RuPhraseChoose,
        EngTrust,
        RuTrust,
        RuTrustSingle,
        EngWrite(localDictionaryService),
        RuWrite(localDictionaryService),
        RuWriteSingleTranslationExam(localDictionaryService),
        ClearEngPhraseSubstitute,
        ClearRuPhraseSubstitute,
        EngPhraseSubstitute,
        RuPhraseSubstitute,
        EngChooseWordInPhrase,
        ClearEngChooseWordInPhrase,
        AssemblePhraseExam,
        IsItRightTranslationExam,
        EngChooseMultipleTranslationExam,
    };
}