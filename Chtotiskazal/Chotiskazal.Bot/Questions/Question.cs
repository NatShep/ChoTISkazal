namespace Chotiskazal.Bot.Questions;

/// <summary>
/// 
/// </summary>
/// <param name="Name">Unique quesion name</param>
/// <param name="Frequency">Relative frequency score</param>
public record Question(
    string Name,
    IQuestionScenario Scenario,
    double ExpectedScore,
    int Frequency,
    double FailScore,
    double PassScore
) {
    public bool NeedClearScreen { get; private init; }

    public QuestionInputType InputType => Scenario.InputType;
    public ScenarioWordTypeFit Fit => Scenario.Fit;

    public Question Clear() =>
        new(
            Name: "Clean " + Name,
            Scenario: new ClearScreenScenarioDecorator(Scenario),
            ExpectedScore: ExpectedScore,
            Frequency: Frequency,
            FailScore: FailScore,
            PassScore: PassScore)
        {
            NeedClearScreen = true
        };

    public Question WithFrequency(int frequency) =>
        new(Name, Scenario, ExpectedScore, frequency, FailScore, PassScore);
}