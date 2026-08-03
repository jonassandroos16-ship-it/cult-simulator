using CultSimulator.Game;

namespace CultSimulator.Tests;

public class OccultTests
{
    private static OccultState NewOccult() => new();
    private static GameState NewState() => GameEngine.InitialState();

    [Fact]
    public void Promote_Requires100Initiates()
    {
        var o = NewOccult(); o.Initiates = 99;
        Assert.False(CultistHierarchy.CanRecruitUnit(o));
        o.Initiates = 100;
        Assert.True(CultistHierarchy.CanRecruitUnit(o));
    }

    [Fact]
    public void Promote_ConsumesInitiatesAndCreatesMinion()
    {
        var o = NewOccult(); o.Initiates = 200;
        var minion = CultistHierarchy.Promote(o);
        Assert.Equal(100, o.Initiates);
        Assert.Single(o.Minions);
        Assert.NotEmpty(minion.Name);
        Assert.NotEmpty(minion.TraitId);
    }

    [Fact]
    public void RecruitUnit_CreatesChosenRole()
    {
        var o = NewOccult(); o.Initiates = 300;
        var zealot = CultistHierarchy.RecruitUnit(o, PromotedRole.Zealot);
        Assert.NotNull(zealot);
        Assert.Equal(PromotedRole.Zealot, zealot!.Role);
        Assert.Equal(180, o.Initiates);
        var scholar = CultistHierarchy.RecruitUnit(o, PromotedRole.Scholar);
        Assert.Equal(PromotedRole.Scholar, scholar!.Role);
    }

    [Fact]
    public void Sacrifice_ConsumesMinionAndGivesFaith()
    {
        var state = NewState();
        state.Occult.Initiates = 100;
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
    public void InitiateCap_BaseIs200()
    {
        var o = NewOccult();
        Assert.Equal(200, CultistHierarchy.InitiateCap(o));
    }

    [Fact]
    public void AppointCouncil_RequiresMinion()
    {
        var o = NewOccult();
        Assert.False(CultistHierarchy.CanAppointCouncil(o, CouncilRole.Archon));
        o.Initiates = 100;
        CultistHierarchy.Promote(o);
        Assert.True(CultistHierarchy.CanAppointCouncil(o, CouncilRole.Archon));
    }

    [Fact]
    public void AppointCouncil_ConsumesMinion()
    {
        var o = NewOccult(); o.Initiates = 100;
        var minion = CultistHierarchy.Promote(o);
        CultistHierarchy.AppointCouncil(o, CouncilRole.Archon, minion.Id);
        Assert.Empty(o.Minions);
        Assert.Single(o.HighCouncil);
    }

    [Fact]
    public void Tech_PrerequisitesEnforced()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 100000;
        Assert.False(TechTree.CanUnlock(state, OccultData.Tech(TechId.OsmoticExtraction)));
        state.Occult.UnlockedTechs.Add(TechId.SanguineAutomata);
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
        state.ActiveCoven.Faith = 100000;
        state.Occult.UnlockedTechs.Add(TechId.SanguineAutomata);
        Assert.False(TechTree.CanUnlock(state, OccultData.Tech(TechId.SanguineAutomata)));
    }

    [Fact]
    public void Conquer_RequiresFaith()
    {
        var state = NewState();
        Assert.False(WorldMapSystem.CanConquer(state, OccultData.MapNode("skanor_runestone")));
        state.ActiveCoven.Faith = 150;
        Assert.True(WorldMapSystem.CanConquer(state, OccultData.MapNode("skanor_runestone")));
    }

    [Fact]
    public void Conquer_SpendsFaith()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 1000;
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        Assert.Equal(850, state.ActiveCoven.Faith);
        Assert.True(WorldMapSystem.IsConquered(state.Occult, "skanor_runestone"));
    }

    [Fact]
    public void SetStance_ChangesNodeStance()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 1000;
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        WorldMapSystem.SetStance(state.Occult, "skanor_runestone", NodeStance.Veil);
        Assert.Equal(NodeStance.Veil, WorldMapSystem.GetNode(state.Occult, "skanor_runestone")!.Stance);
    }

    [Fact]
    public void Conquer_SpendsFaithAndGeneratesIncome()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 1000;
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        Assert.Equal(850, state.ActiveCoven.Faith);
        Assert.True(WorldMapSystem.IsConquered(state.Occult, "skanor_runestone"));
        Assert.True(WorldMapSystem.TotalFaithPerSec(state.Occult) > 0);
    }

    [Fact]
    public void LeyLine_RequiresTwoConqueredNodes()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 100000;
        Assert.False(WorldMapSystem.IsConquered(state.Occult, "skanor_runestone"));
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        Assert.True(WorldMapSystem.IsConquered(state.Occult, "skanor_runestone"));
        Assert.False(WorldMapSystem.IsConquered(state.Occult, "skanor_bog"));
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_bog"));
        Assert.True(WorldMapSystem.IsConquered(state.Occult, "skanor_bog"));
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
        var state = NewState();
        state.Occult.UnlockedTechs.Add(TechId.TransmutationCrucible);
        var sw = ShadowWarEngine.EnsureInitialized(state);
        var recipe = OccultData.Recipe(CauldronRecipeId.CrimsonElixir);
        sw.TotalAgents = recipe.AgentCost + 10;
        var (success, _) = Cauldron.Craft(state, CauldronRecipeId.CrimsonElixir);
        Assert.True(success);
        Assert.Equal(2.0, state.Occult.ElixirTapMult);
        Assert.True(state.Occult.ElixirTimer > 0);
    }

    [Fact]
    public void Craft_InsufficientMaterialsFails()
    {
        var state = NewState();
        state.Occult.UnlockedTechs.Add(TechId.TransmutationCrucible);
        var sw = ShadowWarEngine.EnsureInitialized(state);
        sw.TotalAgents = 0;
        var (success, _) = Cauldron.Craft(state, CauldronRecipeId.CrimsonElixir);
        Assert.False(success);
    }

    [Fact]
    public void Craft_ForgeProducesArtifact()
    {
        var state = NewState();
        state.Occult.UnlockedTechs.Add(TechId.TransmutationCrucible);
        var sw = ShadowWarEngine.EnsureInitialized(state);
        var recipe = OccultData.Recipe(CauldronRecipeId.BloodForge);
        sw.TotalAgents = recipe.AgentCost + 10;
        var (success, artifactId) = Cauldron.Craft(state, CauldronRecipeId.BloodForge);
        Assert.True(success);
        Assert.NotNull(artifactId);
        Assert.True(Grimoire.OwnsArtifact(state.Occult, artifactId!));
    }

    [Fact]
    public void Elixir_ExpiresAfterDuration()
    {
        var state = NewState();
        state.Occult.UnlockedTechs.Add(TechId.TransmutationCrucible);
        var sw = ShadowWarEngine.EnsureInitialized(state);
        var recipe = OccultData.Recipe(CauldronRecipeId.CrimsonElixir);
        sw.TotalAgents = recipe.AgentCost + 10;
        Cauldron.Craft(state, CauldronRecipeId.CrimsonElixir);
        Assert.Equal(2.0, state.Occult.ElixirTapMult);
        Cauldron.TickElixir(state.Occult, OccultBalance.ElixirDurationSec + 1);
        Assert.Equal(0, state.Occult.ElixirTimer);
        Assert.Equal(1.0, state.Occult.ElixirTapMult);
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
        state.ActiveCoven.Faith = 100000;
        WorldMapSystem.Conquer(state, OccultData.MapNode("skanor_runestone"));
        Assert.Equal(2.0, Math.Floor(GrandSacrifice.CalculateFavor(state)));
    }

    [Fact]
    public void PerformSacrifice_GrantsFavorAndResets()
    {
        var state = NewState();
        state.ActiveCoven.TakenOver = true;
        state.Occult.LifetimeFaith = 4_000_000;
        state.ActiveCoven.Faith = 500;
        state.Occult.Initiates = 50;
        var favor = GrandSacrifice.PerformSacrifice(state);
        Assert.True(favor >= 1);
        Assert.Equal(favor, state.EldritchFavor);
        Assert.Equal(0, state.Occult.Initiates);
        Assert.Single(state.Covens);
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
        state.Occult.GrandSacrificeCount = 1;
        state.Occult.Initiates = 100;
        var minion = CultistHierarchy.Promote(state.Occult);
        CultistHierarchy.AppointCouncil(state.Occult, CouncilRole.HighPriest, minion.Id);
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
    public void HireInitiate_IncreasesCount()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 500;
        Assert.True(OccultEngine.HireInitiate(state));
        Assert.Equal(1, state.Occult.Initiates);
    }

    [Fact]
    public void HireInitiate_Capped()
    {
        var state = NewState();
        state.Occult.Initiates = 200;
        Assert.False(OccultEngine.CanHireInitiate(state));
    }

    [Fact]
    public void Tick_GeneratesFaith()
    {
        var state = NewState();
        state.Occult.Initiates = 10;
        var faithBefore = state.ActiveCoven.Faith;
        OccultEngine.Tick(state, 1.0);
        Assert.True(state.ActiveCoven.Faith > faithBefore);
        state.ActiveCoven.Faith = 10000;
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
        o.Initiates = 100; CultistHierarchy.Promote(o);
        Assert.True(OccultEngine.CanActivateFrenzy(o));
    }

    [Fact]
    public void Frenzy_ConsumesMinionAndSetsTimer()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.ExsanguinationEngine);
        o.Initiates = 100; CultistHierarchy.Promote(o);
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
        state.Occult.Initiates = 100; CultistHierarchy.Promote(state.Occult);
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
        state.Occult.Initiates = 10;
        state.ActiveCoven.Faith = 10000;
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
        state.CultName = "Test";
        state.ActiveCoven.Faith = 500; state.Occult.LifetimeFaith = 10000;
        state.Occult.EldritchFavor = 10; state.Occult.Initiates = 50;
        state.Occult.UnlockedTechs.Add(TechId.SanguineAutomata);
        var loaded = SaveLoad.LoadGameWithBackup(SaveLoad.SaveGame(state), null, null).state;
        Assert.Equal(500, loaded.ActiveCoven.Faith);
        Assert.Equal(10000, loaded.Occult.LifetimeFaith);
        Assert.Equal(10, loaded.Occult.EldritchFavor);
        Assert.Equal(50, loaded.Occult.Initiates);
        Assert.Contains(TechId.SanguineAutomata, loaded.Occult.UnlockedTechs);
    }

    [Fact]
    public void LoadGame_MigratesMissingOccultState()
    {
        var loaded = SaveLoad.LoadGameWithBackup("{\"CultName\":\"Old\",\"StartedAt\":1000,\"StoryShown\":false,\"ActiveCovenId\":\"skanor\",\"Covens\":[{\"Id\":\"skanor\",\"Converted\":true}]}", null, null).state;
        Assert.NotNull(loaded.Occult);
        Assert.Equal(0, loaded.Occult.LifetimeFaith);
        Assert.Empty(loaded.Occult.UnlockedTechs);
    }
}
