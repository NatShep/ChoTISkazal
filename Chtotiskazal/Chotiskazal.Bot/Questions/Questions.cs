using Chotiskazal.Bot.ConcreteQuestions;
using SayWhat.Bll.Services;
using SayWhat.Bll.Strings;

namespace Chotiskazal.Bot.Questions;

public static class Questions
{
    private const int Low = 40;
    private const int LowMid = 60;
    private const int Mid = 100;
    private const int HiMid = 150;
    private const int Hi = 200;

    //Expected Score:
    private const double Beginner1 = 1;
    private const double Beginner2 = 2;
    private const double Intermediate1 = 3;
    private const double Intermediate2 = 4;
    private const double Intermediate3 = 5;
    private const double Intermediate4 = 6;
    private const double Advanced = 7;

    public static readonly Question EngChooseMultipleTranslationExam = new("Eng m-choose",
        Scenario: new EngChooseMultipleTranslationsScenario(),
        Frequency: Mid,
        ExpectedScore: Beginner1,
        PassScore: 0.4,
        FailScore: 1.0
    );

    public static readonly Question IsItRightTranslationExam = new("Eng is it right translation",
        Scenario: new IsItRightTranslationScenario(),
        Frequency: LowMid,
        ExpectedScore: Beginner1,
        PassScore: 0.2,
        FailScore: 1.0
    );

    public static readonly Question RuChoose = new("RuChoose", new RuChooseScenario(),
        Frequency: LowMid,
        ExpectedScore: Beginner2,
        PassScore: 0.4,
        FailScore: 1
    );


    public static readonly Question EngChoose = new("Eng Choose", new EngChooseScenario(),
        Frequency: LowMid,
        ExpectedScore: Beginner2,
        PassScore: 0.4,
        FailScore: 1
    );

    public static readonly Question EngTrust = new("Eng trust", new EnTrustScenario(),
        Frequency: LowMid,
        ExpectedScore: Beginner1,
        PassScore: 0.4,
        FailScore: 0.4
    );

    public static readonly Question RuTrust = new("Ru trust", new RuTrustScenario(),
        Frequency: LowMid,
        ExpectedScore: Beginner1,
        PassScore: 0.4,
        FailScore: 0.4
    );

    public static readonly Question TranscriptionExam = new("Trans Choose",
        Scenario: new TranscriptionChooseScenario(),
        Frequency: LowMid,
        ExpectedScore: Intermediate1,
        PassScore: 0.4,
        FailScore: 0.7
    );

    public static readonly Question RuTrustSingle = new("Ru trust single translation",
        new RuTrustSingleTranslationScenario(),
        Frequency: Mid,
        ExpectedScore: Beginner2,
        PassScore: 1.3,
        FailScore: 0.7
    );

    public static readonly Question EngEasyWriteMissingLetter = new("Eng write mising",
        new EngWriteMissingLettersScenario(StarredHardness.Easy),
        Frequency: LowMid,
        ExpectedScore: Intermediate2,
        PassScore: 1.3,
        FailScore: 0.5
    );

    public static readonly Question RuEasyWriteMissingLetter = new("Ru write mising",
        new RuWriteMissingLettersScenario(StarredHardness.Easy),
        Frequency: LowMid,
        ExpectedScore: Intermediate2,
        PassScore: 1.1,
        FailScore: 0.5
    );

    public static readonly Question EngHardWriteMissingLetter = new("Eng write mising hard",
        new EngWriteMissingLettersScenario(StarredHardness.Hard),
        Frequency: LowMid,
        ExpectedScore: Intermediate4,
        PassScore: 1.3,
        FailScore: 0.52
    );

    public static readonly Question RuHardWriteMissingLetter = new("Ru write mising hard",
        new RuWriteMissingLettersScenario(StarredHardness.Hard),
        Frequency: LowMid,
        ExpectedScore: Intermediate4,
        PassScore: 1.1,
        FailScore: 0.56
    );

    public static readonly Question EngChooseWordInExample = new("Eng Choose word in phrase",
        Scenario: new EngChooseWordInExampleScenario(),
        Frequency: Hi,
        ExpectedScore: Intermediate1,
        PassScore: 0.6,
        FailScore: 0.8
    );

    public static readonly Question ClearEngChooseWordInExample = new Question("Eng Choose word in phrase",
        new EngChooseWordInExampleScenario(),
        Frequency: Hi,
        ExpectedScore: Intermediate2,
        PassScore: 0.6,
        FailScore: 0.8
    ).Clear();

    public static readonly Question ExampleEngSubstitute = new("Eng phrase substitute",
        new ExampleEngSubstituteScenario(),
        Frequency: HiMid,
        ExpectedScore: Intermediate3,
        PassScore: 0.8,
        FailScore: 0.4
    );

    public static readonly Question ExampleRuSubstitute = new("Ru phrase substitute",
        new ExampleRuSubstituteScenario(),
        Frequency: HiMid,
        ExpectedScore: Intermediate3,
        PassScore: 0.73,
        FailScore: 0.4
    );

    public static readonly Question ExampleAssembleWithClueExam = new("Assemble phrase with clue",
        new ExampleAssembleWithClueScenario(),
        Frequency: Mid,
        ExpectedScore: Intermediate1,
        PassScore: 1,
        FailScore: 1
    );

    public static readonly Question ExampleAssembleExam = new("Assemble phrase", new ExampleAssembleScenario(),
        Frequency: Mid,
        ExpectedScore: Intermediate2,
        PassScore: 1.47,
        FailScore: 0.6
    );


    public static readonly Question PhraseAssembleWithClueExam = new("Phrase: Assemble with clue",
        new PhraseAssembleWithClueScenario(),
        Frequency: HiMid,
        ExpectedScore: Intermediate2,
        PassScore: 1,
        FailScore: 1
    );

    public static readonly Question PhraseAssembleExam = new("Phrase: Assemble", new PhraseAssembleScenario(),
        Frequency: Mid,
        ExpectedScore: Intermediate3,
        PassScore: 1.47,
        FailScore: 0.6
    );

    public static readonly Question PhraseRuChoose = new("Phrase: RuChoose", new PhraseRuChooseScenario(),
        Frequency: Mid,
        ExpectedScore: Intermediate2,
        PassScore: 1,
        FailScore: 1
    );

    public static readonly Question PhraseEngChoose = new("Phrase: Eng Choose", new PhraseEngChooseScenario(),
        Frequency: Mid,
        ExpectedScore: Intermediate2,
        PassScore: 1,
        FailScore: 1
    );

    public static Question EngWrite(LocalDictionaryService service) => new("Eng Write",
        Scenario: new EngWriteScenario(service),
        Frequency: HiMid,
        ExpectedScore: Intermediate4,
        PassScore: 1.8,
        FailScore: 0.7
    );

    public static Question RuWrite(LocalDictionaryService service) => new("Ru Write",
        Scenario: new RuWriteScenario(service),
        Frequency: HiMid,
        ExpectedScore: Intermediate4,
        PassScore: 1.8,
        FailScore: 0.7
    );

    public static readonly Question EngChooseByTranscriptionExam = new("Choose Eng By Transcription",
        Scenario: new TranscriptionChooseEngScenario(),
        Frequency: Low,
        ExpectedScore: Intermediate3,
        PassScore: 0.9,
        FailScore: 0.6
    );

    public static readonly Question RuChooseByTranscriptionExam = new("Choose Ru By Transcription",
        Scenario: new TranscriptionChooseRuScenario(),
        Frequency: LowMid,
        ExpectedScore: Intermediate4,
        PassScore: 1.0,
        FailScore: 0.6
    );

    public static readonly Question EngExampleChoose = new("Eng Choose Phrase", new ExampleEngChooseScenario(),
        Frequency: Mid,
        ExpectedScore: Intermediate4,
        PassScore: 0.4,
        FailScore: 0.6
    );

    public static readonly Question RuExampleChoose = new("Ru Choose Phrase", new ExampleRuChooseScenario(),
        Frequency: Mid,
        ExpectedScore: Intermediate4,
        PassScore: 0.9,
        FailScore: 0.6
    );

    public static readonly Question ClearExampleEngSubstitute = new Question("Eng phrase substitute",
        Scenario: new ExampleEngSubstituteScenario(),
        Frequency: HiMid,
        ExpectedScore: Intermediate4,
        PassScore: 0.8,
        FailScore: 0.4
    ).Clear();

    public static readonly Question ClearExampleRuSubstitute = new("Ru phrase substitute",
        Scenario: new ExampleRuSubstituteScenario(),
        Frequency: HiMid,
        ExpectedScore: Intermediate4,
        PassScore: 0.73,
        FailScore: 0.4
    );

    public static Question RuWriteSingleTranslation(LocalDictionaryService service) =>
        new("Ru Write Single Translation",
            Scenario: new RuWriteSingleTranslationScenario(service),
            Frequency: Mid,
            ExpectedScore: Advanced,
            PassScore: 2.0,
            FailScore: 0.7
        );

    /// <summary>
    /// Воросы, которые задаются в первый раз для слова.
    /// Тут только тапательные вопросы
    /// </summary>
    public static readonly Question[] FirstQuestions =
    {
        IsItRightTranslationExam,
        EngTrust,
        RuTrust
    };

    public static readonly Question[] BeginnerQuestions =
    {
        EngChoose,
        RuChoose,
        RuExampleChoose,
        EngExampleChoose,
        EngChooseWordInExample,
        IsItRightTranslationExam,
        EngChooseMultipleTranslationExam,
        PhraseEngChoose,
        PhraseRuChoose,
        RuEasyWriteMissingLetter,
        EngEasyWriteMissingLetter
    };

    public static readonly Question[] IntermediateQuestions =
    {
        EngEasyWriteMissingLetter,
        RuEasyWriteMissingLetter,
        EngHardWriteMissingLetter,
        RuHardWriteMissingLetter,
        EngChoose,
        RuChoose,
        RuExampleChoose,
        EngExampleChoose,
        ExampleEngSubstitute,
        ExampleRuSubstitute,
        ExampleAssembleExam,
        ExampleAssembleWithClueExam,
        PhraseAssembleExam,
        PhraseAssembleWithClueExam,
        EngTrust,
        RuTrust,
        RuTrustSingle,
        EngChooseByTranscriptionExam,
        RuChooseByTranscriptionExam,
        TranscriptionExam,
        IsItRightTranslationExam,
        EngChooseMultipleTranslationExam,
        PhraseEngChoose,
        PhraseRuChoose,
    };

    public static Question[] AdvancedQuestions(LocalDictionaryService localDictionaryService) => new[]
    {
        EngHardWriteMissingLetter,
        RuHardWriteMissingLetter,
        EngWrite(localDictionaryService),
        RuWrite(localDictionaryService),
        RuWriteSingleTranslation(localDictionaryService),
        ExampleAssembleExam,
        ExampleAssembleWithClueExam,
        PhraseAssembleExam,
        PhraseAssembleWithClueExam,
        EngChoose,
        RuChoose,
        EngExampleChoose,
        RuExampleChoose,
        EngTrust,
        RuTrust,
        RuTrustSingle,
        ClearExampleEngSubstitute,
        ClearExampleRuSubstitute,
        ExampleEngSubstitute,
        ExampleRuSubstitute,
        EngChooseWordInExample,
        ClearEngChooseWordInExample,
        IsItRightTranslationExam,
        EngChooseMultipleTranslationExam,
        RuChooseByTranscriptionExam,
        PhraseEngChoose,
        PhraseRuChoose,
    };
}