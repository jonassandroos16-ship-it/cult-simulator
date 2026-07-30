using CultSimulator.Game;

namespace CultSimulator.Tests;

public class OccultTests
{
    private static OccultState NewOccult() => new();
    private static GameState NewState() => GameEngine.InitialState();

    [Fact]
    public void Promote_Requires100Acolytes()
    {
        var o = NewOccult(); o.Acolytes = 99;
        Assert.False(CultistHierarchy.CanPromote(o));
        o.Acolytes = 100;
        Assert.True(CultistHierarchy.CanPromote(o));
    }

    [Fact]
    public void Promote_ConsumesAcolytesAndCreatesMinion()
    {
        var o = NewOccult(); o.Acolytes = 200;
        var minion = CultistHierarchy.Promote(o);
        Assert.Equal(100, o.Acolytes);
        Assert.Single(o.Minions);
        Assert.NotEmpty(minion.Name);
        Assert.NotEmpty(minion.TraitId);
    }

    [Fact]
    public void Sacrifice_ConsumesMinionAndGivesFaith()
    {
        var state = NewState();
        state.Occult.Acolytes = 100;
        var minion = CultistHierarchy.Promote(state.Occult);
        var faith = CultistHierarchy.Sacrifice(state, minion.Id);
        Assert.Empty(state.Occult.Minions);
        Assert.True(faith > 0);
        Assert.True(state.ActiveCoven.Faith > 0);
    }

    [Fact]
    public void Sacrifice_NonexistentMinionDoesNothing()
    {
        var state = NewState();
        var faith = CultistHierarchy.Sacrifice(state, "fake");
        Assert.Equal(0, faith);
    }

    [Fact]
    public void AcolyteCap_BaseIs200()
    {
        var o = NewOccult();
        Assert.Equal(200, CultistHierarchy.AcolyteCap(o));
    }

    [Fact]
    public void AppointCouncil_RequiresMinion()
    {
        var state = NewState();
        Assert.False(CultistHierarchy.CanAppointCouncil(state, CouncilRole.Archon));
        state.Occult.Acolytes = 100;
        CultistHierarchy.Promote(state.Occult);
        Assert.True(CultistHierarchy.CanAppointCouncil(state, CouncilRole.Archon));
    }

    [Fact]
    public void AppointCouncil_ConsumesMinion()
    {
        var state = NewState(); state.Occult.Acolytes = 100;
        var minion = CultistHierarchy.Promote(state.Occult);
        CultistHierarchy.AppointCouncil(state, CouncilRole.Archon, minion.Id);
        Assert.Empty(state.Occult.Minions);
        Assert.Single(state.Occult.HighCouncil);
    }

    [Fact]
    public void Tech_PrerequisitesEnforced()
    {
        var state = NewState();
        Assert.False(TechTree.CanUnlock(state, OccultData.Tech(TechId.OsmoticExtraction)));
        state.Occult.UnlockedTechs.Add(TechId.SanguineAutomata);
        state.ActiveCoven.Faith = 500;
        Assert.True(TechTree.CanUnlock(state, OccultData.Tech(TechId.OsmoticExtraction)));
    }

    [Fact]
    public void Tech_UnlockConsumesFaith()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 100;
        Assert.True(TechTree.Unlock(state, TechId.SanguineAutomata));
        Assert.Equal(50, state.ActiveCoven.Faith);
        Assert.Contains(TechId.SanguineAutomata, state.Occult.UnlockedTechs);
    }

    [Fact]
    public void Tech_CannotUnlockAlreadyOwned()
    {
        var state = NewState();
        state.Occult.UnlockedTechs.Add(TechId.SanguineAutomata);
        Assert.False(TechTree.CanUnlock(state, OccultData.Tech(TechId.SanguineAutomata)));
    }

    [Fact]
    public void Conquer_RequiresFaithAndArmy()
    {
        var state = NewState();
        Assert.False(WorldMapSystem.CanConquer(state, OccultData.MapNode("skanor_runestone")));
        state.ActiveCoven.Faith = 150; state.Occult.ArmyPower = 10;
        Assert.True(WorldMapSystem.CanConquer(state, OccultData.MapNode("skanor_runestone")));
    }

    [Fact]
    public void Conquer_SpendsResources()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 1000; state.Occult.ArmyPower = 100;
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        Assert.Equal(850, state.ActiveCoven.Faith);
        Assert.Equal(90, state.Occult.ArmyPower);
        Assert.True(WorldMapSystem.IsConquered(state.Occult, "skanor_runestone"));
    }

    [Fact]
    public void SetStance_ChangesNodeStance()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 1000; state.Occult.ArmyPower = 100;
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        WorldMapSystem.SetStance(state.Occult, "skanor_runestone", NodeStance.Veil);
        Assert.Equal(NodeStance.Veil, WorldMapSystem.GetNode(state.Occult, "skanor_runestone")!.Stance);
    }

    [Fact]
    public void VeilMode_GeneratesZeroSuspicion()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 1000; state.Occult.ArmyPower = 100;
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        WorldMapSystem.SetStance(state.Occult, "skanor_runestone", NodeStance.Veil);
        Assert.Equal(0, WorldMapSystem.TotalSuspicionPerSec(state.Occult));
    }

    [Fact]
    public void Suspicion_ClampedAtMax()
    {
        var state = NewState();
        state.Occult.Suspicion = 99;
        state.ActiveCoven.Faith = 100000; state.Occult.ArmyPower = 10000;
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        WorldMapSystem.TickSuspicion(state.Occult, 100);
        Assert.Equal(OccultBalance.SuspicionMax, state.Occult.Suspicion);
    }

    [Fact]
    public void RaidTriggered_At80Percent()
    {
        var o = NewOccult(); o.Suspicion = 79;
        Assert.False(WorldMapSystem.IsRaidTriggered(o));
        o.Suspicion = 80;
        Assert.True(WorldMapSystem.IsRaidTriggered(o));
    }

    [Fact]
    public void ApplyRaid_ResetsSuspicionAndKillsAcolytes()
    {
        var o = NewOccult(); o.Suspicion = 90; o.Acolytes = 100;
        WorldMapSystem.ApplyRaid(o);
        Assert.Equal(0, o.Suspicion);
        Assert.True(o.Acolytes < 100);
    }

    [Fact]
    public void LeyLine_RequiresTwoConqueredNodes()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 100000; state.Occult.ArmyPower = 10000;
        Assert.False(WorldMapSystem.CanConnectLeyLine(state.Occult, "skanor_runestone", "skanor_bog"));
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        Assert.False(WorldMapSystem.CanConnectLeyLine(state.Occult, "skanor_runestone", "skanor_bog"));
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_bog"));
        Assert.True(WorldMapSystem.CanConnectLeyLine(state.Occult, "skanor_runestone", "skanor_bog"));
    }

    [Fact]
    public void Cauldron_LockedWithoutTech()
    {
        var o = NewOccult();
        Assert.False(Cauldron.IsUnlocked(o));
        o.UnlockedTechs.Add(TechId.TransmutationCrucible);
        Assert.True(Cauldron.IsUnlocked(o));
    }

    [Fact]
    public void Craft_ElixirConsumesMaterials()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.TransmutationCrucible);
        o.Materials[MaterialKind.GraveDust] = 10;
        var (success, _) = Cauldron.Craft(o, CauldronRecipeId.CrimsonElixir);
        Assert.True(success);
        Assert.Equal(7, o.Materials[MaterialKind.GraveDust]);
        Assert.Equal(2.0, o.ElixirTapMult);
        Assert.True(o.ElixirTimer > 0);
    }

    [Fact]
    public void Craft_InsufficientMaterialsFails()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.TransmutationCrucible);
        o.Materials[MaterialKind.GraveDust] = 2;
        var (success, _) = Cauldron.Craft(o, CauldronRecipeId.CrimsonElixir);
        Assert.False(success);
        Assert.Equal(2, o.Materials[MaterialKind.GraveDust]);
    }

    [Fact]
    public void Craft_ForgeProducesArtifact()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.TransmutationCrucible);
        o.Materials[MaterialKind.GraveDust] = 10;
        o.Materials[MaterialKind.DemonBile] = 5;
        var (success, artifactId) = Cauldron.Craft(o, CauldronRecipeId.BloodForge);
        Assert.True(success);
        Assert.NotNull(artifactId);
        Assert.True(Grimoire.OwnsArtifact(o, artifactId!));
    }

    [Fact]
    public void Elixir_ExpiresAfterDuration()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.TransmutationCrucible);
        o.Materials[MaterialKind.GraveDust] = 10;
        Cauldron.Craft(o, CauldronRecipeId.CrimsonElixir);
        Assert.Equal(2.0, o.ElixirTapMult);
        Cauldron.TickElixir(o, OccultBalance.ElixirDurationSec + 1);
        Assert.Equal(0, o.ElixirTimer);
        Assert.Equal(1.0, o.ElixirTapMult);
    }

    [Fact]
    public void Favor_ZeroBelowThreshold()
    {
        var state = NewState();
        state.Occult.LifetimeFaith = 999999;
        Assert.Equal(0, GrandSacrifice.CalculateFavor(state));
    }

    [Fact]
    public void Favor_CalculatedFromLifetimeFaith()
    {
        var state = NewState();
        state.Occult.LifetimeFaith = 4_000_000;
        Assert.Equal(2.0, GrandSacrifice.CalculateFavor(state));
    }

    [Fact]
    public void Favor_ContinentMultiplier()
    {
        var state = NewState();
        state.Occult.LifetimeFaith = 4_000_000;
        state.ActiveCoven.Faith = 100000; state.Occult.ArmyPower = 10000;
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        Assert.Equal(2.0, Math.Floor(GrandSacrifice.CalculateFavor(state)));
    }

    [Fact]
    public void PerformSacrifice_GrantsFavorAndResets()
    {
        var state = NewState();
        state.Occult.LifetimeFaith = 4_000_000;
        state.ActiveCoven.Faith = 500;
        state.Occult.Acolytes = 50;
        var favor = GrandSacrifice.PerformSacrifice(state);
        Assert.True(favor >= 1);
        Assert.Equal(favor, state.EldritchFavor);
        Assert.Equal(0, state.Occult.Acolytes);
    }

    [Fact]
    public void PerformSacrifice_MemoriesRetainsFaith()
    {
        var state = NewState();
        state.Occult.LifetimeFaith = 4_000_000;
        state.ActiveCoven.Faith = 10000;
        state.Occult.UnlockedTechs.Add(TechId.MemoriesOfTheDeep);
        GrandSacrifice.PerformSacrifice(state);
        Assert.Equal(1000, state.ActiveCoven.Faith);
    }

    [Fact]
    public void PerformSacrifice_AstralAnchorKeepsHighPriest()
    {
        var state = NewState();
        state.Occult.LifetimeFaith = 4_000_000;
        state.Occult.UnlockedTechs.Add(TechId.AstralAnchor);
        state.GrandSacrificeCount = 1;
        state.Occult.Acolytes = 100;
        var minion = CultistHierarchy.Promote(state.Occult);
        CultistHierarchy.AppointCouncil(state, CouncilRole.HighPriest, minion.Id);
        GrandSacrifice.PerformSacrifice(state);
        Assert.Contains(state.Occult.HighCouncil, c => c.Role == CouncilRole.HighPriest);
        Assert.Contains(TechId.AstralAnchor, state.Occult.UnlockedTechs);
    }

    [Fact]
    public void GlobalProductionMult_ScalesWithFavor()
    {
        var state = NewState();
        Assert.Equal(1.0, GrandSacrifice.GlobalProductionMult(state));
        state.EldritchFavor = 10;
        Assert.Equal(1.2, GrandSacrifice.GlobalProductionMult(state));
    }

    [Fact]
    public void Tap_GeneratesFaith()
    {
        var state = NewState();
        var gained = OccultEngine.Tap(state);
        Assert.True(gained > 0);
        Assert.True(state.ActiveCoven.Faith > 0);
        Assert.True(state.Occult.LifetimeFaith > 0);
    }

    [Fact]
    public void BuySermonPower_IncreasesLevel()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 500;
        Assert.True(OccultEngine.BuySermonPower(state));
        Assert.Equal(1, state.Occult.SermonPowerLevel);
        Assert.True(state.ActiveCoven.Faith < 500);
    }

    [Fact]
    public void HireAcolyte_IncreasesCount()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 500;
        Assert.True(OccultEngine.HireAcolyte(state));
        Assert.Equal(1, state.Occult.Acolytes);
    }

    [Fact]
    public void HireAcolyte_Capped()
    {
        var state = NewState();
        state.Occult.Acolytes = 200;
        Assert.False(OccultEngine.CanHireAcolyte(state));
    }

    [Fact]
    public void Tick_GeneratesFaith()
    {
        var state = NewState();
        state.Occult.Acolytes = 10;
        var faithBefore = state.ActiveCoven.Faith;
        OccultEngine.Tick(state, 1.0);
        Assert.True(state.ActiveCoven.Faith > faithBefore);
        state.ActiveCoven.Faith = 10000; state.Occult.ArmyPower = 1000;
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        var mapFaithBefore = state.ActiveCoven.Faith;
        OccultEngine.Tick(state, 1.0);
        Assert.True(state.ActiveCoven.Faith > mapFaithBefore);
    }

    [Fact]
    public void Frenzy_RequiresTechAndMinion()
    {
        var o = NewOccult();
        Assert.False(OccultEngine.CanActivateFrenzy(o));
        o.UnlockedTechs.Add(TechId.ExsanguinationEngine);
        Assert.False(OccultEngine.CanActivateFrenzy(o));
        o.Acolytes = 100; CultistHierarchy.Promote(o);
        Assert.True(OccultEngine.CanActivateFrenzy(o));
    }

    [Fact]
    public void Frenzy_ConsumesMinionAndSetsTimer()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.ExsanguinationEngine);
        o.Acolytes = 100; CultistHierarchy.Promote(o);
        OccultEngine.ActivateFrenzy(o);
        Assert.Empty(o.Minions);
        Assert.True(o.FrenzyTimer > 0);
        Assert.True(o.IsFrenzyActive);
    }

    [Fact]
    public void Frenzy_MultipliesClickPower()
    {
        var state = NewState();
        var basePower = OccultEngine.ClickPower(state);
        state.Occult.UnlockedTechs.Add(TechId.ExsanguinationEngine);
        state.Occult.Acolytes = 100; CultistHierarchy.Promote(state.Occult);
        OccultEngine.ActivateFrenzy(state.Occult);
        Assert.True(OccultEngine.ClickPower(state) > basePower * 5);
    }

    [Fact]
    public void MassHysteria_RequiresTech()
    {
        var o = NewOccult();
        Assert.False(OccultEngine.CanActivateMassHysteria(o));
        o.UnlockedTechs.Add(TechId.MassHysteria);
        Assert.True(OccultEngine.CanActivateMassHysteria(o));
    }

    [Fact]
    public void MassHysteria_DoublesFaith()
    {
        var state = NewState();
        state.Occult.Acolytes = 10;
        state.ActiveCoven.Faith = 10000; state.Occult.ArmyPower = 1000;
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        var baseFaith = OccultEngine.TotalMapFaithPerSec(state);
        state.Occult.UnlockedTechs.Add(TechId.MassHysteria);
        OccultEngine.ActivateMassHysteria(state.Occult);
        Assert.True(OccultEngine.TotalMapFaithPerSec(state) > baseFaith * 1.5);
    }

    [Fact]
    public void SaveLoad_PreservesOccultState()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 500; state.Occult.LifetimeFaith = 10000;
        state.EldritchFavor = 10; state.Occult.Acolytes = 50;
        state.Occult.UnlockedTechs.Add(TechId.SanguineAutomata);
        var loaded = SaveLoad.LoadGame(SaveLoad.SaveGame(state));
        Assert.Equal(500, loaded.ActiveCoven.Faith);
        Assert.Equal(10000, loaded.Occult.LifetimeFaith);
        Assert.Equal(10, loaded.EldritchFavor);
        Assert.Equal(50, loaded.Occult.Acolytes);
        Assert.Contains(TechId.SanguineAutomata, loaded.Occult.UnlockedTechs);
    }

    [Fact]
    public void LoadGame_MigratesMissingOccultState()
    {
        var loaded = SaveLoad.LoadGame("{\"CultName\":\"Old\",\"StartedAt\":1000,\"StoryShown\":false,\"ActiveCovenId\":\"skanor\",\"Covens\":[]}");
        Assert.NotNull(loaded.Occult);
        Assert.Equal(0, loaded.Occult.LifetimeFaith);
        Assert.Empty(loaded.Occult.UnlockedTechs);
    }

    [Fact]
    public void OccultState_IsPerCoven()
    {
        var state = NewState();
        state.Occult.Acolytes = 50;
        state.Covens.Add(new CovenState { Id = "test_coven", Converted = true, Occult = new OccultState { Acolytes = 200 } });
        Assert.Equal(50, state.Covens.First(c => c.Id == "skanor").Occult.Acolytes);
        Assert.Equal(200, state.Covens.First(c => c.Id == "test_coven").Occult.Acolytes);
        state.ActiveCovenId = "test_coven";
        Assert.Equal(200, state.Occult.Acolytes);
        state.ActiveCovenId = "skanor";
        Assert.Equal(50, state.Occult.Acolytes);
    }

    [Fact]
    public void TickAllCovens_TicksEachCovenOccult()
    {
        var state = NewState();
        state.Occult.Acolytes = 10;
        state.Covens.Add(new CovenState { Id = "test_coven", Converted = true, Occult = new OccultState { Acolytes = 20 } });
        var homeFaithBefore = state.Covens.First(c => c.Id == "skanor").Faith;
        var testFaithBefore = state.Covens.First(c => c.Id == "test_coven").Faith;
        GameEngine.TickAllCovens(state);
        Assert.True(state.Covens.First(c => c.Id == "skanor").Faith > homeFaithBefore);
        Assert.True(state.Covens.First(c => c.Id == "test_coven").Faith > testFaithBefore);
    }

    [Fact]
    public void GrandSacrifice_SumsLifetimeFaithAcrossCovens()
    {
        var state = NewState();
        state.Covens.Add(new CovenState { Id = "test_coven", Converted = true, Occult = new OccultState { LifetimeFaith = 2_000_000 } });
        state.Occult.LifetimeFaith = 2_000_000;
        Assert.Equal(2.0, GrandSacrifice.CalculateFavor(state));
    }
}
