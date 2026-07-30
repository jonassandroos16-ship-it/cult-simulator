using System.Collections.Immutable;

namespace CultSimulator.Game;

public static class GameData
{
    public static readonly ImmutableArray<BuildingDef> Buildings = ImmutableArray.Create(
        new BuildingDef(BuildingType.Shrine, "Shrine", "🕯️", 40, ResourceKind.Faith, 1.15, "+0.3 Faith/s"),
        new BuildingDef(BuildingType.Cathedral, "Cathedral", "⛪", 80, ResourceKind.Gold, 1.18, "+0.2 Gold/s"),
        new BuildingDef(BuildingType.Monolith, "Monolith", "🗿", 300, ResourceKind.Faith, 1.20, "+8% Faith generation"),
        new BuildingDef(BuildingType.Treasury, "Treasury", "💰", 500, ResourceKind.Gold, 1.20, "+8% Gold generation"),
        new BuildingDef(BuildingType.Bank, "Bank", "🏦", 200, ResourceKind.Gold, 1.25, "Increases idle income cap"),
        new BuildingDef(BuildingType.Observatory, "Observatory", "🔭", 800, ResourceKind.Gold, 1.22, "+6% Faith generation"),
        new BuildingDef(BuildingType.Reliquary, "Reliquary", "📿", 1500, ResourceKind.Gold, 1.24, "+6% Gold generation"),
        new BuildingDef(BuildingType.Undercroft, "Undercroft", "⚰️", 3000, ResourceKind.Gold, 1.28, "+2 Acolyte cap per level"));

    public static readonly ImmutableArray<UpgradeDef> Upgrades = ImmutableArray.Create(
        new UpgradeDef(UpgradeId.Hymnal, "Sacred Hymnal", "📜", 120, 0, "Preaching yields 2× Faith", 0),
        new UpgradeDef(UpgradeId.Relics, "Golden Relics", "🏺", 0, 250, "Followers give 2× Gold", 15),
        new UpgradeDef(UpgradeId.Visions, "Prophetic Visions", "🔮", 600, 0, "Followers give 2× Faith", 40),
        new UpgradeDef(UpgradeId.Ascendance, "Rite of Ascendance", "🌟", 1500, 1000, "All production ×1.5", 120),
        new UpgradeDef(UpgradeId.BankVault, "Reinforced Vault", "🔒", 0, 400, "Bank idle cap ×2", 10),
        new UpgradeDef(UpgradeId.OffshoreAccounts, "Offshore Accounts", "🏝️", 0, 1200, "Bank idle cap ×2", 30),
        new UpgradeDef(UpgradeId.DarkLedger, "Dark Ledger", "📓", 800, 600, "Bank idle cap ×1.5", 50),
        new UpgradeDef(UpgradeId.SoulEndowment, "Soul Endowment", "💀", 2000, 1500, "Bank idle cap ×1.5", 100));

    public static readonly ImmutableArray<UpgradeId> BankUpgrades = ImmutableArray.Create(
        UpgradeId.BankVault,
        UpgradeId.OffshoreAccounts,
        UpgradeId.DarkLedger,
        UpgradeId.SoulEndowment);

    public static readonly ImmutableArray<BuildingType> PreachBuildings = ImmutableArray.Create(
        BuildingType.Shrine,
        BuildingType.Cathedral,
        BuildingType.Monolith,
        BuildingType.Treasury,
        BuildingType.Observatory,
        BuildingType.Reliquary,
        BuildingType.Undercroft);

    public static readonly Dictionary<string, Dictionary<BuildingType, string>> CovenBuildingNames = new()
    {
        ["skanor"] = new()
        {
            { BuildingType.Shrine, "Seidr Altar" },
            { BuildingType.Cathedral, "Hof" },
            { BuildingType.Monolith, "Runestone" },
            { BuildingType.Treasury, "Silver Hoard" },
            { BuildingType.Observatory, "Star Watch" },
            { BuildingType.Reliquary, "Bone Reliquary" },
            { BuildingType.Undercroft, "Frost Crypt" }
        },
        ["la_recta_provincia"] = new()
        {
            { BuildingType.Shrine, "Capilla" },
            { BuildingType.Cathedral, "Catedral" },
            { BuildingType.Monolith, "Piedra de Sacrificio" },
            { BuildingType.Treasury, "Cofre del Santo Oficio" },
            { BuildingType.Observatory, "Observatorio Austral" },
            { BuildingType.Reliquary, "Relicario Patagón" },
            { BuildingType.Undercroft, "Bóveda de Chiloé" }
        },
        ["benandanti"] = new()
        {
            { BuildingType.Shrine, "Stria Altar" },
            { BuildingType.Cathedral, "Chiesa Rovinata" },
            { BuildingType.Monolith, "Pietra Lunare" },
            { BuildingType.Treasury, "Forziere delle Streghe" },
            { BuildingType.Observatory, "Osservatorio Lunare" },
            { BuildingType.Reliquary, "Reliquiario Friulano" },
            { BuildingType.Undercroft, "Cripta dei Benandanti" }
        },
        ["malkin_tower_coven"] = new()
        {
            { BuildingType.Shrine, "Witch's Altar" },
            { BuildingType.Cathedral, "Tower Keep" },
            { BuildingType.Monolith, "Standing Stone" },
            { BuildingType.Treasury, "Lancashire Chest" },
            { BuildingType.Observatory, "Tower Observatory" },
            { BuildingType.Reliquary, "Pendle Reliquary" },
            { BuildingType.Undercroft, "Malkin Cellar" }
        },
        ["north_berwick_coven"] = new()
        {
            { BuildingType.Shrine, "Kirk Altar" },
            { BuildingType.Cathedral, "Auld Kirk" },
            { BuildingType.Monolith, "Heid Stane" },
            { BuildingType.Treasury, "Scottish Coffer" },
            { BuildingType.Observatory, "North Berwick Watch" },
            { BuildingType.Reliquary, "Lothian Reliquary" },
            { BuildingType.Undercroft, "Berwick Vaults" }
        },
        ["la_cabotina"] = new()
        {
            { BuildingType.Shrine, "Autel Cabotin" },
            { BuildingType.Cathedral, "Chapelle Profane" },
            { BuildingType.Monolith, "Menhir Occulte" },
            { BuildingType.Treasury, "Coffre Marseillais" },
            { BuildingType.Observatory, "Observatoire Ligurien" },
            { BuildingType.Reliquary, "Reliquaire de Triora" },
            { BuildingType.Undercroft, "Caveau Cabotin" }
        },
        ["ixchel_priestesses"] = new()
        {
            { BuildingType.Shrine, "Isla Altar" },
            { BuildingType.Cathedral, "Piramide" },
            { BuildingType.Monolith, "Estela" },
            { BuildingType.Treasury, "Cofre Maya" },
            { BuildingType.Observatory, "Observatorio de Ixchel" },
            { BuildingType.Reliquary, "Reliquario Maya" },
            { BuildingType.Undercroft, "Cenote Sagrado" }
        },
        ["new_forest_coven"] = new()
        {
            { BuildingType.Shrine, "Forest Shrine" },
            { BuildingType.Cathedral, "Grove Cathedral" },
            { BuildingType.Monolith, "Omen Stone" },
            { BuildingType.Treasury, "Wessex Strongbox" },
            { BuildingType.Observatory, "New Forest Watch" },
            { BuildingType.Reliquary, "Hampshire Reliquary" },
            { BuildingType.Undercroft, "Barrow Crypt" }
        }
    };

    public static string BuildingNameFor(string covenId, BuildingType type)
    {
        if (CovenBuildingNames.TryGetValue(covenId, out var names) && names.TryGetValue(type, out var name))
            return name;
        return GameData.Buildings.First(b => b.Type == type).Name;
    }

    public static readonly ImmutableArray<RankDef> Ranks = ImmutableArray.Create(
        new RankDef("Novice", 0, "#94a3b8", "You light your first candle in the dark. The shadows listen, curious and patient."),
        new RankDef("Adept", 25, "#7dd3fc", "Whispers of your name travel on the wind. The faithful gather, hungry for truth."),
        new RankDef("Mystic", 100, "#c4b5fd", "Your voice bends the veil between worlds. What was hidden now answers your call."),
        new RankDef("Prophet", 250, "#fbbf24", "Nations tremble at your proclamations. You no longer speak for the faithful — the faithful speak for you."),
        new RankDef("Demigod", 600, "#fb7185", "Mortality frays at the edges of your being. The line between worship and reality blurs in your presence."),
        new RankDef("Ascended", 1500, "#f472b6", "You have crossed the threshold. Flesh, doubt, and death are memories. The world reshapes itself around your will."));

    public static readonly ImmutableArray<EventDef> Events = ImmutableArray.Create(
        new EventDef("lost_wanderer", "A Lost Wanderer",
            "A gaunt figure stumbles into your circle, eyes wide with desperation. \"I have walked for forty days. I seek only purpose.\"",
            new EventChoice("Welcome them into the fold", "+3 Followers, −20 Faith", s => { s.Followers += 3; s.Faith -= 20; return null; }),
            new EventChoice("Take their meager coin", "+50 Gold", s => { s.Gold += 50; return null; })),
        new EventDef("wealthy_patron", "A Wealthy Patron",
            "A noble in silk robes arrives with a heavy chest. \"I am drawn to your teachings. Perhaps we can... help each other.\"",
            new EventChoice("Accept their donation", "+120 Gold", s => { s.Gold += 120; return null; }),
            new EventChoice("Convert them to the faith", "+5 Followers, −40 Gold", s => { s.Followers += 5; s.Gold -= 40; return null; })),
        new EventDef("voice_of_doubt", "A Voice of Doubt",
            "A former priest stands at the edge of your gathering, voice trembling with conviction. \"Your doctrine is hollow. I challenge you to debate.\"",
            new EventChoice("Debate publicly", "+100 Faith if you win, −5 Followers if you lose", s =>
            {
                bool won = Random.Shared.NextDouble() < 0.5;
                if (won)
                {
                    s.Faith += 100;
                    return "You dismantled his arguments before the crowd. The faithful cheer, and the priest slinks away in silence. +100 Faith.";
                }
                s.Followers -= 5;
                return "His words cut deep. Doubt spreads through the crowd as five followers drift away into the night. −5 Followers.";
            }),
            new EventChoice("Perform a miracle", "+60 Faith, −30 Gold", s => { s.Faith += 60; s.Gold -= 30; return null; })),
        new EventDef("rival_cult", "A Rival Cult",
            "Word reaches you of a competing order gaining followers nearby. Their leader mocks your teachings from the market square.",
            new EventChoice("Outshine their ritual", "+150 Faith, −50 Gold", s => { s.Faith += 150; s.Gold -= 50; return null; }),
            new EventChoice("Ignore the distraction", "+4 Followers (quiet growth)", s => { s.Followers += 4; return null; })),
        new EventDef("blood_moon", "A Blood Moon Rises",
            "The sky bleeds crimson. Your followers whisper that a great ritual is possible beneath the cursed moon.",
            new EventChoice("Perform the blood ritual", "+8 Followers, −100 Faith", s => { s.Followers += 8; s.Faith -= 100; return null; }),
            new EventChoice("Prophesy the omen", "+200 Faith", s => { s.Faith += 200; return null; })));
}
