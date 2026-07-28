using CultSimulator.Game;

namespace CultSimulator.Tests;

public class CultGameTests
{
    private static GameState NewState() => GameEngine.InitialState();

    [Fact]
    public void Preach_GeneratesFaith()
    {
        var s = NewState();
        GameEngine.Preach(s);
        Assert.True(s.Faith > 0);
        Assert.Equal(1, s.PreachCount);
    }

    [Fact]
    public void Preach_YieldScalesWithFollowers()
    {
        var s = NewState(); s.Followers = 100;
        GameEngine.Preach(s);
        Assert.Equal(2.0, s.Faith, precision: 2);
    }

    [Fact]
    public void Preach_HymnalDoublesYield()
    {
        var s = NewState(); s.Followers = 10;
        GameEngine.Preach(s); var without = s.Faith;
        s.Faith = 0; s.Upgrades.Add(UpgradeId.Hymnal);
        GameEngine.Preach(s);
        Assert.Equal(without * 2, s.Faith, precision: 2);
    }

    [Fact]
    public void Preach_AscendanceMultipliesYield()
    {
        var s = NewState(); s.Followers = 10; s.Upgrades.Add(UpgradeId.Ascendance);
        GameEngine.Preach(s);
        Assert.Equal(1.65, s.Faith, precision: 2);
    }

    [Fact]
    public void Recruit_ConvertsFaithToFollower()
    {
        var s = NewState(); s.Faith = 30;
        GameEngine.Recruit(s);
        Assert.Equal(1, s.Followers);
        Assert.Equal(20, s.Faith);
    }

    [Fact]
    public void Recruit_RequiresEnoughFaith()
    {
        var s = NewState(); s.Faith = 9;
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
        var s = NewState(); s.Faith = 100;
        GameEngine.BuyBuilding(s, BuildingType.Shrine);
        Assert.Equal(60, s.Faith);
        Assert.Equal(1, s.Buildings[BuildingType.Shrine]);
    }

    [Fact]
    public void BuyBuilding_SpendGold()
    {
        var s = NewState(); s.Gold = 200;
        GameEngine.BuyBuilding(s, BuildingType.Cathedral);
        Assert.Equal(120, s.Gold);
        Assert.Equal(1, s.Buildings[BuildingType.Cathedral]);
    }

    [Fact]
    public void BuyBuilding_InsufficientFaithDoesNothing()
    {
        var s = NewState(); s.Faith = 39;
        GameEngine.BuyBuilding(s, BuildingType.Shrine);
        Assert.Equal(0, s.Buildings.GetValueOrDefault(BuildingType.Shrine));
        Assert.Equal(39, s.Faith);
    }

    [Fact]
    public void TickIncome_FollowerPassiveFaith()
    {
        var s = NewState(); s.Followers = 10;
        var (faith, _) = GameEngine.TickIncome(s);
        Assert.Equal(2.0, faith, precision: 2);
    }

    [Fact]
    public void TickIncome_FollowerPassiveGold()
    {
        var s = NewState(); s.Followers = 10;
        var (_, gold) = GameEngine.TickIncome(s);
        Assert.Equal(1.0, gold, precision: 2);
    }

    [Fact]
    public void TickIncome_ShrineAddsFlatFaith()
    {
        var s = NewState(); s.Buildings[BuildingType.Shrine] = 3;
        var (faith, _) = GameEngine.TickIncome(s);
        Assert.Equal(3.0, faith, precision: 2);
    }

    [Fact]
    public void TickIncome_CathedralAddsFlatGold()
    {
        var s = NewState(); s.Buildings[BuildingType.Cathedral] = 2;
        var (_, gold) = GameEngine.TickIncome(s);
        Assert.Equal(1.2, gold, precision: 2);
    }

    [Fact]
    public void TickIncome_MonolithBoostsFaith()
    {
        var s = NewState(); s.Followers = 10; s.Buildings[BuildingType.Monolith] = 2;
        var (faith, _) = GameEngine.TickIncome(s);
        Assert.Equal(2.4, faith, precision: 2);
    }

    [Fact]
    public void TickIncome_TreasuryBoostsGold()
    {
        var s = NewState(); s.Followers = 10; s.Buildings[BuildingType.Treasury] = 3;
        var (_, gold) = GameEngine.TickIncome(s);
        Assert.Equal(1.3, gold, precision: 2);
    }

    [Fact]
    public void TickIncome_AscendanceBoostsEverything()
    {
        var s = NewState(); s.Followers = 10; s.Upgrades.Add(UpgradeId.Ascendance);
        var (faith, gold) = GameEngine.TickIncome(s);
        Assert.Equal(3.0, faith, precision: 2);
        Assert.Equal(1.5, gold, precision: 2);
    }

    [Fact]
    public void BuyUpgrade_SpendsResources()
    {
        var s = NewState(); s.Faith = 200;
        GameEngine.BuyUpgrade(s, UpgradeId.Hymnal);
        Assert.Equal(80, s.Faith);
        Assert.Contains(UpgradeId.Hymnal, s.Upgrades);
    }

    [Fact]
    public void BuyUpgrade_LockedByFollowers()
    {
        var s = NewState(); s.Followers = 10; s.Gold = 500;
        GameEngine.BuyUpgrade(s, UpgradeId.Relics);
        Assert.DoesNotContain(UpgradeId.Relics, s.Upgrades);
        Assert.Equal(500, s.Gold);
    }

    [Fact]
    public void BuyUpgrade_AlreadyOwnedDoesNothing()
    {
        var s = NewState(); s.Faith = 500; s.Upgrades.Add(UpgradeId.Hymnal);
        GameEngine.BuyUpgrade(s, UpgradeId.Hymnal);
        Assert.Equal(500, s.Faith);
    }

    [Fact]
    public void UpgradeUnlocked_AtThreshold()
    {
        var s = NewState(); s.Followers = 15;
        var relics = GameData.Upgrades.First(u => u.Id == UpgradeId.Relics);
        Assert.True(GameEngine.UpgradeUnlocked(s, relics));
    }

    [Fact]
    public void UpgradeUnlocked_BelowThreshold()
    {
        var s = NewState(); s.Followers = 14;
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
        var s = NewState(); s.Followers = 12;
        Assert.Equal(0.48, GameEngine.RankProgress(s), precision: 2);
    }

    [Fact]
    public void RankProgress_FullAtMaxRank()
    {
        var s = NewState(); s.Followers = 2000;
        Assert.Equal(1.0, GameEngine.RankProgress(s));
    }

    [Fact]
    public void SaveLoad_RoundTrips()
    {
        var s = NewState();
        s.CultName = "Test Cult"; s.Followers = 42; s.Faith = 100.5; s.Gold = 50.3;
        s.Buildings[BuildingType.Shrine] = 3; s.Upgrades.Add(UpgradeId.Hymnal);
        var json = SaveLoad.SaveGame(s);
        var loaded = SaveLoad.LoadGame(json);
        Assert.Equal("Test Cult", loaded.CultName);
        Assert.Equal(42, loaded.Followers);
        Assert.Equal(100.5, loaded.Faith);
        Assert.Equal(50.3, loaded.Gold);
        Assert.Equal(3, loaded.Buildings[BuildingType.Shrine]);
        Assert.Contains(UpgradeId.Hymnal, loaded.Upgrades);
    }

    [Fact]
    public void LoadGame_NullReturnsFresh()
    {
        var s = SaveLoad.LoadGame(null);
        Assert.Equal(0, s.Followers);
        Assert.Equal("", s.CultName);
    }

    [Fact]
    public void LoadGame_CorruptReturnsFresh()
    {
        var s = SaveLoad.LoadGame("not valid json");
        Assert.Equal(0, s.Followers);
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
        var s = NewState(); s.Faith = 100; s.Followers = 10;
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
        var s = NewState();
        Assert.Equal(1.0, GameEngine.FaithMultiplier(s));
    }

    [Fact]
    public void FaithMultiplier_WithVisions()
    {
        var s = NewState(); s.Upgrades.Add(UpgradeId.Visions);
        Assert.Equal(2.0, GameEngine.FaithMultiplier(s));
    }

    [Fact]
    public void GoldMultiplier_WithRelics()
    {
        var s = NewState(); s.Upgrades.Add(UpgradeId.Relics);
        Assert.Equal(2.0, GameEngine.GoldMultiplier(s));
    }

    [Fact]
    public void GoldMultiplier_WithTreasury()
    {
        var s = NewState(); s.Buildings[BuildingType.Treasury] = 5;
        Assert.Equal(1.5, GameEngine.GoldMultiplier(s));
    }

    [Fact]
    public void Simulation_FullProgression()
    {
        var s = NewState(); s.CultName = "Test Order";
        for (int i = 0; i < 50; i++) GameEngine.Preach(s);
        Assert.True(s.Faith >= 40);
        while (GameEngine.CanRecruit(s) && s.Followers < 5) GameEngine.Recruit(s);
        Assert.True(s.Followers > 0);
        s.Faith = 100;
        GameEngine.BuyBuilding(s, BuildingType.Shrine);
        Assert.Equal(1, s.Buildings[BuildingType.Shrine]);
        var (faith, gold) = GameEngine.TickIncome(s);
        Assert.True(faith > 0);
        Assert.True(gold > 0);
        Assert.Equal("Novice", GameEngine.RankFor(s.Followers).Name);
        s.Followers = 25;
        Assert.Equal("Adept", GameEngine.RankFor(s.Followers).Name);
    }
}
