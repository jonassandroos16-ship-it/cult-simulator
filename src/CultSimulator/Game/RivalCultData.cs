using System.Collections.Immutable;

namespace CultSimulator.Game;

public static class RivalCultData
{
    public static readonly ImmutableArray<RivalCultDef> Rivals = ImmutableArray.Create(
        new RivalCultDef(
            "order_of_dawn",
            "The Order of the Dawn",
            "☀️",
            RivalCultArchetype.TheOrderOfTheDawn,
            "A zealot order that hunts what they call 'abominations.' They move fast and strike hard but lack subtlety.",
            "europe",
            0.08, 1.2, 0.7),
        new RivalCultDef(
            "crimson_conclave",
            "The Crimson Conclave",
            "🩸",
            RivalCultArchetype.TheCrimsonConclave,
            "Blood-rites and old pacts. They are patient, methodical, and hard to dislodge once entrenched.",
            "south_america",
            0.05, 1.5, 0.4),
        new RivalCultDef(
            "silent_choir",
            "The Silent Choir",
            "🤫",
            RivalCultArchetype.TheSilentChoir,
            "Whispers in the halls of power. They prefer infiltration to confrontation and grow quietly.",
            "asia",
            0.06, 1.0, 0.3),
        new RivalCultDef(
            "obsidian_circle",
            "The Obsidian Circle",
            "🜲",
            RivalCultArchetype.TheObsidianCircle,
            "An ancient cabal of sorcerer-aristocrats. Slow to act but devastating when they commit.",
            "africa",
            0.04, 1.8, 0.5),
        new RivalCultDef(
            "silver_legion",
            "The Silver Legion",
            "🛡️",
            RivalCultArchetype.TheSilentChoir,
            "A modern occult syndicate that controls media narratives and government puppets. Adaptable and well-funded.",
            "north_america",
            0.07, 1.1, 0.6),
        new RivalCultDef(
            "deep_current",
            "The Deep Current",
            "🌊",
            RivalCultArchetype.TheCrimsonConclave,
            "Isolationist sorcerers of the southern seas. Few in number but their distance makes reinforcement slow.",
            "oceania",
            0.03, 1.3, 0.3),
        new RivalCultDef(
            "iron_caliphate",
            "The Iron Caliphate",
            "🕌",
            RivalCultArchetype.TheOrderOfTheDawn,
            "Crossroads faction blending ancient mysticism with political ambition. Multiple cells compete for dominance.",
            "middle_east",
            0.06, 1.4, 0.5)
    );

    public static RivalCultDef? Find(string id) =>
        Rivals.FirstOrDefault(r => r.Id == id);
}
