namespace CultSimulator.Game;

public class ConversionState
{
    public string CovenId { get; set; } = "";
    public int CurrentStep { get; set; }
    public double Progress { get; set; }
    public bool Completed { get; set; }
    public string? LastOutcome { get; set; }
    public bool BattlePhase { get; set; }
    public bool BattleWon { get; set; }
}
