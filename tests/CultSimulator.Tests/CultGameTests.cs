using System.Collections.Immutable;
using CultSimulator.Game;

namespace CultSimulator.Tests;

public class CultGameTests
{
    private static GameState NewState()
    {
        var s = GameEngine.InitialState();
        s.HomeCoven.Followers = 10000;
        s.HomeCoven.Faith = 100000;
        s.HomeCoven.Gold = 50000;
        return s;
    }

    private static ImmutableArray<WorldLocationDef> TestLocations() => ImmutableArray.Create(
        new WorldLocationDef("skanor", "Skanör", "", "", "", "", "europe", 0, 0, "", "", 0, 1.0, new()),
        new WorldLocationDef("uppsala_gothi", "Uppsala", "", "", "", "", "europe", 0, 0, "", "", 20, 1.0, new()),
        new WorldLocationDef("hedeby_vikings", "Hedeby", "", "", "", "", "europe", 0, 0, "", "", 80, 1.0, new()),
        new WorldLocationDef("salem_remnant", "Salem", "", "", "", "", "north_america", 0, 0, "", "", 800, 1.0, new()),
        new WorldLocationDef("voodoo_quarter", "Voodoo", "", "", "", "", "north_america", 0, 0, "", "", 1200, 1.0, new()));

    [Fact]
    public void HomeCoven_Initialized()
    {
        var s = GameEngine.InitialState();
        Assert.Equal("skanor", s.ActiveCovenId);
        Assert.NotNull(s.HomeCoven);
        Assert.Equal("skanor", s.HomeCoven.Id);
    }

    [Fact]
    public void TotalFollowers_SumsConvertedCovens()
    {
        var s = NewState();
        s.HomeCoven.Followers = 100;
        Assert.Equal(100, CovenProgress.TotalFollowers(s));
    }

    [Fact]
    public void CanConvert_RequiresFollowers()
    {
        var s = NewState();
        s.HomeCoven.Followers = 10;
        var loc = TestLocations()[1]; // 20 followers required
        Assert.False(CovenProgress.CanConvert(s, loc));
        s.HomeCoven.Followers = 20;
        Assert.True(CovenProgress.CanConvert(s, loc));
    }

    [Fact]
    public void Takeover_MarksCovenConverted()
    {
        var s = NewState();
        var loc = TestLocations()[1];
        CovenProgress.Takeover(s, loc);
        var coven = s.FindCoven(loc.Id);
        Assert.NotNull(coven);
        Assert.True(coven!.Converted);
    }

    [Fact]
    public void NextTarget_ReturnsFirstUnconverted()
    {
        var s = NewState();
        var locs = TestLocations();
        var next = CovenProgress.NextTarget(s, locs);
        Assert.NotNull(next);
        Assert.Equal("uppsala_gothi", next!.Id);
    }

    [Fact]
    public void NextTarget_NullWhenAllConverted()
    {
        var s = NewState();
        var locs = TestLocations();
        foreach (var loc in locs.Where(l => l.Id != "skanor"))
        {
            CovenProgress.Takeover(s, loc);
        }
        Assert.Null(CovenProgress.NextTarget(s, locs));
    }

    [Fact]
    public void HasCovenInContinent_EuropeAlwaysTrue()
    {
        var s = NewState();
        Assert.True(CovenProgress.HasCovenInContinent(s, TestLocations(), "europe"));
    }

    [Fact]
    public void HasCovenInContinent_OtherContinentsRequireConvertedCoven()
    {
        var s = NewState();
        Assert.False(CovenProgress.HasCovenInContinent(s, TestLocations(), "north_america"));
        CovenProgress.Takeover(s, TestLocations()[3]); // salem_remnant
        Assert.True(CovenProgress.HasCovenInContinent(s, TestLocations(), "north_america"));
    }

    [Fact]
    public void IsContinentComplete_FalseWhenNotAllConverted()
    {
        var s = NewState();
        var locs = TestLocations();
        CovenProgress.Takeover(s, locs[1]);
        Assert.False(CovenProgress.IsContinentComplete(s, locs, "europe"));
    }

    [Fact]
    public void IsContinentComplete_TrueWhenAllConverted()
    {
        var s = NewState();
        var locs = TestLocations();
        CovenProgress.Takeover(s, locs[1]);
        CovenProgress.Takeover(s, locs[2]);
        Assert.True(CovenProgress.IsContinentComplete(s, locs, "europe"));
    }

    [Fact]
    public void CurrentContinent_ReturnsFirstUncompleted()
    {
        var s = NewState();
        var locs = TestLocations();
        Assert.Equal("europe", CovenProgress.CurrentContinent(s, locs));
        CovenProgress.Takeover(s, locs[1]);
        CovenProgress.Takeover(s, locs[2]);
        Assert.Equal("north_america", CovenProgress.CurrentContinent(s, locs));
    }

    [Fact]
    public void IsContinentUnlocked_EuropeAlwaysTrue()
    {
        var s = NewState();
        Assert.True(CovenProgress.IsContinentUnlocked(s, TestLocations(), "europe"));
    }

    [Fact]
    public void IsContinentUnlocked_NorthAmericaRequiresEuropeComplete()
    {
        var s = NewState();
        var locs = TestLocations();
        Assert.False(CovenProgress.IsContinentUnlocked(s, locs, "north_america"));
        CovenProgress.Takeover(s, locs[1]);
        CovenProgress.Takeover(s, locs[2]);
        Assert.True(CovenProgress.IsContinentUnlocked(s, locs, "north_america"));
    }

    [Fact]
    public void ConversionProgress_ReturnsRatio()
    {
        var s = NewState();
        s.HomeCoven.Followers = 10;
        var loc = TestLocations()[1]; // 20 required
        Assert.Equal(0.5, CovenProgress.ConversionProgress(s, loc));
    }

    [Fact]
    public void ConversionProgress_CappedAtOne()
    {
        var s = NewState();
        s.HomeCoven.Followers = 1000;
        var loc = TestLocations()[1]; // 20 required
        Assert.Equal(1.0, CovenProgress.ConversionProgress(s, loc));
    }

    [Fact]
    public void SwitchActive_ChangesActiveCoven()
    {
        var s = NewState();
        var locs = TestLocations();
        CovenProgress.Takeover(s, locs[3]);
        CovenProgress.SwitchActive(s, locs[3].Id);
        Assert.Equal(locs[3].Id, s.ActiveCovenId);
    }

    [Fact]
    public void SwitchActive_DoesNotSwitchToUnconverted()
    {
        var s = NewState();
        var locs = TestLocations();
        CovenProgress.SwitchActive(s, locs[3].Id);
        Assert.Equal("skanor", s.ActiveCovenId);
    }

    [Fact]
    public void LocalCultEngine_SpawnOne_AddsActiveCult()
    {
        var s = NewState();
        LocalCultEngine.SpawnOne(s, "skanor");
        Assert.Single(s.ActiveLocalCults);
    }

    [Fact]
    public void LocalCultEngine_ConvertWithFollowerReward()
    {
        var s = NewState();
        s.HomeCoven.Followers = 100;
        LocalCultEngine.SpawnOne(s, "skanor");
        var cultId = s.ActiveLocalCults[0].CultId;
        var followersBefore = s.HomeCoven.Followers;
        LocalCultEngine.Convert(s, cultId, LocalCultReward.Followers);
        Assert.True(s.HomeCoven.Followers > followersBefore);
        Assert.Empty(s.ActiveLocalCults);
    }

    [Fact]
    public void LocalCultEngine_ConvertWithGoldReward()
    {
        var s = NewState();
        s.HomeCoven.Followers = 100;
        LocalCultEngine.SpawnOne(s, "skanor");
        var cultId = s.ActiveLocalCults[0].CultId;
        var goldBefore = s.HomeCoven.Gold;
        LocalCultEngine.Convert(s, cultId, LocalCultReward.Gold);
        Assert.True(s.HomeCoven.Gold > goldBefore);
        Assert.Empty(s.ActiveLocalCults);
    }

    // --- Continent foothold tests ---

    private static WorldLocationDef LocIn(string id, string continent, int req) =>
        new WorldLocationDef(id, id, "", "", "", "", continent, 0, 0, "", "", req, 1.0, new List<CovenEventData>());

    private static ImmutableArray<WorldLocationDef> ContinentLocations() => ImmutableArray.Create(
        LocIn("e1", "europe", 10), LocIn("e2", "europe", 20), LocIn("e3", "europe", 30),
        LocIn("n1", "north_america", 100), LocIn("n2", "north_america", 200));

    private static void ConvertCoven(GameState s, string id)
    {
        var existing = s.FindCoven(id);
        if (existing != null) { existing.Converted = true; return; }
        s.Covens.Add(new CovenState { Id = id, Converted = true, Buildings = new Dictionary<BuildingType, int>(), Upgrades = new List<UpgradeId>() });
    }

    [Fact]
    public void IsContinentComplete_TrueWhenAllConverted()
    {
        var s = NewState();
        var locs = ContinentLocations();
        Assert.False(CovenProgress.IsContinentComplete(s, locs, "europe"));
        ConvertCoven(s, "e1");
        ConvertCoven(s, "e2");
        ConvertCoven(s, "e3");
        Assert.True(CovenProgress.IsContinentComplete(s, locs, "europe"));
    }

    [Fact]
    public void NewlyCompletedContinent_NullWhenNotComplete()
    {
        var s = NewState();
        var locs = ContinentLocations();
        ConvertCoven(s, "e1");
        ConvertCoven(s, "e2");
        Assert.Null(CovenProgress.NewlyCompletedContinent(s, locs));
    }

    [Fact]
    public void NewlyCompletedContinent_ReturnsEuropeWhenComplete()
    {
        var s = NewState();
        var locs = ContinentLocations();
        ConvertCoven(s, "e1");
        ConvertCoven(s, "e2");
        ConvertCoven(s, "e3");
        Assert.Equal("europe", CovenProgress.NewlyCompletedContinent(s, locs));
    }

    [Fact]
    public void MarkContinentStoryPending_SetsFlag()
    {
        var s = NewState();
        CovenProgress.MarkContinentStoryPending(s, "europe");
        Assert.Equal("europe", s.PendingContinentStory);
    }

    [Fact]
    public void GrantFoothold_AddsCovenAndClearsPending()
    {
        var s = NewState();
        var locs = ContinentLocations();
        CovenProgress.MarkContinentStoryPending(s, "europe");
        var foothold = CovenProgress.GrantFoothold(s, locs);
        Assert.NotNull(foothold);
        Assert.Equal("north_america", foothold!.Continent);
        Assert.Contains(foothold.CovenId, s.RevealedFootholds);
        Assert.Null(s.PendingContinentStory);
        var coven = s.FindCoven(foothold.CovenId);
        Assert.NotNull(coven);
        Assert.True(coven!.Converted);
    }

    [Fact]
    public void GrantFoothold_NullWhenNoPending()
    {
        var s = NewState();
        var locs = ContinentLocations();
        Assert.Null(CovenProgress.GrantFoothold(s, locs));
    }

    [Fact]
    public void NewlyCompletedContinent_NullAfterFootholdGranted()
    {
        var s = NewState();
        var locs = ContinentLocations();
        ConvertCoven(s, "e1");
        ConvertCoven(s, "e2");
        ConvertCoven(s, "e3");
        Assert.Equal("europe", CovenProgress.NewlyCompletedContinent(s, locs));
        CovenProgress.MarkContinentStoryPending(s, "europe");
        CovenProgress.GrantFoothold(s, locs);
        Assert.Null(CovenProgress.NewlyCompletedContinent(s, locs));
    }

    [Fact]
    public void HasCovenInContinent_TrueAfterFootholdGranted()
    {
        var s = NewState();
        var locList = ContinentLocations().ToList();
        Assert.False(CovenProgress.HasCovenInContinent(s, locList.ToImmutableArray(), "north_america"));
        CovenProgress.MarkContinentStoryPending(s, "europe");
        var foothold = CovenProgress.GrantFoothold(s, locList.ToImmutableArray());
        Assert.NotNull(foothold);
        locList.Add(ContinentFootholds.ToLocation(foothold!));
        Assert.True(CovenProgress.HasCovenInContinent(s, locList.ToImmutableArray(), "north_america"));
    }

    [Fact]
    public void ContinentFootholds_HasEntryForEachTransition()
    {
        Assert.NotNull(ContinentFootholds.ForCompleted("europe"));
        Assert.NotNull(ContinentFootholds.ForCompleted("north_america"));
        Assert.NotNull(ContinentFootholds.ForCompleted("south_america"));
        Assert.NotNull(ContinentFootholds.ForCompleted("africa"));
        Assert.NotNull(ContinentFootholds.ForCompleted("middle_east"));
        Assert.NotNull(ContinentFootholds.ForCompleted("asia"));
    }

    [Fact]
    public void ContinentFootholds_OceaniaHasNoNextFoothold()
    {
        Assert.Null(ContinentFootholds.ForCompleted("oceania"));
    }

    [Fact]
    public void ContinentFootholds_EachFootholdUnlocksNextContinent()
    {
        Assert.Equal("north_america", ContinentFootholds.ForCompleted("europe")!.Continent);
        Assert.Equal("south_america", ContinentFootholds.ForCompleted("north_america")!.Continent);
        Assert.Equal("africa", ContinentFootholds.ForCompleted("south_america")!.Continent);
        Assert.Equal("middle_east", ContinentFootholds.ForCompleted("africa")!.Continent);
        Assert.Equal("asia", ContinentFootholds.ForCompleted("middle_east")!.Continent);
        Assert.Equal("oceania", ContinentFootholds.ForCompleted("asia")!.Continent);
    }

    [Fact]
    public void ContinentFootholds_IsFoothold_RecognizesFootholdIds()
    {
        Assert.True(ContinentFootholds.IsFoothold("vinland_outpost"));
        Assert.True(ContinentFootholds.IsFoothold("wayfinder_shrine"));
        Assert.False(ContinentFootholds.IsFoothold("skanor"));
        Assert.False(ContinentFootholds.IsFoothold("nonexistent"));
    }
}
