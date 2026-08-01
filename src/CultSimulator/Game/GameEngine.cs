using System.Collections.Immutable;

namespace CultSimulator.Game;

public static class GameEngine
{
    public static double CovenBaseMultiplier(GameState state, string covenId)
    {
        var coven = state.FindCoven(covenId);
        if (coven != null && coven.BaseMultiplier > 0)
            return coven.BaseMultiplier;
        return 1.0;
    }

    public static double PreachMultiplier(CovenState s)
    {
        double mult = 1.0 + s.Followers * GameBalance.PreachFollowerScaling;
        if (s.HasUpgrade(UpgradeId.Hymnal)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        mult *= s.BaseMultiplier > 0 ? s.BaseMultiplier : 1.0;
        return mult;
    }

    public static double PreachMultiplier(GameState s)
    {
        double mult = PreachMultiplier(s.ActiveCoven);
        mult += s.Occult.SermonPowerLevel;
        mult *= GrandSacrifice.GlobalProductionMult(s);
        if (s.Occult.IsFrenzyActive) mult *= OccultBalance.FrenzyMultiplier;
        return mult;
    }

    public static double FaithMultiplier(CovenState s)
    {
        double mult = 1.0 + s.Buildings.GetValueOrDefault(BuildingType.Monolith) * GameBalance.MonolithFaithBonus;
        mult += s.Buildings.GetValueOrDefault(BuildingType.Observatory) * GameBalance.ObservatoryFaithBonus;
        if (s.HasUpgrade(UpgradeId.Visions)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        return mult;
    }

    public static double GoldMultiplier(CovenState s)
    {
        double mult = 1.0 + s.Buildings.GetValueOrDefault(BuildingType.Treasury) * GameBalance.TreasuryGoldBonus;
        mult += s.Buildings.GetValueOrDefault(BuildingType.Reliquary) * GameBalance.ReliquaryGoldBonus;
        if (s.HasUpgrade(UpgradeId.Relics)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        return mult;
    }

    public static int BuildingCost(BuildingDef def, int owned) => (int)Math.Ceiling(def.BaseCost * Math.Pow(def.Growth, owned));
    public static int BankBuildingCost(int owned) => (int)Math.Ceiling(GameBalance.BankBaseCost * Math.Pow(GameBalance.BankCostGrowth, owned));
    public static bool CanAfford(CovenState s, int faithCost, int goldCost) => s.Faith >= faithCost && s.Gold >= goldCost;
    public static bool CanAffordUpgrade(CovenState s, UpgradeDef def, GameState state) => s.Faith >= def.FaithCost && s.Gold >= def.GoldCost && state.ShadowWarOrInit.AvailableAgents >= def.AgentCost;

    public static int RecruitCostFor(CovenState s) => s.Followers == 0 ? GameBalance.RecruitBaseCost : (int)Math.Ceiling(GameBalance.RecruitBaseCost * Math.Pow(GameBalance.RecruitCostGrowth, s.Followers));
    public static bool CanRecruit(CovenState s) => s.Faith >= RecruitCostFor(s);
    public static bool UpgradeUnlocked(CovenState s, UpgradeDef def) => s.Followers >= def.UnlockFollowers;
    public static bool CanBuyUpgrade(CovenState s, UpgradeDef def, GameState state) => !s.HasUpgrade(def.Id) && UpgradeUnlocked(s, def) && CanAffordUpgrade(s, def, state);

    public static (double faith, double gold) TickIncome(CovenState s)
    {
        double faith = s.Followers * GameBalance.FollowerFaithPerSec;
        double gold = s.Followers * GameBalance.FollowerGoldPerSec;
        faith += s.Buildings.GetValueOrDefault(BuildingType.Shrine) * GameBalance.ShrineFaithPerSec;
        gold += s.Buildings.GetValueOrDefault(BuildingType.Cathedral) * GameBalance.CathedralGoldPerSec;
        faith *= FaithMultiplier(s);
        gold *= GoldMultiplier(s);
        faith *= s.BaseMultiplier > 0 ? s.BaseMultiplier : 1.0;
        return (faith, gold);
    }

    public static (double faith, double gold) TotalTickIncome(GameState state)
    {
        double faith = 0, gold = 0;
        foreach (var coven in state.Covens) { if (!coven.TakenOver) continue; var (f, g) = TickIncome(coven); faith += f; gold += g; }
        return (faith, gold);
    }

    public static (double faith, double gold) TotalIncomePerSec(GameState state)
    {
        var (faith, gold) = TotalTickIncome(state);
        faith += OccultEngine.TotalFaithPerSec(state) + OccultEngine.TotalMapFaithPerSec(state);
        if (state.ShadowWar != null)
            faith *= 1.0 + ShadowWarEngine.FaithMultiplierBonus(state.ShadowWar);
        return (faith, gold);
    }

    public static double IdleCapSeconds(CovenState s)
    {
        int bankLevel = s.Buildings.GetValueOrDefault(BuildingType.Bank);
        if (bankLevel == 0) return GameBalance.BankCapHours[0] * 3600.0;
        int tier = Math.Min(bankLevel, GameBalance.BankCapHours.Length - 1);
        double mult = 1.0;
        if (s.HasUpgrade(UpgradeId.BankVault)) mult *= 1.5;
        if (s.HasUpgrade(UpgradeId.OffshoreAccounts)) mult *= 1.5;
        if (s.HasUpgrade(UpgradeId.DarkLedger)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.SoulEndowment)) mult *= 2.0;
        return GameBalance.BankCapHours[tier] * 3600.0 * mult;
    }

    public static string IdleCapDisplay(CovenState s)
    {
        double hours = IdleCapSeconds(s) / 3600.0;
        if (hours < 1.0) return $"{Math.Floor(hours * 60)} min";
        if (hours < 24.0) return $"{hours:F1} h";
        return $"{hours / 24.0:F1} d";
    }

    public static void TickAllCovens(GameState state, WorldLocationService locations)
    {
        SyncBaseMultipliers(state, locations);
        foreach (var coven in state.Covens)
        {
            if (!coven.TakenOver) continue;
            var (faith, gold) = TickIncome(coven);
            coven.Faith += faith; coven.Gold += gold;
        }
        OccultEngine.Tick(state, 1.0);
        ShadowWarEngine.Tick(state.ShadowWarOrInit, state, locations, 1.0);
        RivalCultEngine.Tick(state, locations, 1.0);
        BattleEngine.Tick(state, locations, 1.0);
    }

    /// <summary>
    /// Copies BaseMultiplier from each coven's WorldLocationDef into its
    /// CovenState. Called during ticks so newly-converted covens pick up
    /// their multiplier automatically. Safe to call every tick — it's a
    /// simple field copy that only changes when a coven is first converted.
    /// </summary>
    public static void SyncBaseMultipliers(GameState state, WorldLocationService locations)
    {
        foreach (var coven in state.Covens)
        {
            if (!coven.Converted) continue;
            var loc = locations.Find(coven.Id);
            if (loc != null)
                coven.BaseMultiplier = loc.BaseMultiplier;
        }
    }

    public static (double faith, double gold, double lostFaith, double lostGold) ApplyOfflineIncome(GameState state, long elapsedMs)
    {
        double elapsedSec = elapsedMs / 1000.0;
        if (elapsedSec <= 0) return (0, 0, 0, 0);
        double totalFaith = 0, totalGold = 0, totalLostFaith = 0, totalLostGold = 0;

        foreach (var coven in state.Covens)
        {
            if (!coven.TakenOver) continue;
            var (fps, gps) = TickIncome(coven);
            double cap = IdleCapSeconds(coven);
            double eff = Math.Min(elapsedSec, cap);
            double f = fps * eff; double g = gps * eff;
            coven.Faith += f; coven.Gold += g;
            totalFaith += f; totalGold += g;

            double occultFps = OccultEngine.TotalFaithPerSecForCoven(state, coven) + OccultEngine.TotalMapFaithPerSecForCoven(coven.Occult, state);
            double occultFaith = occultFps * eff;
            coven.Faith += occultFaith;
            coven.Occult.LifetimeFaith += occultFaith;
            totalFaith += occultFaith;

            if (elapsedSec > cap)
            {
                double lostSec = elapsedSec - cap;
                totalLostFaith += (fps + occultFps) * lostSec;
                totalLostGold += gps * lostSec;
            }
        }

        return (totalFaith, totalGold, totalLostFaith, totalLostGold);
    }

    public static RankDef RankFor(int followers) { RankDef? current = null; foreach (var r in GameData.Ranks) if (followers >= r.MinFollowers) current = r; return current!; }
    public static RankDef? NextRank(int followers) { foreach (var r in GameData.Ranks) if (r.MinFollowers > followers) return r; return null; }
    public static double RankProgress(CovenState s) { var c = RankFor(s.Followers); var n = NextRank(s.Followers); if (n == null) return 1.0; double range = n.MinFollowers - c.MinFollowers; if (range <= 0) return 1.0; return Math.Clamp((double)(s.Followers - c.MinFollowers) / range, 0.0, 1.0); }

    public static RankDef RankFor(GameState s) => RankFor(CovenProgress.TotalFollowers(s));
    public static RankDef? NextRank(GameState s) => NextRank(CovenProgress.TotalFollowers(s));
    public static double RankProgress(GameState s) { int total = CovenProgress.TotalFollowers(s); var c = RankFor(total); var n = NextRank(total); if (n == null) return 1.0; double range = n.MinFollowers - c.MinFollowers; if (range <= 0) return 1.0; return Math.Clamp((double)(total - c.MinFollowers) / range, 0.0, 1.0); }

    public static double Preach(CovenState s) { s.PreachCount++; var gained = PreachMultiplier(s); s.Faith += gained; return gained; }
    public static void Recruit(CovenState s) { if (!CanRecruit(s)) return; s.Faith -= RecruitCostFor(s); s.Followers++; }
    public static int RecruitMultiple(CovenState s, int max)
    {
        if (max == 0) return 0;
        int recruited = 0;
        int limit = max < 0 ? int.MaxValue : max;
        while (recruited < limit && CanRecruit(s))
        {
            s.Faith -= RecruitCostFor(s);
            s.Followers++;
            recruited++;
        }
        return recruited;
    }
    public static void BuyBuilding(CovenState s, BuildingType type) { var def = GameData.Buildings.First(b => b.Type == type); int owned = s.Buildings.GetValueOrDefault(type); int cost = BuildingCost(def, owned); if (def.CostResource == ResourceKind.Faith) { if (s.Faith < cost) return; s.Faith -= cost; } else { if (s.Gold < cost) return; s.Gold -= cost; } s.Buildings[type] = owned + 1; }
    public static void BuyBank(CovenState s) { int owned = s.Buildings.GetValueOrDefault(BuildingType.Bank); int cost = BankBuildingCost(owned); if (s.Gold < cost) return; s.Gold -= cost; s.Buildings[BuildingType.Bank] = owned + 1; }
    public static void BuyUpgrade(CovenState s, UpgradeId id, GameState state) { var def = GameData.Upgrades.First(u => u.Id == id); if (!CanBuyUpgrade(s, def, state)) return; s.Faith -= def.FaithCost; s.Gold -= def.GoldCost; if (def.AgentCost > 0) state.ShadowWarOrInit.TotalAgents -= def.AgentCost; s.Upgrades.Add(id); }

    public static GameState InitialState() => new() { CultName = "", StoryShown = false, ActiveCovenId = "skanor", Covens = new List<CovenState> { new CovenState { Id = "skanor", TakenOver = true, BaseMultiplier = 1.0, Occult = new OccultState() } }, ShadowWar = ShadowWarEngine.CreateInitialState(), StartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), LastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };

    public static double FaithMultiplier(GameState s) => FaithMultiplier(s.ActiveCoven);
    public static double GoldMultiplier(GameState s) => GoldMultiplier(s.ActiveCoven);
    public static int RecruitCostFor(GameState s) => RecruitCostFor(s.ActiveCoven);
    public static bool CanRecruit(GameState s) => CanRecruit(s.ActiveCoven);
    public static bool UpgradeUnlocked(GameState s, UpgradeDef def) => UpgradeUnlocked(s.ActiveCoven, def);
    public static bool CanBuyUpgrade(GameState s, UpgradeDef def) => CanBuyUpgrade(s.ActiveCoven, def, s);
    public static bool CanAfford(GameState s, int faithCost, int goldCost) => CanAfford(s.ActiveCoven, faithCost, goldCost);
    public static (double faith, double gold) TickIncome(GameState s) => TickIncome(s.ActiveCoven);
    public static double Preach(GameState s) { s.ActiveCoven.PreachCount++; var gained = PreachMultiplier(s); s.ActiveCoven.Faith += gained; return gained; }
    public static void Recruit(GameState s) => Recruit(s.ActiveCoven);
    public static int RecruitMultiple(GameState s, int max) => RecruitMultiple(s.ActiveCoven, max);
    public static void BuyBuilding(GameState s, BuildingType type) => BuyBuilding(s.ActiveCoven, type);
    public static void BuyBank(GameState s) => BuyBank(s.ActiveCoven);
    public static void BuyUpgrade(GameState s, UpgradeId id) => BuyUpgrade(s.ActiveCoven, id, s);
}