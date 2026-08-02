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
        Assert.Equal(200, o.Initiates);
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
    public void InitiateCap_IncreasesWithTech()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.AutophagousCult);
        Assert.True(CultistHierarchy.InitiateCap(o) > 200);
    }

    [Fact]
    public void InitiateCap_IncreasesWithFleshGolem()
    {
        var o = NewOccult();
        o.OwnedArtifacts.Add("flesh_golem");
        Assert.True(CultistHierarchy.InitiateCap(o) > 200);
    }

    [Fact]
    public void Promote_CreatesMinionWithRandomName()
    {
        var o = NewOccult(); o.Initiates = 100;
        var minion = CultistHierarchy.Promote(o);
        Assert.NotEmpty(minion.Name);
        Assert.NotEmpty(minion.TraitId);
    }

    [Fact]
    public void Promote_GeneratesRandomTrait()
    {
        var o = NewOccult(); o.Initiates = 500;
        var traits = new HashSet<string>();
        for (int i = 0; i < 3; i++)
        {
            var m = CultistHierarchy.Promote(o);
            traits.Add(m.TraitId);
        }
        Assert.True(traits.Count >= 1);
    }

    [Fact]
    public void RecruitUnit_RequiresEnoughInitiates()
    {
        var o = NewOccult(); o.Initiates = 99;
        Assert.Null(CultistHierarchy.RecruitUnit(o, PromotedRole.Zealot));
    }

    [Fact]
    public void RecruitUnit_ConsumesCorrectInitiates()
    {
        var o = NewOccult(); o.Initiates = 300;
        CultistHierarchy.RecruitUnit(o, PromotedRole.Zealot);
        Assert.Equal(200, o.Initiates);
    }

    [Fact]
    public void Sacrifice_GivesFaithBasedOnTier()
    {
        var state = NewState();
        state.Occult.Initiates = 100;
        var minion = CultistHierarchy.Promote(state.Occult);
        var faith = CultistHierarchy.Sacrifice(state, minion.Id);
        Assert.True(faith > 0);
    }

    [Fact]
    public void AppointCouncil_RequiresMinion()
    {
        var o = NewOccult();
        Assert.False(CultistHierarchy.CanAppoint(o, CouncilRole.Inquisitor));
        o.Initiates = 100;
        var minion = CultistHierarchy.Promote(o);
        Assert.True(CultistHierarchy.CanAppoint(o, CouncilRole.Inquisitor));
    }

    [Fact]
    public void AppointCouncil_AssignsMinionToRole()
    {
        var o = NewOccult(); o.Initiates = 100;
        var minion = CultistHierarchy.Promote(o);
        CultistHierarchy.AppointCouncil(o, CouncilRole.Inquisitor, minion.Id);
        Assert.Single(o.HighCouncil);
        Assert.Equal(CouncilRole.Inquisitor, o.HighCouncil[0].Role);
    }

    [Fact]
    public void RemoveCouncil_FreesMinion()
    {
        var o = NewOccult(); o.Initiates = 100;
        var minion = CultistHierarchy.Promote(o);
        CultistHierarchy.AppointCouncil(o, CouncilRole.Inquisitor, minion.Id);
        CultistHierarchy.RemoveCouncil(o, CouncilRole.Inquisitor);
        Assert.Empty(o.HighCouncil);
    }

    [Fact]
    public void TechTree_UnlockRequiresFaith()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 10;
        TechTree.Unlock(state, TechId.SanguineAutomata);
        Assert.DoesNotContain(TechId.SanguineAutomata, state.Occult.UnlockedTechs);
    }

    [Fact]
    public void TechTree_UnlockDeductsFaith()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 100;
        TechTree.Unlock(state, TechId.SanguineAutomata);
        Assert.Contains(TechId.SanguineAutomata, state.Occult.UnlockedTechs);
        Assert.True(state.ActiveCoven.Faith < 100);
    }

    [Fact]
    public void TechTree_UnlockRequiresPrerequisites()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 10000;
        TechTree.Unlock(state, TechId.OsmoticExtraction);
        Assert.DoesNotContain(TechId.OsmoticExtraction, state.Occult.UnlockedTechs);
    }

    [Fact]
    public void TechTree_UnlockWithPrerequisites()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 10000;
        TechTree.Unlock(state, TechId.SanguineAutomata);
        TechTree.Unlock(state, TechId.OsmoticExtraction);
        Assert.Contains(TechId.OsmoticExtraction, state.Occult.UnlockedTechs);
    }

    [Fact]
    public void Grimoire_SocketArtifact()
    {
        var o = NewOccult();
        o.OwnedArtifacts.Add("blood_chalice");
        Grimoire.Socket(o, "blood_chalice");
        Assert.Contains("blood_chalice", o.SocketedArtifacts);
    }

    [Fact]
    public void Grimoire_UnsocketArtifact()
    {
        var o = NewOccult();
        o.OwnedArtifacts.Add("blood_chalice");
        Grimoire.Socket(o, "blood_chalice");
        Grimoire.Unsocket(o, "blood_chalice");
        Assert.DoesNotContain("blood_chalice", o.SocketedArtifacts);
    }

    [Fact]
    public void Grimoire_SocketRequiresOwnership()
    {
        var o = NewOccult();
        Grimoire.Socket(o, "blood_chalice");
        Assert.DoesNotContain("blood_chalice", o.SocketedArtifacts);
    }

    [Fact]
    public void Grimoire_SocketLimitedByTech()
    {
        var o = NewOccult();
        o.OwnedArtifacts.Add("blood_chalice");
        o.OwnedArtifacts.Add("void_orb");
        Grimoire.Socket(o, "blood_chalice");
        Grimoire.Socket(o, "void_orb");
        Assert.Single(o.SocketedArtifacts);
    }

    [Fact]
    public void Grimoire_SecondSlotRequiresTech()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.SecondSocket);
        o.OwnedArtifacts.Add("blood_chalice");
        o.OwnedArtifacts.Add("void_orb");
        Grimoire.Socket(o, "blood_chalice");
        Grimoire.Socket(o, "void_orb");
        Assert.Equal(2, o.SocketedArtifacts.Count);
    }

    [Fact]
    public void Grimoire_ThirdSlotRequiresTech()
    {
        var o = NewOccult();
        o.UnlockedTechs.Add(TechId.SecondSocket);
        o.UnlockedTechs.Add(TechId.ThirdSocket);
        o.OwnedArtifacts.Add("blood_chalice");
        o.OwnedArtifacts.Add("void_orb");
        o.OwnedArtifacts.Add("mind_eye");
        Grimoire.Socket(o, "blood_chalice");
        Grimoire.Socket(o, "void_orb");
        Grimoire.Socket(o, "mind_eye");
        Assert.Equal(3, o.SocketedArtifacts.Count);
    }

    [Fact]
    public void Cauldron_CraftRequiresTransmutationTech()
    {
        var state = NewState();
        var (success, _) = Cauldron.Craft(state, CauldronRecipeId.CrimsonElixir);
        Assert.False(success);
    }

    [Fact]
    public void Cauldron_CraftWithTechAndAgents()
    {
        var state = NewState();
        state.Occult.UnlockedTechs.Add(TechId.TransmutationCrucible);
        state.ShadowWarOrInit.TotalAgents = 100;
        var (success, _) = Cauldron.Craft(state, CauldronRecipeId.CrimsonElixir);
        Assert.True(success);
        Assert.True(state.Occult.ElixirTimer > 0);
    }

    [Fact]
    public void Cauldron_CraftConsumesAgents()
    {
        var state = NewState();
        state.Occult.UnlockedTechs.Add(TechId.TransmutationCrucible);
        state.ShadowWarOrInit.TotalAgents = 100;
        var before = state.ShadowWarOrInit.AvailableAgents;
        Cauldron.Craft(state, CauldronRecipeId.CrimsonElixir);
        Assert.True(state.ShadowWarOrInit.AvailableAgents < before);
    }

    [Fact]
    public void OccultEngine_Tap_GivesFaith()
    {
        var state = NewState();
        state.Occult.Initiates = 10;
        var faith = OccultEngine.Tap(state);
        Assert.True(faith > 0);
        Assert.True(state.ActiveCoven.Faith > 0);
    }

    [Fact]
    public void OccultEngine_BuySermonPower_IncreasesPower()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 1000;
        var initial = state.Occult.SermonPowerLevel;
        OccultEngine.BuySermonPower(state);
        Assert.True(state.Occult.SermonPowerLevel > initial);
    }

    [Fact]
    public void OccultEngine_HireAcolyte_AddsAcolyte()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 1000;
        var initial = state.Occult.Acolytes;
        OccultEngine.HireAcolyte(state);
        Assert.True(state.Occult.Acolytes > initial);
    }

    [Fact]
    public void OccultEngine_Tick_GeneratesFaithFromAcolytes()
    {
        var state = NewState();
        state.Occult.Acolytes = 10;
        var initialFaith = state.ActiveCoven.Faith;
        OccultEngine.Tick(state, 1.0);
        Assert.True(state.ActiveCoven.Faith > initialFaith);
    }

    [Fact]
    public void OccultEngine_Tick_GeneratesFaithFromInitiates()
    {
        var state = NewState();
        state.Occult.Initiates = 10;
        var initialFaith = state.ActiveCoven.Faith;
        OccultEngine.Tick(state, 1.0);
        Assert.True(state.ActiveCoven.Faith > initialFaith);
    }

    [Fact]
    public void GrandSacrifice_PerformSacrifice_GrantsFavor()
    {
        var state = NewState();
        state.CultName = "Test";
        state.Occult.LifetimeFaith = 2_000_000;
        state.EldritchFavor = 0;
        var favor = GrandSacrifice.PerformSacrifice(state);
        Assert.True(favor > 0);
        Assert.True(state.EldritchFavor > 0);
    }

    [Fact]
    public void GrandSacrifice_PerformSacrifice_ResetsProgress()
    {
        var state = NewState();
        state.CultName = "Test";
        state.Occult.LifetimeFaith = 2_000_000;
        state.ActiveCoven.Faith = 5000;
        state.Occult.Initiates = 50;
        GrandSacrifice.PerformSacrifice(state);
        Assert.Equal(0, state.ActiveCoven.Faith);
        Assert.Equal(0, state.Occult.Initiates);
    }

    [Fact]
    public void GrandSacrifice_PerformSacrifice_IncrementsCount()
    {
        var state = NewState();
        state.CultName = "Test";
        state.Occult.LifetimeFaith = 2_000_000;
        var count = state.GrandSacrificeCount;
        GrandSacrifice.PerformSacrifice(state);
        Assert.True(state.GrandSacrificeCount > count);
    }

    [Fact]
    public void SaveLoad_RoundTripPreservesState()
    {
        var state = NewState();
        state.CultName = "TestCult";
        state.ActiveCoven.Faith = 500;
        state.Occult.LifetimeFaith = 10000;
        state.Occult.EldritchFavor = 10;
        state.Occult.Initiates = 50;
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

    [Fact]
    public void SaveLoad_VersionIsSetOnSave()
    {
        var state = NewState();
        state.CultName = "Test";
        var json = SaveLoad.SaveGame(state);
        Assert.Contains("\"SaveVersion\":2", json);
    }

    [Fact]
    public void SaveLoad_MigratesVersionZeroToCurrent()
    {
        var oldJson = "{\"CultName\":\"Old\",\"StartedAt\":1000,\"StoryShown\":false,\"SaveVersion\":0,\"ActiveCovenId\":\"skanor\",\"Covens\":[{\"Id\":\"skanor\",\"Converted\":true}]}";
        var (state, success) = SaveLoad.LoadGameWithBackup(oldJson, null, null);
        Assert.True(success);
        Assert.Equal(SaveLoad.CurrentVersion, state.SaveVersion);
        Assert.NotNull(state.ShadowWar);
        Assert.NotNull(state.BattleSystem);
        Assert.NotNull(state.RivalCults);
    }

    [Fact]
    public void SaveLoad_MigratesVersionOneToCurrent()
    {
        var oldJson = "{\"CultName\":\"Old\",\"StartedAt\":1000,\"StoryShown\":false,\"SaveVersion\":1,\"ActiveCovenId\":\"skanor\",\"Covens\":[{\"Id\":\"skanor\",\"Converted\":true}]}";
        var (state, success) = SaveLoad.LoadGameWithBackup(oldJson, null, null);
        Assert.True(success);
        Assert.Equal(SaveLoad.CurrentVersion, state.SaveVersion);
        Assert.NotNull(state.LocalCultBattles);
        Assert.NotNull(state.RivalCults);
        Assert.NotNull(state.RivalCults.RivalBattles);
    }

    [Fact]
    public void SaveLoad_RejectsCorruptedJson()
    {
        Assert.False(SaveLoad.IsValidSave(null));
        Assert.False(SaveLoad.IsValidSave(""));
        Assert.False(SaveLoad.IsValidSave("{bad json"));
        Assert.True(SaveLoad.IsCorrupted(null));
        Assert.True(SaveLoad.IsCorrupted("{bad json"));
        Assert.False(SaveLoad.IsCorrupted("{\"CultName\":\"Test\",\"ActiveCovenId\":\"skanor\",\"Covens\":[]}"));
    }

    [Fact]
    public void SaveLoad_FallsBackToBackupWhenPrimaryCorrupt()
    {
        var backup = "{\"CultName\":\"Backup\",\"StartedAt\":2000,\"StoryShown\":true,\"SaveVersion\":2,\"ActiveCovenId\":\"skanor\",\"Covens\":[{\"Id\":\"skanor\",\"Converted\":true}]}";
        var (state, success) = SaveLoad.LoadGameWithBackup("CORRUPTED", backup, null);
        Assert.True(success);
        Assert.Equal("Backup", state.CultName);
    }

    [Fact]
    public void SaveLoad_FallsBackToBackup2WhenAllCorrupt()
    {
        var backup2 = "{\"CultName\":\"Backup2\",\"StartedAt\":3000,\"StoryShown\":true,\"SaveVersion\":2,\"ActiveCovenId\":\"skanor\",\"Covens\":[{\"Id\":\"skanor\",\"Converted\":true}]}";
        var (state, success) = SaveLoad.LoadGameWithBackup("CORRUPTED", "ALSO_CORRUPT", backup2);
        Assert.True(success);
        Assert.Equal("Backup2", state.CultName);
    }
}
