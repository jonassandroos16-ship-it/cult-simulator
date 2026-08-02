namespace CultSimulator.Game;

public enum AgentType
{
    Initiate,
    Zealot,
    Infiltrator,
    Scholar,
    Mage
}

public enum BattleStatus
{
    NotStarted,
    Active,
    Victory,
    Defeat
}

public enum BattlePhase
{
    /// <summary>No rival cult in this continent yet.</summary>
    NoThreat,
    /// <summary>Rival cult present — player can deploy agents.</summary>
    Deploy,
    /// <summary>Battle in progress — agents fight automatically each tick.</summary>
    Fighting,
    /// <summary>Battle won — rival pushed back, cooldown before next.</summary>
    Cooldown
}
