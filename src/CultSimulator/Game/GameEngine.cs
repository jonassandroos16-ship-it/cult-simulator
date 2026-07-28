namespace CultSimulator.Game;

public static class GameEngine
{
    public static double PreachMultiplier(GameState s)
    {
        double mult = 1.0 + s.Followers * 0.01;
        if (s.HasUpgrade(UpgradeId.Hymnal)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        return mult;
    }

    public static double FaithMultiplier(GameState s)
    {
        double mult = 1.0 + s.Buildings.GetValueOrDefault(BuildingType.Monolith) * GameBalance.MonolithFaithBonus;
        if (s.HasUpgrade(UpgradeId.Visions)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        return mult;
    }

    public static double GoldMultiplier(GameState s)
    {
        double mult = 1.0 + s.Buildings.GetValueOrDefault(BuildingType.Treasury) * GameBalance.TreasuryGoldBonus;
        if (s.HasUpgrade(UpgradeId.Relics)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        return mult;
    }

    public static int BuildingCost(BuildingDef def, int owned) =>
        (int)Math.Ceiling(def.BaseCost * Math.Pow(def.Growth, owned));

    public static bool CanAfford(GameState s, int faithCost, int goldCost) =>
        s.Faith >= faithCost && s.Gold >= goldCost;

    public static bool CanRecruit(GameState s) => s.Faith >= GameBalance.RecruitCost;

    public static bool UpgradeUnlocked(GameState s, UpgradeDef def) =>
        s.Followers >= def.UnlockFollowers;

    public static bool CanBuyUpgrade(GameState s, UpgradeDef def) =>
        !s.HasUpgrade(def.Id) && UpgradeUnlocked(s, def) && CanAfford(s, def.FaithCost, def.GoldCost);

    public static (double faith, double gold) TickIncome(GameState s)
    {
        double faith = s.Followers * GameBalance.FollowerFaithPerSec;
        double gold = s.Followers * GameBalance.FollowerGoldPerSec;
        faith += s.Buildings.GetValueOrDefault(BuildingType.Shrine) * GameBalance.ShrineFaithPerSec;
        gold += s.Buildings.GetValueOrDefault(BuildingType.Cathedral) * GameBalance.CathedralGoldPerSec;
        faith *= FaithMultiplier(s);
        gold *= GoldMultiplier(s);
        return (faith, gold);
    }

    public static RankDef RankFor(int followers)
    {
        RankDef? current = null;
        foreach (var r in GameData.Ranks)
            if (followers >= r.MinFollowers) current = r;
        return current!;
    }

    public static RankDef? NextRank(int followers)
    {
        foreach (var r in GameData.Ranks)
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
        s.Faith -= GameBalance.RecruitCost;
        s.Followers++;
    }

    public static void BuyBuilding(GameState s, BuildingType type)
    {
        var def = GameData.Buildings.First(b => b.Type == type);
        int owned = s.Buildings.GetValueOrDefault(type);
        int cost = BuildingCost(def, owned);
        if (def.CostResource == ResourceKind.Faith) { if (s.Faith < cost) return; s.Faith -= cost; }
        else { if (s.Gold < cost) return; s.Gold -= cost; }
        s.Buildings[type] = owned + 1;
    }

    public static void BuyUpgrade(GameState s, UpgradeId id)
    {
        var def = GameData.Upgrades.First(u => u.Id == id);
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
}
