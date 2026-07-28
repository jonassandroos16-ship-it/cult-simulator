using CultSimulator.Game;

namespace CultSimulator.Tests;

public class OccultTests
{
    private static OccultState NewOccult() => new();
    private static GameState NewState() => GameEngine.InitialState();

    // --- Cultist Hierarchy ---

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
    public void Sacrifice_ConsumesMinionAndGivesResources()
    {
        var o = NewOccult(); o.Acolytes = 100;
        var minion = CultistHierarchy.Promote(o);
        var (devotion, fk) = CultistHierarchy.Sacrifice(o, minion.Id);
        Assert.Empty(o.Minions);
        Assert.True(devotion > 0);
        Assert.True(fk > 0);
        Assert.True(o.ForbiddenKnowledge > 0);
    }

    [Fact]
    public void Sacrifice_NonexistentMinionDoesNothing()
    {
        var o = NewOccult();
        var (devotion, fk) = CultistHierarchy.Sacrifice(o, "fake");
        Assert.Equal(0, devotion);
        Assert.Equal(0, fk);
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
        var o = NewOccult();
        Assert.False(CultistHierarchy.CanAppointCouncil(o, CouncilRole.Archon));
        o.Acolytes = 100;
        CultistHierarchy.Promote(o);
        Assert.True(CultistHierarchy.CanAppointCouncil(o, CouncilRole.Archon));
    }

    [Fact]
    public void AppointCouncil_ConsumesMinion()
    {
        var o = NewOccult(); o.Acolytes = 100;
        var minion = CultistHierarchy.Promote(o);
        CultistHierarchy.AppointCouncil(o, CouncilRole.Inquisitor, minion.Id);
        Assert.Empty(o.Minions);
        Assert.Single(o.HighCouncil);
    }

    [Fact]
    public void HighPriest_RequiresGrandSacrifice()
    {
        var o = NewOccult(); o.Acolytes = 100;
        var minion = CultistHierarchy.Promote(o);
        Assert.False(CultistHierarchy.CanAppointCouncil(o, CouncilRole.HighPriest));
        o.GrandSacrificeCount = 1;
        Assert.True(CultistHierarchy.CanAppointCouncil(o, CouncilRole.HighPriest));
    }

    // --- Tech Tree ---

    [Fact]
    public void Tech_PrerequisitesBlockUnlock()
    {
        var o = NewOccult(); o.ForbiddenKnowledge = 100000;
        var osmotic = OccultData.Tech(TechId.OsmoticExtraction);
        Assert.False(TechTree.CanUnlock(o, osmotic));
    }

    [Fact]
    public void Tech_UnlockSpendsFk()
    {
        var o = NewOccult(); o.ForbiddenKnowledge = 100;
        var sanguine = OccultData.Tech(TechId.SanguineAutomata);
        Assert.True(TechTree.CanUnlock(o, sanguine));
        TechTree.Unlock(o, TechId.SanguineAutomata);
        Assert.Equal(50, o.ForbiddenKnowledge);
        Assert.Contains(TechId.SanguineAutomata, o.UnlockedTechs);
    }

    [Fact]
    public void Tech_SequentialPrereqsWork()
    {
        var o = NewOccult(); o.ForbiddenKnowledge = 100000;
        TechTree.Unlock(o, TechId.SanguineAutomata);
        Assert.True(TechTree.PrerequisitesMet(o, OccultData.Tech(TechId.OsmoticExtraction)));
        Assert.False(TechTree.PrerequisitesMet(o, OccultData.Tech(TechId.AutophagousCult)));
    }

    [Fact]
    public void Tech_WhispersReducesSuspicion()
    {
        var o = NewOccult();
        Assert.Equal(1.0, TechTree.SuspicionReductionMult(o));
        o.UnlockedTechs.Add(TechId.WhispersInTheDark);
        Assert.Equal(0.85, TechTree.SuspicionReductionMult(o));
    }

    [Fact]
    public void Tech_ResonanceMasteryDoublesSetBonus()
    {
        var o = NewOccult();
        Assert.Equal(1.0, TechTree.SetBonusMult(o));
        o.UnlockedTechs.Add(TechId.ResonanceMastery);
        Assert.Equal(2.0, TechTree.SetBonusMult(o));
    }

    [Fact]
    public void Tech_MemoriesRetainsFk()
    {
        var o = NewOccult();
        Assert.Equal(0.0, TechTree.FkRetentionPercent(o));
        o.UnlockedTechs.Add(TechId.MemoriesOfTheDeep);
        Assert.Equal(0.10, TechTree.FkRetentionPercent(o));
    }

    // --- Grimoire ---

    [Fact]
    public void Socket_BaseCountIs1()
    {
        var o = NewOccult();
        Assert.Equal(1, o.UnlockedSocketCount);
    }

    [Fact]
    public void Socket_SecondSocketTechIncreasesCount()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.SecondSocket);
        Assert.Equal(2, o.UnlockedSocketCount);
        o.UnlockedTechs.Add(TechId.ThirdSocket);
        Assert.Equal(3, o.UnlockedSocketCount);
    }

    [Fact]
    public void Socket_AddsArtifact()
    {
        var o = NewOccult();
        Grimoire.AddArtifact(o, "blood_chalice");
        Assert.True(Grimoire.OwnsArtifact(o, "blood_chalice"));
        Grimoire.Socket(o, "blood_chalice");
        Assert.Contains("blood_chalice", o.SocketedArtifacts);
        Assert.False(o.OwnedArtifacts.Contains("blood_chalice"));
    }

    [Fact]
    public void Socket_CannotExceedLimit()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.SecondSocket);
        Grimoire.AddArtifact(o, "blood_chalice");
        Grimoire.AddArtifact(o, "blood_blade");
        Assert.True(Grimoire.Socket(o, "blood_chalice"));
        Assert.True(Grimoire.Socket(o, "blood_blade"));
        Grimoire.AddArtifact(o, "blood_heart");
        Assert.False(Grimoire.Socket(o, "blood_heart"));
    }

    [Fact]
    public void Unsocket_ReturnsToInventory()
    {
        var o = NewOccult();
        Grimoire.AddArtifact(o, "blood_chalice");
        Grimoire.Socket(o, "blood_chalice");
        Grimoire.Unsocket(o, "blood_chalice");
        Assert.DoesNotContain("blood_chalice", o.SocketedArtifacts);
        Assert.Contains("blood_chalice", o.OwnedArtifacts);
    }

    [Fact]
    public void BloodSetBonus_Requires3Blood()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.SecondSocket);
        o.UnlockedTechs.Add(TechId.ThirdSocket);
        Grimoire.AddArtifact(o, "blood_chalice");
        Grimoire.AddArtifact(o, "blood_blade");
        Grimoire.AddArtifact(o, "blood_heart");
        Grimoire.Socket(o, "blood_chalice");
        Grimoire.Socket(o, "blood_blade");
        Assert.False(Grimoire.HasSetBonus(o, ArtifactSuit.Blood));
        Grimoire.Socket(o, "blood_heart");
        Assert.True(Grimoire.HasSetBonus(o, ArtifactSuit.Blood));
    }

    [Fact]
    public void BloodSetBonus_IncreasesTapPower()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.SecondSocket);
        o.UnlockedTechs.Add(TechId.ThirdSocket);
        Assert.Equal(1.0, Grimoire.TapPowerBonus(o));
        Grimoire.AddArtifact(o, "blood_chalice");
        Grimoire.AddArtifact(o, "blood_blade");
        Grimoire.AddArtifact(o, "blood_heart");
        Grimoire.Socket(o, "blood_chalice");
        Grimoire.Socket(o, "blood_blade");
        Grimoire.Socket(o, "blood_heart");
        Assert.Equal(3.15, Grimoire.TapPowerBonus(o), precision: 2);
    }

    [Fact]
    public void BloodVoidConversion_Requires2Blood1Void()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.SecondSocket);
        o.UnlockedTechs.Add(TechId.ThirdSocket);
        Assert.False(Grimoire.BloodVoidConversionActive(o));
        Grimoire.AddArtifact(o, "blood_chalice");
        Grimoire.AddArtifact(o, "blood_blade");
        Grimoire.AddArtifact(o, "void_orb");
        Grimoire.Socket(o, "blood_chalice");
        Grimoire.Socket(o, "blood_blade");
        Grimoire.Socket(o, "void_orb");
        Assert.True(Grimoire.BloodVoidConversionActive(o));
    }

    // --- World Map ---

    [Fact]
    public void Conquer_RequiresDevotionAndArmy()
    {
        var o = NewOccult();
        var node = OccultData.MapNode("old_library");
        Assert.False(WorldMapSystem.CanConquer(o, node));
        o.Devotion = 500; o.ArmyPower = 50;
        Assert.True(WorldMapSystem.CanConquer(o, node));
    }

    [Fact]
    public void Conquer_SpendsResources()
    {
        var o = NewOccult(); o.Devotion = 1000; o.ArmyPower = 100;
        var node = OccultData.MapNode("old_library");
        WorldMapSystem.Conquer(o, node);
        Assert.Equal(500, o.Devotion);
        Assert.Equal(50, o.ArmyPower);
        Assert.True(WorldMapSystem.IsConquered(o, "old_library"));
    }

    [Fact]
    public void SetStance_ChangesNodeStance()
    {
        var o = NewOccult(); o.Devotion = 1000; o.ArmyPower = 100;
        var node = OccultData.MapNode("old_library");
        WorldMapSystem.Conquer(o, node);
        WorldMapSystem.SetStance(o, "old_library", NodeStance.Veil);
        Assert.Equal(NodeStance.Veil, WorldMapSystem.GetNode(o, "old_library")!.Stance);
    }

    [Fact]
    public void VeilMode_GeneratesZeroSuspicion()
    {
        var o = NewOccult(); o.Devotion = 1000; o.ArmyPower = 100;
        var node = OccultData.MapNode("old_library");
        WorldMapSystem.Conquer(o, node);
        WorldMapSystem.SetStance(o, "old_library", NodeStance.Veil);
        Assert.Equal(0, WorldMapSystem.TotalSuspicionPerSec(o));
    }

    [Fact]
    public void Suspicion_ClampedAtMax()
    {
        var o = NewOccult(); o.Suspicion = 99;
        o.Devotion = 100000; o.ArmyPower = 10000;
        WorldMapSystem.Conquer(o, OccultData.MapNode("flesh_pit"));
        WorldMapSystem.TickSuspicion(o, 100);
        Assert.Equal(OccultBalance.SuspicionMax, o.Suspicion);
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
    public void LeyLine_RequiresTech()
    {
        var o = NewOccult(); o.Devotion = 100000; o.ArmyPower = 10000;
        Assert.False(WorldMapSystem.CanConnectLeyLine(o, "a", "b"));
        o.UnlockedTechs.Add(TechId.LeyLineWeaving);
        WorldMapSystem.Conquer(o, OccultData.MapNode("old_library"));
        WorldMapSystem.Conquer(o, OccultData.MapNode("ancient_ruins"));
        Assert.True(WorldMapSystem.CanConnectLeyLine(o, "old_library", "ancient_ruins"));
    }

    // --- Cauldron ---

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

    // --- Grand Sacrifice ---

    [Fact]
    public void Favor_ZeroBelowThreshold()
    {
        var state = NewState();
        state.Occult.LifetimeDevotion = 999999;
        Assert.Equal(0, GrandSacrifice.CalculateFavor(state));
    }

    [Fact]
    public void Favor_CalculatedFromLifetimeDevotion()
    {
        var state = NewState();
        state.Occult.LifetimeDevotion = 4_000_000;
        Assert.Equal(2.0, GrandSacrifice.CalculateFavor(state));
    }

    [Fact]
    public void Favor_ContinentMultiplier()
    {
        var state = NewState();
        state.Occult.LifetimeDevotion = 4_000_000;
        state.Occult.Devotion = 100000; state.Occult.ArmyPower = 10000;
        WorldMapSystem.Conquer(state.Occult, OccultData.MapNode("old_library"));
        Assert.Equal(2.0, Math.Floor(GrandSacrifice.CalculateFavor(state)));
    }

    [Fact]
    public void PerformSacrifice_GrantsFavorAndResets()
    {
        var state = NewState();
        state.Occult.LifetimeDevotion = 4_000_000;
        state.Occult.Devotion = 500;
        state.Occult.Acolytes = 50;
        var favor = GrandSacrifice.PerformSacrifice(state);
        Assert.True(favor >= 1);
        Assert.Equal(favor, state.Occult.EldritchFavor);
        Assert.Equal(0, state.Occult.Devotion);
        Assert.Equal(0, state.Occult.Acolytes);
        Assert.Single(state.Covens);
    }

    [Fact]
    public void PerformSacrifice_MemoriesRetainsFk()
    {
        var state = NewState();
        state.Occult.LifetimeDevotion = 4_000_000;
        state.Occult.ForbiddenKnowledge = 10000;
        state.Occult.UnlockedTechs.Add(TechId.MemoriesOfTheDeep);
        GrandSacrifice.PerformSacrifice(state);
        Assert.Equal(1000, state.Occult.ForbiddenKnowledge);
    }

    [Fact]
    public void PerformSacrifice_AstralAnchorKeepsHighPriest()
    {
        var state = NewState();
        state.Occult.LifetimeDevotion = 4_000_000;
        state.Occult.UnlockedTechs.Add(TechId.AstralAnchor);
        state.Occult.GrandSacrificeCount = 1;
        state.Occult.Acolytes = 100;
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
        state.Occult.EldritchFavor = 10;
        Assert.Equal(1.2, GrandSacrifice.GlobalProductionMult(state));
    }

    // --- OccultEngine integration ---

    [Fact]
    public void Tap_GeneratesDevotion()
    {
        var state = NewState();
        var gained = OccultEngine.Tap(state);
        Assert.True(gained > 0);
        Assert.True(state.Occult.Devotion > 0);
        Assert.True(state.Occult.LifetimeDevotion > 0);
    }

    [Fact]
    public void BuyClickPower_IncreasesLevel()
    {
        var o = NewOccult(); o.Devotion = 500;
        Assert.True(OccultEngine.BuyClickPower(o));
        Assert.Equal(1, o.ClickPowerLevel);
        Assert.True(o.Devotion < 500);
    }

    [Fact]
    public void HireAcolyte_IncreasesCount()
    {
        var o = NewOccult(); o.Devotion = 500;
        Assert.True(OccultEngine.HireAcolyte(o));
        Assert.Equal(1, o.Acolytes);
    }

    [Fact]
    public void HireAcolyte_Capped()
    {
        var o = NewOccult(); o.Acolytes = 200;
        Assert.False(OccultEngine.CanHireAcolyte(o));
    }

    [Fact]
    public void Tick_GeneratesDevotionAndFk()
    {
        var state = NewState();
        state.Occult.Acolytes = 10;
        var devotionBefore = state.Occult.Devotion;
        OccultEngine.Tick(state, 1.0);
        Assert.True(state.Occult.Devotion > devotionBefore);
        state.Occult.Devotion = 10000; state.Occult.ArmyPower = 1000;
        WorldMapSystem.Conquer(state.Occult, OccultData.MapNode("old_library"));
        var fkBefore = state.Occult.ForbiddenKnowledge;
        OccultEngine.Tick(state, 1.0);
        Assert.True(state.Occult.ForbiddenKnowledge > fkBefore);
    }

    [Fact]
    public void Frenzy_RequiresTechAndMinion()
    {
        var o = NewOccult();
        Assert.False(OccultEngine.CanActivateFrenzy(o));
        o.UnlockedTechs.Add(TechId.ExsanguinationEngine);
        Assert.False(OccultEngine.CanActivateFrenzy(o));
        o.Acolytes = 100;
        CultistHierarchy.Promote(o);
        Assert.True(OccultEngine.CanActivateFrenzy(o));
    }

    [Fact]
    public void Frenzy_ConsumesMinionAndSetsTimer()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.ExsanguinationEngine);
        o.Acolytes = 100;
        CultistHierarchy.Promote(o);
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
        state.Occult.Acolytes = 100;
        CultistHierarchy.Promote(state.Occult);
        OccultEngine.ActivateFrenzy(state.Occult);
        var frenzyPower = OccultEngine.ClickPower(state);
        Assert.True(frenzyPower > basePower * 5);
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
    public void MassHysteria_DoublesFk()
    {
        var state = NewState();
        state.Occult.Acolytes = 10;
        state.Occult.Devotion = 10000; state.Occult.ArmyPower = 1000;
        WorldMapSystem.Conquer(state.Occult, OccultData.MapNode("old_library"));
        var baseFk = OccultEngine.TotalFkPerSec(state);
        state.Occult.UnlockedTechs.Add(TechId.MassHysteria);
        OccultEngine.ActivateMassHysteria(state.Occult);
        var hysteriaFk = OccultEngine.TotalFkPerSec(state);
        Assert.True(hysteriaFk > baseFk * 1.5);
    }

    // --- Save/Load migration ---

    [Fact]
    public void SaveLoad_PreservesOccultState()
    {
        var state = NewState();
        state.Occult.Devotion = 500;
        state.Occult.ForbiddenKnowledge = 200;
        state.Occult.EldritchFavor = 10;
        state.Occult.Acolytes = 50;
        state.Occult.UnlockedTechs.Add(TechId.SanguineAutomata);
        var json = SaveLoad.SaveGame(state);
        var loaded = SaveLoad.LoadGame(json);
        Assert.Equal(500, loaded.Occult.Devotion);
        Assert.Equal(200, loaded.Occult.ForbiddenKnowledge);
        Assert.Equal(10, loaded.Occult.EldritchFavor);
        Assert.Equal(50, loaded.Occult.Acolytes);
        Assert.Contains(TechId.SanguineAutomata, loaded.Occult.UnlockedTechs);
    }

    [Fact]
    public void LoadGame_MigratesMissingOccultState()
    {
        var oldJson = "{\"CultName\":\"Old\",\"StartedAt\":1000,\"StoryShown\":false,\"ActiveCovenId\":\"skanor\",\"Covens\":[]}";
        var loaded = SaveLoad.LoadGame(oldJson);
        Assert.NotNull(loaded.Occult);
        Assert.Equal(0, loaded.Occult.Devotion);
        Assert.Empty(loaded.Occult.UnlockedTechs);
    }
}
