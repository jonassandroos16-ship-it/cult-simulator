using System.Collections.Immutable;

namespace CultSimulator.Game;

/// <summary>
/// Static definitions for buildings, upgrades, ranks, and random events.
/// All gameplay content lives here so the engine and UI stay data-driven.
/// </summary>
public static class GameData
{
    public static readonly ImmutableArray<BuildingDef> Buildings = ImmutableArray.Create(
        new BuildingDef(BuildingType.Shrine, "Shrine", "🕯️", 40, ResourceKind.Faith, 1.15, "+1 Faith/s"),
        new BuildingDef(BuildingType.Cathedral, "Cathedral", "⛪", 80, ResourceKind.Gold, 1.18, "+0.6 Gold/s"),
        new BuildingDef(BuildingType.Monolith, "Monolith", "🗿", 300, ResourceKind.Faith, 1.20, "+10% Faith generation"),
        new BuildingDef(BuildingType.Treasury, "Treasury", "💰", 500, ResourceKind.Gold, 1.20, "+10% Gold generation"));

    public static readonly ImmutableArray<UpgradeDef> Upgrades = ImmutableArray.Create(
        new UpgradeDef(UpgradeId.Hymnal, "Sacred Hymnal", "📜", 120, 0, "Preaching yields 2× Faith", 0),
        new UpgradeDef(UpgradeId.Relics, "Golden Relics", "🏺", 0, 250, "Followers give 2× Gold", 15),
        new UpgradeDef(UpgradeId.Visions, "Prophetic Visions", "🔮", 600, 0, "Followers give 2× Faith", 40),
        new UpgradeDef(UpgradeId.Ascendance, "Rite of Ascendance", "🌟", 1500, 1000, "All production ×1.5", 120));

    public static readonly ImmutableArray<RankDef> Ranks = ImmutableArray.Create(
        new RankDef("Novice", 0, "#94a3b8"),
        new RankDef("Adept", 25, "#7dd3fc"),
        new RankDef("Mystic", 100, "#c4b5fd"),
        new RankDef("Prophet", 250, "#fbbf24"),
        new RankDef("Demigod", 600, "#fb7185"),
        new RankDef("Ascended", 1500, "#f472b6"));

    public static readonly ImmutableArray<EventDef> Events = ImmutableArray.Create(
        new EventDef("lost_wanderer", "A Lost Wanderer",
            "A gaunt figure stumbles into your circle, eyes wide with desperation. \"I have walked for forty days. I seek only purpose.\"",
            new EventChoice("Welcome them into the fold", "+3 Followers, −20 Faith", s => { s.Followers += 3; s.Faith -= 20; }),
            new EventChoice("Take their meager coin", "+50 Gold", s => { s.Gold += 50; })),
        new EventDef("wealthy_patron", "A Wealthy Patron",
            "A noble in silk robes arrives with a heavy chest. \"I am drawn to your teachings. Perhaps we can... help each other.\"",
            new EventChoice("Accept their donation", "+120 Gold", s => { s.Gold += 120; }),
            new EventChoice("Convert them to the faith", "+5 Followers, −40 Gold", s => { s.Followers += 5; s.Gold -= 40; })),
        new EventDef("voice_of_doubt", "A Voice of Doubt",
            "A former priest stands at the edge of your gathering, voice trembling with conviction. \"Your doctrine is hollow. I challenge you to debate.\"",
            new EventChoice("Debate publicly", "+100 Faith if you win, −5 Followers if you lose", s => { s.Faith += 100; s.Followers -= 5; }),
            new EventChoice("Perform a miracle", "+60 Faith, −30 Gold", s => { s.Faith += 60; s.Gold -= 30; })),
        new EventDef("rival_cult", "A Rival Cult",
            "Word reaches you of a competing order gaining followers nearby. Their leader mocks your teachings from the market square.",
            new EventChoice("Outshine their ritual", "+150 Faith, −50 Gold", s => { s.Faith += 150; s.Gold -= 50; }),
            new EventChoice("Ignore the distraction", "+4 Followers (quiet growth)", s => { s.Followers += 4; })),
        new EventDef("blood_moon", "A Blood Moon Rises",
            "The sky bleeds crimson. Your followers whisper that a great ritual is possible beneath the cursed moon.",
            new EventChoice("Perform the blood ritual", "+8 Followers, −100 Faith", s => { s.Followers += 8; s.Faith -= 100; }),
            new EventChoice("Prophesy the omen", "+200 Faith", s => { s.Faith += 200; })));
}
