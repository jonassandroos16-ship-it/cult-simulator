using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public enum BuildingType { Shrine, Cathedral, Monolith, Treasury }
public enum UpgradeId { Hymnal, Relics, Visions, Ascendance }
public enum ResourceKind { Faith, Gold }

public record BuildingDef(
    BuildingType Type,
    string Name,
    string Icon,
    int BaseCost,
    ResourceKind CostResource,
    double Growth,
    string EffectDescription);

public record UpgradeDef(
    UpgradeId Id,
    string Name,
    string Icon,
    int FaithCost,
    int GoldCost,
    string EffectDescription,
    int UnlockFollowers);

public record RankDef(string Name, int MinFollowers, string Color);

public record EventChoice(string Label, string Description, Action<GameState> Apply);

public record EventDef(
    string Id,
    string Title,
    string Narrative,
    EventChoice ChoiceA,
    EventChoice ChoiceB);

public static class CultGame
{
    public const int RecruitCost = 10;
    public const string SaveKey = "cult_simulator_save_v1";

    public const double FollowerFaithPerSec = 0.2;
    public const double FollowerGoldPerSec = 0.1;
    public const double ShrineFaithPerSec = 1.0;
    public const double CathedralGoldPerSec = 0.6;
    public const double MonolithFaithBonus = 0.10;
    public const double TreasuryGoldBonus = 0.10;

    public const int EventIntervalSeconds = 30;
    public const double EventTriggerChance = 0.5;
    public const int EventMinFollowers = 5;

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

    public static double PreachMultiplier(GameState s)
    {
        double mult = 1.0 + s.Followers * 0.01;
        if (s.HasUpgrade(UpgradeId.Hymnal)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        return mult;
    }

    public static double FaithMultiplier(GameState s)
    {
        double mult = 1.0 + s.Buildings.GetValueOrDefault(BuildingType.Monolith) * MonolithFaithBonus;
        if (s.HasUpgrade(UpgradeId.Visions)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        return mult;
    }

    public static double GoldMultiplier(GameState s)
    {
        double mult = 1.0 + s.Buildings.GetValueOrDefault(BuildingType.Treasury) * TreasuryGoldBonus;
        if (s.HasUpgrade(UpgradeId.Relics)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        return mult;
    }

    public static int BuildingCost(BuildingDef def, int owned) =>
        (int)Math.Ceiling(def.BaseCost * Math.Pow(def.Growth, owned));

    public static bool CanAfford(GameState s, int faithCost, int goldCost) =>
        s.Faith >= faithCost && s.Gold >= goldCost;

    public static bool CanRecruit(GameState s) => s.Faith >= RecruitCost;

    public static bool UpgradeUnlocked(GameState s, UpgradeDef def) =>
        s.Followers >= def.UnlockFollowers;

    public static bool CanBuyUpgrade(GameState s, UpgradeDef def) =>
        !s.HasUpgrade(def.Id) && UpgradeUnlocked(s, def) && CanAfford(s, def.FaithCost, def.GoldCost);

    public static (double faith, double gold) TickIncome(GameState s)
    {
        double faith = s.Followers * FollowerFaithPerSec;
        double gold = s.Followers * FollowerGoldPerSec;
        faith += s.Buildings.GetValueOrDefault(BuildingType.Shrine) * ShrineFaithPerSec;
        gold += s.Buildings.GetValueOrDefault(BuildingType.Cathedral) * CathedralGoldPerSec;
        faith *= FaithMultiplier(s);
        gold *= GoldMultiplier(s);
        return (faith, gold);
    }

    public static RankDef RankFor(int followers)
    {
        RankDef? current = null;
        foreach (var r in Ranks)
            if (followers >= r.MinFollowers) current = r;
        return current!;
    }

    public static RankDef? NextRank(int followers)
    {
        foreach (var r in Ranks)
            if (r.MinFollowers > followers) return r;
        return null;
    }

    public static double RankProgress(GameState s)
    {
        var current = RankFor(s.Followers);
        var next = NextRank(s.Followers);
        if (next == null) return 1.0;
        return (double)(s.Followers - current.MinFollowers) / (next.MinFollowers - current.MinFollowers);
    }

    public static double Preach(GameState s)
    {
        s.PreachCount++;
        var gained = PreachMultiplier(s);
        s.Faith += gained;
        return gained;
    }

    public static void Recruit(GameState s)
    {
        if (!CanRecruit(s)) return;
        s.Faith -= RecruitCost;
        s.Followers++;
    }

    public static void BuyBuilding(GameState s, BuildingType type)
    {
        var def = Buildings.First(b => b.Type == type);
        int owned = s.Buildings.GetValueOrDefault(type);
        int cost = BuildingCost(def, owned);
        if (def.CostResource == ResourceKind.Faith) { if (s.Faith < cost) return; s.Faith -= cost; }
        else { if (s.Gold < cost) return; s.Gold -= cost; }
        s.Buildings[type] = owned + 1;
    }

    public static void BuyUpgrade(GameState s, UpgradeId id)
    {
        var def = Upgrades.First(u => u.Id == id);
        if (!CanBuyUpgrade(s, def)) return;
        s.Faith -= def.FaithCost;
        s.Gold -= def.GoldCost;
        s.Upgrades.Add(id);
    }

    public static GameState InitialState() => new()
    {
        CultName = "",
        Followers = 0,
        Faith = 0,
        Gold = 0,
        PreachCount = 0,
        Buildings = new Dictionary<BuildingType, int>(),
        Upgrades = new List<UpgradeId>(),
        StartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    };

    public static GameState LoadGame(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return InitialState();
        try
        {
            var state = JsonSerializer.Deserialize<GameState>(json, JsonOptions);
            if (state == null) return InitialState();
            state.Buildings ??= new Dictionary<BuildingType, int>();
            state.Upgrades ??= new List<UpgradeId>();
            return state;
        }
        catch { return InitialState(); }
    }

    public static string SaveGame(GameState s) =>
        JsonSerializer.Serialize(s, JsonOptions);

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false
    };

    public static string Fmt(double value)
    {
        if (value < 0) return "-" + Fmt(-value);
        if (value < 10) return value.ToString("F1");
        if (value < 1000) return Math.Floor(value).ToString("F0");
        if (value < 1_000_000) return (value / 1000).ToString("F2") + "K";
        if (value < 1_000_000_000) return (value / 1_000_000).ToString("F2") + "M";
        return (value / 1_000_000_000).ToString("F2") + "B";
    }
}
