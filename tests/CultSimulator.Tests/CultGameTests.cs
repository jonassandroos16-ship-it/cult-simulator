using System.Collections.Immutable;
using System.Net.Http;
using CultSimulator.Game;

namespace CultSimulator.Tests;

public class CultGameTests
{
    private static GameState NewState() => GameEngine.InitialState();
    private static CovenState NewCoven() => new CovenState { Id = "skanor", TakenOver = true };

    private static WorldLocationService CreateLocations()
    {
        var svc = new WorldLocationService(new HttpClient());
        var locs = ImmutableArray.Create(
            new WorldLocationDef("skanor", "Skanör", "", "Sweden", "🇸🇪", "Viking Age", "europe", 55.63, 13.07, "", "", 0, 1.0, new List<CovenEventData>()),
            new WorldLocationDef("la_recta_provincia", "La Recta Provincia", "", "Chile", "🇨🇱", "Colonial", "south_america", -42.18, -73.90, "", "", 25, 1.0, new List<CovenEventData>
            {
                new CovenEventData { Id = "lrp_1", Title = "Test Event 1", Narrative = "Test", ChoiceA = new CovenEventChoiceData { Label = "A", Description = "a" }, ChoiceB = new CovenEventChoiceData { Label = "B", Description = "b" } },
                new CovenEventData { Id = "lrp_2", Title = "Test Event 2", Narrative = "Test", ChoiceA = new CovenEventChoiceData { Label = "A", Description = "a" }, ChoiceB = new CovenEventChoiceData { Label = "B", Description = "b" } },
                new CovenEventData { Id = "lrp_3", Title = "Test Event 3", Narrative = "Test", ChoiceA = new CovenEventChoiceData { Label = "A", Description = "a" }, ChoiceB = new CovenEventChoiceData { Label = "B", Description = "b" } },
            }),
            new WorldLocationDef("rival", "Rival", "", "", "", "", "europe", 0, 0, "", "", 150, 1.0, new List<CovenEventData>()),
            new WorldLocationDef("a", "A", "", "", "", "", "europe", 0, 0, "", "", 10, 1.0, new List<CovenEventData>()),
            new WorldLocationDef("b", "B", "", "", "", "", "europe", 0, 0, "", "", 20, 1.0, new List<CovenEventData>()),
            new WorldLocationDef("c", "C", "", "", "", "", "europe", 0, 0, "", "", 30, 1.0, new List<CovenEventData>())
        );
        typeof(WorldLocationService).GetField("_locations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(svc, locs);
        typeof(WorldLocationService).GetField("_loaded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(svc, true);
        return svc;
    }

    private static ConversionDataService CreateConversions()
    {
        var locs = CreateLocations();
        return new ConversionDataService(locs);
    }

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
        Assert.Equal(1.4, s.Faith, precision: 2);
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
        Assert.Equal(1.56, s.Faith, precision: 2);
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
    public void Recruit_CostScalesWithFollowers()
    {
        var s = NewCoven(); s.Followers = 5; s.Faith = 1000;
        var costBefore = GameEngine.RecruitCostFor(s);
        GameEngine.Recruit(s);
        var costAfter = GameEngine.RecruitCostFor(s);
        Assert.True(costAfter > costBefore);
    }

    [Fact]
    public void BuyBuilding_SpendsFaith()
    {
        var s = NewCoven(); s.Faith = 100;
        GameEngine.BuyBuilding(s, BuildingType.Shrine);
        Assert.Equal(1, s.Buildings.GetValueOrDefault(BuildingType.Shrine));
        Assert.True(s.Faith < 100);
    }

    [Fact]
    public void BuyBuilding_CostScales()
    {
        var s = NewCoven(); s.Faith = 1000;
        GameEngine.BuyBuilding(s, BuildingType.Shrine);
        var firstCost = 40;
        Assert.True(s.Faith < 1000 - firstCost + 1);
    }

    [Fact]
    public void TickAllCovens_GeneratesPassiveIncome()
    {
        var s = NewState();
        s.HomeCoven.Followers = 10;
        s.HomeCoven.Buildings[BuildingType.Shrine] = 2;
        GameEngine.TickAllCovens(s, new WorldLocationService(new HttpClient()));
        Assert.True(s.HomeCoven.Faith > 0);
    }

    [Fact]
    public void CanAfford_ChecksBothResources()
    {
        var s = NewCoven(); s.Faith = 50; s.Gold = 50;
        Assert.True(GameEngine.CanAfford(s, 50, 50));
        Assert.False(GameEngine.CanAfford(s, 51, 50));
        Assert.False(GameEngine.CanAfford(s, 50, 51));
    }

    [Fact]
    public void InitialState_HasHomeCoven()
    {
        var s = NewState();
        Assert.Single(s.Covens);
        Assert.Equal("skanor", s.HomeCoven.Id);
        Assert.True(s.HomeCoven.Converted);
        Assert.Equal("skanor", s.ActiveCovenId);
    }

    [Fact]
    public void OfflineIncome_CalculatesCorrectly()
    {
        var s = NewState();
        s.HomeCoven.Followers = 10;
        s.HomeCoven.Buildings[BuildingType.Shrine] = 2;
        s.LastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 10000;
        var (faith, gold, _, _) = GameEngine.ApplyOfflineIncome(s, 10000);
        Assert.True(faith > 0);
    }

    [Fact]
    public void RankFor_ReturnsCorrectRank()
    {
        Assert.Equal("Novice", GameEngine.RankFor(0).Name);
        Assert.Equal("Novice", GameEngine.RankFor(10).Name);
        Assert.Equal("Adept", GameEngine.RankFor(25).Name);
    }

    private static WorldLocationDef Loc(string id, int req) =>
        new WorldLocationDef(id, id, "", "", "", "", "europe", 0, 0, "", "", req, 1.0, new List<CovenEventData>());

    private static ImmutableArray<WorldLocationDef> Locations(params WorldLocationDef[] locs) =>
        ImmutableArray.Create(locs);

    [Fact]
    public void CanConvert_RequiresFollowers()
    {
        var s = NewState();
        s.HomeCoven.Followers = 149;
        var locs = Locations(Loc("rival", 150));
        Assert.False(CovenProgress.CanConvert(s, locs[0]));
        s.HomeCoven.Followers = 150;
        Assert.True(CovenProgress.CanConvert(s, locs[0]));
    }

    [Fact]
    public void CanConvert_CannotConvertHome()
    {
        var s = NewState();
        var home = Loc("skanor", 0);
        Assert.False(CovenProgress.CanConvert(s, home));
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
        Assert.True(rival!.Converted);
        Assert.Equal(0, rival.Followers);
        Assert.Equal(50, s.HomeCoven.Followers);
        Assert.Equal(100, s.HomeCoven.Faith);
        Assert.Equal(75, s.HomeCoven.Gold);
    }

    [Fact]
    public void NextTarget_ReturnsFirstNotConverted()
    {
        var s = NewState();
        s.HomeCoven.Followers = 30;
        var locs = Locations(Loc("a", 10), Loc("b", 20), Loc("c", 30));
        Assert.Equal("a", CovenProgress.NextTarget(s, locs)!.Id);
        CovenProgress.Takeover(s, locs[0]);
        Assert.Equal("b", CovenProgress.NextTarget(s, locs)!.Id);
    }

    [Fact]
    public void NextTarget_NullWhenAllConverted()
    {
        var s = NewState();
        s.HomeCoven.Followers = 10;
        var locs = Locations(Loc("a", 10));
        CovenProgress.Takeover(s, locs[0]);
        Assert.Null(CovenProgress.NextTarget(s, locs));
    }

    [Fact]
    public void ConversionProgress_FractionThenFull()
    {
        var s = NewState();
        var loc = Loc("rival", 100);
        s.HomeCoven.Followers = 25;
        Assert.Equal(0.25, CovenProgress.ConversionProgress(s, loc), precision: 2);
        s.HomeCoven.Followers = 100;
        Assert.Equal(1.0, CovenProgress.ConversionProgress(s, loc));
    }

    [Fact]
    public void SwitchActive_OnlyToConverted()
    {
        var s = NewState();
        s.Covens.Add(new CovenState { Id = "rival" });
        CovenProgress.SwitchActive(s, "rival");
        Assert.Equal("skanor", s.ActiveCovenId);
        s.FindCoven("rival")!.Converted = true;
        CovenProgress.SwitchActive(s, "rival");
        Assert.Equal("rival", s.ActiveCovenId);
    }

    [Fact]
    public void TotalFollowers_SumsConverted()
    {
        var s = NewState();
        s.HomeCoven.Followers = 30;
        s.Covens.Add(new CovenState { Id = "rival", Converted = true, Followers = 20 });
        Assert.Equal(50, CovenProgress.TotalFollowers(s));
    }

    [Fact]
    public void TakenOverAlias_MapsToConverted()
    {
        var c = new CovenState { Id = "test" };
        c.TakenOver = true;
        Assert.True(c.Converted);
        c.TakenOver = false;
        Assert.False(c.Converted);
    }

    [Fact]
    public void ConversionEngine_StartSetsState()
    {
        var s = NewState();
        var conv = CreateConversions();
        s.HomeCoven.Followers = 30;
        var loc = conv.Find("la_recta_provincia") != null
            ? CreateLocations().Find("la_recta_provincia")!
            : Loc("la_recta_provincia", 25);
        ConversionEngine.StartConversion(s, conv, loc);
        Assert.NotNull(s.Conversion);
        Assert.Equal("la_recta_provincia", s.Conversion!.CovenId);
        Assert.Equal(0, s.Conversion.CurrentStep);
        Assert.False(s.Conversion.Completed);
    }

    [Fact]
    public void ConversionEngine_CanStartRequiresFollowers()
    {
        var s = NewState();
        s.HomeCoven.Followers = 10;
        var loc = Loc("la_recta_provincia", 25);
        Assert.False(ConversionEngine.CanStartConversion(s, loc));
        s.HomeCoven.Followers = 25;
        Assert.True(ConversionEngine.CanStartConversion(s, loc));
    }

    [Fact]
    public void ConversionEngine_IsActiveTrueAfterStart()
    {
        var s = NewState();
        var conv = CreateConversions();
        s.HomeCoven.Followers = 30;
        var loc = CreateLocations().Find("la_recta_provincia")!;
        ConversionEngine.StartConversion(s, conv, loc);
        Assert.True(ConversionEngine.IsActive(s));
    }

    [Fact]
    public void ConversionEngine_ApplyChoiceAdvancesStep()
    {
        var s = NewState();
        var conv = CreateConversions();
        s.HomeCoven.Followers = 30;
        s.HomeCoven.Gold = 200;
        var loc = CreateLocations().Find("la_recta_provincia")!;
        ConversionEngine.StartConversion(s, conv, loc);
        var step = ConversionEngine.CurrentStep(s, conv);
        Assert.NotNull(step);
        ConversionEngine.ApplyChoice(s, conv, step!.ChoiceA);
        Assert.Equal(1, s.Conversion!.CurrentStep);
        Assert.True(s.Conversion.Progress > 0);
    }

    [Fact]
    public void ConversionEngine_FullSequenceEntersBattlePhase()
    {
        var s = NewState();
        var conv = CreateConversions();
        s.HomeCoven.Followers = 100;
        s.HomeCoven.Faith = 500;
        s.HomeCoven.Gold = 500;
        var loc = CreateLocations().Find("la_recta_provincia")!;
        ConversionEngine.StartConversion(s, conv, loc);
        var def = conv.Find("la_recta_provincia");
        Assert.NotNull(def);
        foreach (var step in def!.Steps)
            ConversionEngine.ApplyChoice(s, conv, step.ChoiceA);
        Assert.True(s.Conversion!.BattlePhase);
        Assert.False(s.Conversion.Completed);
    }

    [Fact]
    public void ConversionEngine_CancelClearsState()
    {
        var s = NewState();
        var conv = CreateConversions();
        s.HomeCoven.Followers = 30;
        var loc = CreateLocations().Find("la_recta_provincia")!;
        ConversionEngine.StartConversion(s, conv, loc);
        Assert.NotNull(s.Conversion);
        ConversionEngine.Cancel(s);
        Assert.Null(s.Conversion);
    }

    [Fact]
    public void ConversionEngine_ClearCompletedResetsState()
    {
        var s = NewState();
        var conv = CreateConversions();
        s.HomeCoven.Followers = 100;
        s.HomeCoven.Faith = 500;
        s.HomeCoven.Gold = 500;
        var loc = CreateLocations().Find("la_recta_provincia")!;
        ConversionEngine.StartConversion(s, conv, loc);
        var def = conv.Find("la_recta_provincia");
        foreach (var step in def!.Steps) ConversionEngine.ApplyChoice(s, conv, step.ChoiceA);
        ConversionEngine.OnBattleWon(s, conv);
        Assert.True(s.Conversion!.Completed);
        ConversionEngine.ClearCompleted(s);
        Assert.Null(s.Conversion);
    }

    [Fact]
    public void ConversionData_AllRivalCovensHaveDefinitions()
    {
        var conv = CreateConversions();
        var covenIds = new[] { "la_recta_provincia" };
        foreach (var id in covenIds)
        {
            var def = conv.Find(id);
            Assert.NotNull(def);
            Assert.True(def!.Steps.Count >= 3, $"Coven {id} should have at least 3 steps");
        }
    }

    [Fact]
    public void ConversionData_EachCovenHasUniqueTheme()
    {
        var conv = CreateConversions();
        var themes = conv.All.Select(c => c.Theme).ToList();
        var unique = themes.Distinct().Count();
        Assert.Equal(themes.Count, unique);
    }

    [Fact]
    public void LocalCultData_AllCovensHaveThreeLocalCults()
    {
        var covenIds = new[] { "skanor", "uppsala_gothi", "hedeby_vikings", "trossky_berserkers", "jomsborg_elite", "salem_remnant", "voodoo_quarter", "silicon_circle", "hudson_witches", "montreal_night", "la_recta_provincia", "amazon_curanderos", "andean_pacha", "pantanal_feiticeira", "guarani_shadows", "kush_sorcerers", "ifa_oracles", "dogon_star_priests", "zulu_sangoma", "axum_guardians", "babylon_mages", "djinn_binders", "hashashin_shadow", "sumerian_deep", "qabbalah_masters", "iga_shinobi", "koga_nightblades", "takeda_ronin", "wu_dang_immortals", "shadow_shogun", "maori_tohunga", "dreamtime_elders", "polynesian_navigators", "papuan_spirits", "pacific_abyss" };
        foreach (var id in covenIds)
        {
            var cults = LocalCultData.ForCoven(id);
            Assert.Equal(3, cults.Count);
        }
    }

    [Fact]
    public void LocalCultData_FindReturnsCorrectCult()
    {
        var cult = LocalCultData.Find("falsterbo");
        Assert.NotNull(cult);
        Assert.Equal("skanor", cult!.ParentCovenId);
        Assert.Equal("Falsterbo Heathens", cult.Name);
    }

    [Fact]
    public void LocalCultEngine_SpawnAddsToActiveList()
    {
        var s = NewState();
        s.HomeCoven.Followers = 10;
        LocalCultEngine.SpawnOne(s, "skanor");
        Assert.Single(s.ActiveLocalCults);
    }

    [Fact]
    public void LocalCultEngine_MaxThreeActive()
    {
        var s = NewState();
        s.HomeCoven.Followers = 100;
        LocalCultEngine.SpawnOne(s, "skanor");
        LocalCultEngine.SpawnOne(s, "skanor");
        LocalCultEngine.SpawnOne(s, "skanor");
        Assert.Equal(3, s.ActiveLocalCults.Count);
        LocalCultEngine.SpawnOne(s, "skanor");
        Assert.Equal(3, s.ActiveLocalCults.Count);
    }

    [Fact]
    public void LocalCultEngine_ConvertRequiresFollowers()
    {
        var s = NewState();
        s.HomeCoven.Followers = 5;
        var def = LocalCultData.Find("falsterbo");
        Assert.False(LocalCultEngine.CanConvert(s, def!));
        s.HomeCoven.Followers = 8;
        Assert.True(LocalCultEngine.CanConvert(s, def!));
    }

    [Fact]
    public void LocalCultEngine_ConvertGivesRewardAndRemovesFromMap()
    {
        var s = NewState();
        s.HomeCoven.Followers = 100;
        LocalCultEngine.SpawnOne(s, "skanor");
        Assert.Single(s.ActiveLocalCults);
        var cultId = s.ActiveLocalCults[0].CultId;
        LocalCultEngine.Convert(s, cultId, LocalCultReward.Followers);
        Assert.Empty(s.ActiveLocalCults);
        Assert.True(s.HomeCoven.Followers >= 100);
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
