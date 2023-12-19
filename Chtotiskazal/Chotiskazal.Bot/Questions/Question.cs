namespace Chotiskazal.Bot.Questions;

public record Question(
    string Name,
    IQuestionLogic Scenario,
    double ExpectedScore,
    int Frequency,
    double FailScore,
    double PassScore
) {
    public bool NeedClearScreen { get; private init; }

    public QuestionInputType InputType => Scenario.InputType;

    public Question Clear() =>
        new(
            Name: "Clean " + Name,
            Scenario: new ClearScreenLogicDecorator(Scenario),
            ExpectedScore: ExpectedScore,
            Frequency: Frequency,
            FailScore: FailScore,
            PassScore: PassScore)
        {
            NeedClearScreen = true
        };
}