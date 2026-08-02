using System.Collections.Immutable;

namespace CultSimulator.Game;

public static class BattleData
{
    public static readonly ImmutableArray<AgentTypeDef> AgentTypes = ImmutableArray.Create(
        new AgentTypeDef(AgentType.Initiate, "Initiate", "🧙",
            "Basic agent. Cheap and expendable. Low attack but decent defense.",
            Attack: 2.0, Defense: 3.0, Stealth: 1.0, AgentCost: 5),
        new AgentTypeDef(AgentType.Zealot, "Zealot", "⚔️",
            "Fanatical warrior. High attack, low stealth. Recruit Zealots in the Sanctum first.",
            Attack: 6.0, Defense: 4.0, Stealth: 0.3, AgentCost: 20),
        new AgentTypeDef(AgentType.Infiltrator, "Infiltrator", "🗡️",
            "Stealth operative. Low attack but high stealth — reduces rival counterattack damage. Recruit Infiltrators in the Sanctum first.",
            Attack: 3.0, Defense: 2.0, Stealth: 3.0, AgentCost: 15)
    );

    public static AgentTypeDef? AgentDef(AgentType type) =>
        AgentTypes.FirstOrDefault(a => a.Type == type);

    public static readonly ImmutableArray<BattleTheaterDef> Theaters = ImmutableArray.Create(
        new BattleTheaterDef("europe", "Europe", "🏰",
            "The Order of the Dawn hunts abominations. They strike fast and hard."),
        new BattleTheaterDef("north_america", "North America", "🗽",
            "Rival cults vie for control of media and government institutions."),
        new BattleTheaterDef("south_america", "South America", "🌴",
            "The Crimson Conclave spreads through blood-rites and old pacts."),
        new BattleTheaterDef("asia", "Asia", "🏯",
            "The Silent Choir whispers in the halls of power."),
        new BattleTheaterDef("oceania", "Oceania", "🦘",
            "Distant shores — fewer rivals but isolation makes reinforcement slow."),
        new BattleTheaterDef("africa", "Africa", "🌍",
            "The Obsidian Circle — ancient sorcerer-aristocrats, slow but devastating."),
        new BattleTheaterDef("middle_east", "Middle East", "🕌",
            "Crossroads of empires. Multiple factions compete for influence.")
    );

    public static BattleTheaterDef? Theater(string continentId) =>
        Theaters.FirstOrDefault(t => t.ContinentId == continentId);

    public static RivalCultDef? RivalForContinent(string continentId) =>
        RivalCultData.Rivals.FirstOrDefault(r => r.PreferredTerritoryId == continentId);
}
