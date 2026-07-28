using CultSimulator.Game;

namespace CultSimulator.Tests;

public class CultGameTests
{
    private static GameState NewState() => GameEngine.InitialState();
    private static CovenState NewCoven() => new CovenState { Id = "skanor", TakenOver = true };

    [Fact]
    public void Preach_GeneratesFaith()
    {
        var s = NewCoven();
        GameEngine.Preach(s);
        Assert.True(s.Faith > 0);
        Assert.Equal(1, s.PreachCount);
    }

    [Fact]
    public void Preach_YieldScalesWithFollowers()
    {
        var s = NewCoven(); s.Followers = 100;
        GameEngine.Preach(s);
        Assert.Equal(2.0, s.Faith, precision: 2);
    }

    [Fact]
    public void Preach_HymnalDoublesYield()
    {
        var s = NewCoven(); s.Followers = 10;
        GameEngine.Preach(s); var without = s.Faith;
        s.Faith = 0; s.Upgrades.Add(UpgradeId.Hymnal);
        GameEngine.Preach(s);
        Assert.Equal(without * 2, s.Faith, precision: 2);
    }

    [Fact]
    public void Preach_AscendanceMultipliesYield()
    {
        var s = NewCoven(); s.Followers = 10; s.Upgrades.Add(UpgradeId.Ascendance);
        GameEngine.Preach(s);
        Assert.Equal(1.65, s.Faith, precision: 2);
    }

    [Fact]
    public void Recruit_ConvertsFaithToFollower()
    {
        var s = NewCoven(); s.Faith = 30;
        GameEngine.Recruit(s);
        Assert.Equal(1, s.Followers);
        Assert.Equal(20, s.Faith);
    }

    [Fact]
    public void Recruit_RequiresEnoughFaith()
    {
        var s = NewCoven(); s.Faith = 9;
        GameEngine.Recruit(s);
        Assert.Equal(0, s.Followers);
        Assert.Equal(9, s.Faith);
    }

    [Fact]
    public void BuildingCost_ScalesGeometrically()
    {
        var shrine = GameData.Buildings.First(b => b.Type == BuildingType.Shrine);
        Assert.Equal(40, GameEngine.BuildingCost(shrine, 0));
        Assert.Equal((int)Math.Ceiling(40 * 1.15), GameEngine.BuildingCost(shrine, 1));
        Assert.Equal((int)Math.Ceiling(40 * Math.Pow(1.15, 5)), GameEngine.BuildingCost(shrine, 5));
    }

    [Fact]
    public void BuyBuilding_SpendFaithAndIncrement()
    {
        var s = NewCoven(); s.Faith = 100;
        GameEngine.BuyBuilding(s, BuildingType.Shrine);
        Assert.Equal(60, s.Faith);
        Assert.Equal(1, s.Buildings[BuildingType.Shrine]);
    }

    [Fact]
    public void BuyBuilding_SpendGold()
    {
        var s = NewCoven(); s.Gold = 200;
        GameEngine.BuyBuilding(s, BuildingType.Cathedral);
        Assert.Equal(120, s.Gold);
        Assert.Equal(1, s.Buildings[BuildingType.Cathedral]);
    }

    [Fact]
    public void BuyBuilding_InsufficientFaithDoesNothing()
    {
        var s = NewCoven(); s.Faith = 39;
        GameEngine.BuyBuilding(s, BuildingType.Shrine);
        Assert.Equal(0, s.Buildings.GetValueOrDefault(BuildingType.Shrine));
        Assert.Equal(39, s.Faith);
    }

    [Fact]
    public void TickIncome_FollowerPassiveFaith()
    {
        var s = NewCoven(); s.Followers = 10;
        var (faith, _) = GameEngine.TickIncome(s);
        Assert.Equal(2.0, faith, precision: 2);
    }

    [Fact]
    public void TickIncome_FollowerPassiveGold()
    {
        var s = NewCoven(); s.Followers = 10;
        var (_, gold) = GameEngine.TickIncome(s);
        Assert.Equal(1.0, gold, precision: 2);
    }

    [Fact]
    public void TickIncome_ShrineAddsFlatFaith()
    {
        var s = NewCoven(); s.Buildings[BuildingType.Shrine] = 3;
        var (faith, _) = GameEngine.TickIncome(s);
        Assert.Equal(3.0, faith, precision: 2);
    }

    [Fact]
    public void TickIncome_CathedralAddsFlatGold()
    {
        var s = NewCoven(); s.Buildings[BuildingType.Cathedral] = 2;
        var (_, gold) = GameEngine.TickIncome(s);
        Assert.Equal(1.2, gold, precision: 2);
    }

    [Fact]
    public void TickIncome_MonolithBoostsFaith()
    {
        var s = NewCoven(); s.Followers = 10; s.Buildings[BuildingType.Monolith] = 2;
        var (faith, _) = GameEngine.TickIncome(s);
        Assert.Equal(2.4, faith, precision: 2);
    }

    [Fact]
    public void TickIncome_TreasuryBoostsGold()
    {
        var s = NewCoven(); s.Followers = 10; s.Buildings[BuildingType.Treasury] = 3;
        var (_, gold) = GameEngine.TickIncome(s);
        Assert.Equal(1.3, gold, precision: 2);
    }

    [Fact]
    public void TickIncome_AscendanceBoostsEverything()
    {
        var s = NewCoven(); s.Followers = 10; s.Upgrades.Add(UpgradeId.Ascendance);
        var (faith, gold) = GameEngine.TickIncome(s);
        Assert.Equal(3.0, faith, precision: 2);
        Assert.Equal(1.5, gold, precision: 2);
    }

    [Fact]
    public void BuyUpgrade_SpendsResources()
    {
        var s = NewCoven(); s.Faith = 200;
        GameEngine.BuyUpgrade(s, UpgradeId.Hymnal);
        Assert.Equal(80, s.Faith);
        Assert.Contains(UpgradeId.Hymnal, s.Upgrades);
    }

    [Fact]
    public void BuyUpgrade_LockedByFollowers()
    {
        var s = NewCoven(); s.Followers = 10; s.Gold = 500;
        GameEngine.BuyUpgrade(s, UpgradeId.Relics);
        Assert.DoesNotContain(UpgradeId.Relics, s.Upgrades);
        Assert.Equal(500, s.Gold);
    }

    [Fact]
    public void BuyUpgrade_AlreadyOwnedDoesNothing()
    {
        var s = NewCoven(); s.Faith = 500; s.Upgrades.Add(UpgradeId.Hymnal);
        GameEngine.BuyUpgrade(s, UpgradeId.Hymnal);
        Assert.Equal(500, s.Faith);
    }

    [Fact]
    public void UpgradeUnlocked_AtThreshold()
    {
        var s = NewCoven(); s.Followers = 15;
        var relics = GameData.Upgrades.First(u => u.Id == UpgradeId.Relics);
        Assert.True(GameEngine.UpgradeUnlocked(s, relics));
    }

    [Fact]
    public void UpgradeUnlocked_BelowThreshold()
    {
        var s = NewCoven(); s.Followers = 14;
        var relics = GameData.Upgrades.First(u => u.Id == UpgradeId.Relics);
        Assert.False(GameEngine.UpgradeUnlocked(s, relics));
    }

    [Theory]
    [InlineData(0, "Novice")]
    [InlineData(24, "Novice")]
    [InlineData(25, "Adept")]
    [InlineData(99, "Adept")]
    [InlineData(100, "Mystic")]
    [InlineData(250, "Prophet")]
    [InlineData(600, "Demigod")]
    [InlineData(1500, "Ascended")]
    [InlineData(5000, "Ascended")]
    public void RankFor_ReturnsCorrectRank(int followers, string expected)
    {
        Assert.Equal(expected, GameEngine.RankFor(followers).Name);
    }

    [Fact]
    public void NextRank_ReturnsNextThreshold()
    {
        Assert.Equal("Adept", GameEngine.NextRank(0)!.Name);
        Assert.Equal("Mystic", GameEngine.NextRank(25)!.Name);
    }

    [Fact]
    public void NextRank_NullAtMaxRank()
    {
        Assert.Null(GameEngine.NextRank(1500));
        Assert.Null(GameEngine.NextRank(5000));
    }

    [Fact]
    public void RankProgress_BetweenRanks()
    {
        var s = NewCoven(); s.Followers = 12;
        Assert.Equal(0.48, GameEngine.RankProgress(s), precision: 2);
    }

    [Fact]
    public void RankProgress_FullAtMaxRank()
    {
        var s = NewCoven(); s.Followers = 2000;
        Assert.Equal(1.0, GameEngine.RankProgress(s));
    }

    [Fact]
    public void SaveLoad_RoundTrips()
    {
        var s = NewState();
        s.CultName = "Test Cult";
        s.HomeCoven.Followers = 42; s.HomeCoven.Faith = 100.5; s.HomeCoven.Gold = 50.3;
        s.HomeCoven.Buildings[BuildingType.Shrine] = 3; s.HomeCoven.Upgrades.Add(UpgradeId.Hymnal);
        var json = SaveLoad.SaveGame(s);
        var loaded = SaveLoad.LoadGame(json);
        Assert.Equal("Test Cult", loaded.CultName);
        Assert.Equal(42, loaded.HomeCoven.Followers);
        Assert.Equal(100.5, loaded.HomeCoven.Faith);
        Assert.Equal(50.3, loaded.HomeCoven.Gold);
        Assert.Equal(3, loaded.HomeCoven.Buildings[BuildingType.Shrine]);
        Assert.Contains(UpgradeId.Hymnal, loaded.HomeCoven.Upgrades);
    }

    [Fact]
    public void LoadGame_NullReturnsFresh()
    {
        var s = SaveLoad.LoadGame(null);
        Assert.Equal(0, s.HomeCoven.Followers);
        Assert.Equal("", s.CultName);
    }

    [Fact]
    public void LoadGame_CorruptReturnsFresh()
    {
        var s = SaveLoad.LoadGame("not valid json");
        Assert.Equal(0, s.HomeCoven.Followers);
    }

    [Theory]
    [InlineData(0.5, "0.5")]
    [InlineData(9.9, "9.9")]
    [InlineData(10, "10")]
    [InlineData(999, "999")]
    [InlineData(1500, "1.50K")]
    [InlineData(1_000_000, "1.00M")]
    [InlineData(1_500_000_000, "1.50B")]
    public void Fmt_FormatsCorrectly(double value, string expected)
    {
        Assert.Equal(expected, NumberFormat.Fmt(value));
    }

    [Fact]
    public void Fmt_HandlesNegative()
    {
        Assert.StartsWith("-", NumberFormat.Fmt(-5.0));
    }

    [Fact]
    public void EventChoice_AppliesMutation()
    {
        var s = NewCoven(); s.Faith = 100; s.Followers = 10;
        var wanderer = GameData.Events.First(e => e.Id == "lost_wanderer");
        wanderer.ChoiceA.Apply(s);
        Assert.Equal(13, s.Followers);
        Assert.Equal(80, s.Faith);
    }

    [Fact]
    public void Events_AllHaveTwoChoices()
    {
        foreach (var ev in GameData.Events)
        {
            Assert.NotNull(ev.ChoiceA);
            Assert.NotNull(ev.ChoiceB);
            Assert.NotEmpty(ev.ChoiceA.Label);
            Assert.NotEmpty(ev.ChoiceB.Label);
        }
    }

    [Fact]
    public void FaithMultiplier_NoUpgrades()
    {
        var s = NewCoven();
        Assert.Equal(1.0, GameEngine.FaithMultiplier(s));
    }

    [Fact]
    public void FaithMultiplier_WithVisions()
    {
        var s = NewCoven(); s.Upgrades.Add(UpgradeId.Visions);
        Assert.Equal(2.0, GameEngine.FaithMultiplier(s));
    }

    [Fact]
    public void GoldMultiplier_WithRelics()
    {
        var s = NewCoven(); s.Upgrades.Add(UpgradeId.Relics);
        Assert.Equal(2.0, GameEngine.GoldMultiplier(s));
    }

    [Fact]
    public void GoldMultiplier_WithTreasury()
    {
        var s = NewCoven(); s.Buildings[BuildingType.Treasury] = 5;
        Assert.Equal(1.5, GameEngine.GoldMultiplier(s));
    }

    [Fact]
    public void Simulation_FullProgression()
    {
        var s = NewState(); s.CultName = "Test Order";
        for (int i = 0; i < 50; i++) GameEngine.Preach(s.ActiveCoven);
        Assert.True(s.ActiveCoven.Faith >= 40);
        while (GameEngine.CanRecruit(s.ActiveCoven) && s.ActiveCoven.Followers < 5) GameEngine.Recruit(s.ActiveCoven);
        Assert.True(s.ActiveCoven.Followers > 0);
        s.ActiveCoven.Faith = 100;
        GameEngine.BuyBuilding(s.ActiveCoven, BuildingType.Shrine);
        Assert.Equal(1, s.ActiveCoven.Buildings[BuildingType.Shrine]);
        var (faith, gold) = GameEngine.TickIncome(s.ActiveCoven);
        Assert.True(faith > 0);
        Assert.True(gold > 0);
        Assert.Equal("Novice", GameEngine.RankFor(s.ActiveCoven.Followers).Name);
        s.ActiveCoven.Followers = 25;
        Assert.Equal("Adept", GameEngine.RankFor(s.ActiveCoven.Followers).Name);
    }

    // --- Coven takeover tests ---

    private static WorldLocationDef Loc(string id, int req) =>
        new WorldLocationDef(id, id, "", "", "", "", 0, 0, "", "", req, 1.0);

    private static ImmutableArray<WorldLocationDef> Locations(params WorldLocationDef[] locs) =>
        ImmutableArray.Create(locs);

    [Fact]
    public void InitialState_HasHomeCoven()
    {
        var s = NewState();
        Assert.Single(s.Covens);
        Assert.Equal("skanor", s.HomeCoven.Id);
        Assert.True(s.HomeCoven.TakenOver);
        Assert.Equal("skanor", s.ActiveCovenId);
    }

    [Fact]
    public void CanTakeover_RequiresFollowers()
    {
        var s = NewState();
        s.HomeCoven.Followers = 49;
        var locs = Locations(Loc("rival", 50));
        Assert.False(CovenProgress.CanTakeover(s, locs[0]));
        s.HomeCoven.Followers = 50;
        Assert.True(CovenProgress.CanTakeover(s, locs[0]));
    }

    [Fact]
    public void CanTakeover_CannotTakeHome()
    {
        var s = NewState();
        var home = Loc("skanor", 0);
        Assert.False(CovenProgress.CanTakeover(s, home));
    }

    [Fact]
    public void Takeover_MarksCovenAndSpendsResources()
    {
        var s = NewState();
        s.HomeCoven.Followers = 100; s.HomeCoven.Faith = 200; s.HomeCoven.Gold = 150;
        var loc = Loc("rival", 50);
        CovenProgress.Takeover(s, loc);
        var rival = s.FindCoven("rival");
        Assert.NotNull(rival);
        Assert.True(rival!.TakenOver);
        Assert.Equal(0, rival.Followers);
        Assert.Equal(50, s.HomeCoven.Followers);
        Assert.Equal(100, s.HomeCoven.Faith);
        Assert.Equal(75, s.HomeCoven.Gold);
    }

    [Fact]
    public void NextTarget_ReturnsFirstNotTakenOver()
    {
        var s = NewState();
        var locs = Locations(Loc("a", 10), Loc("b", 20), Loc("c", 30));
        Assert.Equal("a", CovenProgress.NextTarget(s, locs)!.Id);
        CovenProgress.Takeover(s, locs[0]);
        Assert.Equal("b", CovenProgress.NextTarget(s, locs)!.Id);
    }

    [Fact]
    public void NextTarget_NullWhenAllTaken()
    {
        var s = NewState();
        var locs = Locations(Loc("a", 10));
        CovenProgress.Takeover(s, locs[0]);
        Assert.Null(CovenProgress.NextTarget(s, locs));
    }

    [Fact]
    public void TakeoverProgress_FractionThenFull()
    {
        var s = NewState();
        var loc = Loc("rival", 100);
        s.HomeCoven.Followers = 25;
        Assert.Equal(0.25, CovenProgress.TakeoverProgress(s, loc), precision: 2);
        s.HomeCoven.Followers = 100;
        Assert.Equal(1.0, CovenProgress.TakeoverProgress(s, loc));
    }

    [Fact]
    public void SwitchActive_OnlyToTakenOver()
    {
        var s = NewState();
        s.Covens.Add(new CovenState { Id = "rival" });
        CovenProgress.SwitchActive(s, "rival");
        Assert.Equal("skanor", s.ActiveCovenId);
        s.FindCoven("rival")!.TakenOver = true;
        CovenProgress.SwitchActive(s, "rival");
        Assert.Equal("rival", s.ActiveCovenId);
    }

    [Fact]
    public void TickAllCovens_OnlyTakenOver()
    {
        var s = NewState();
        s.HomeCoven.Followers = 10;
        s.Covens.Add(new CovenState { Id = "rival", Followers = 20 });
        GameEngine.TickAllCovens(s);
        Assert.True(s.HomeCoven.Faith > 0);
        Assert.Equal(0, s.FindCoven("rival")!.Faith);
    }

    [Fact]
    public void TotalFollowers_SumsTakenOver()
    {
        var s = NewState();
        s.HomeCoven.Followers = 30;
        s.Covens.Add(new CovenState { Id = "rival", TakenOver = true, Followers = 20 });
        Assert.Equal(50, CovenProgress.TotalFollowers(s));
    }

    [Fact]
    public void SaveLoad_MigratesOldSave()
    {
        // Simulate a pre-story save: no Covens list, StoryShown missing
        var oldJson = "{\"CultName\":\"Old Cult\",\"StartedAt\":1000,\"StoryShown\":false,\"ActiveCovenId\":\"\",\"Covens\":[]}";
        var loaded = SaveLoad.LoadGame(oldJson);
        Assert.Equal("Old Cult", loaded.CultName);
        Assert.Single(loaded.Covens);
        Assert.Equal("skanor", loaded.HomeCoven.Id);
        Assert.False(loaded.StoryShown);
        Assert.Equal("skanor", loaded.ActiveCovenId);
    }
}
