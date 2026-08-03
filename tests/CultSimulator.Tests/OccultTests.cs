using CultSimulator.Game;

namespace CultSimulator.Tests;

public class OccultTests
{
    private static GameState NewState()
    {
        var state = GameEngine.InitialState();
        state.CultName = "Test";
        return state;
    }

    private static OccultState NewOccult() => new();

    [Fact]
    public void Tap_GeneratesFaith()
    {
        var state = NewState();
        var faithBefore = state.ActiveCoven.Faith;
        OccultEngine.Tap(state);
        Assert.True(state.ActiveCoven.Faith > faithBefore);
    }

    [Fact]
    public void BuySermonPower_IncreasesTapMultiplier()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 10000;
        var initialMultiplier = state.Occult.TapMultiplier;
        OccultEngine.BuySermonPower(state);
        Assert.True(state.Occult.TapMultiplier > initialMultiplier);
    }

    [Fact]
    public void HireInitiate_RequiresAndConsumesFaith()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 500;
        Assert.True(OccultEngine.CanHireInitiate(state));
        OccultEngine.HireInitiate(state);
        Assert.True(state.Occult.Initiates > 0);
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
        o.Minions.Add(new Minion { Id = "m1", Name = "Test", Role = PromotedRole.ShadowAcolyte });
        Assert.True(OccultEngine.CanActivateFrenzy(o));
    }

    [Fact]
    public void GrandSacrifice_GrantsFavor()
    {
        var state = NewState();
        state.Occult.LifetimeFaith = 2_000_000;
        var favorBefore = state.EldritchFavor;
        var favor = GrandSacrifice.PerformSacrifice(state);
        Assert.True(favor > 0);
        Assert.True(state.EldritchFavor > favorBefore);
    }

    [Fact]
    public void TechTree_UnlocksTech()
    {
        var state = NewState();
        state.ActiveCoven.Faith = 100000;
        TechTree.Unlock(state, TechId.SanguineAutomata);
        Assert.Contains(TechId.SanguineAutomata, state.Occult.UnlockedTechs);
    }

    [Fact]
    public void Grimoire_SocketsAndUnsocketsArtifact()
    {
        var state = NewState();
        state.Occult.OwnedArtifacts.Add("ancient_tablet");
        Grimoire.Socket(state.Occult, "ancient_tablet");
        Assert.Contains("ancient_tablet", state.Occult.SocketedArtifacts);
        Grimoire.Unsocket(state.Occult, "ancient_tablet");
        Assert.DoesNotContain("ancient_tablet", state.Occult.SocketedArtifacts);
    }

    [Fact]
    public void Cauldron_CraftsRecipe()
    {
        var state = NewState();
        state.Occult.Materials[MaterialKind.Blood] = 100;
        state.Occult.Materials[MaterialKind.Bone] = 100;
        Cauldron.Craft(state, CauldronRecipeId.WarElixir);
        Assert.True(state.Occult.Materials[MaterialKind.WarElixir] > 0);
    }

    [Fact]
    public void CultistHierarchy_PromotesMinion()
    {
        var state = NewState();
        state.Occult.Initiates = 10;
        CultistHierarchy.Promote(state.Occult);
        Assert.True(state.Occult.Minions.Count > 0);
        Assert.Equal(0, state.Occult.Initiates);
    }

    [Fact]
    public void CultistHierarchy_RecruitUnitForRole()
    {
        var state = NewState();
        state.Occult.Initiates = 100;
        CultistHierarchy.Promote(state.Occult);
        var minion = state.Occult.Minions[0];
        minion.Role = PromotedRole.ShadowAcolyte;
        CultistHierarchy.RecruitUnitForRole(state.Occult, PromotedRole.ShadowAcolyte);
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
        Assert.False(SaveLoad.IsCorrupted("{\"CultName\":\"Test\",\"ActiveCovenId\":\"skanor\",\"Covens\":[],\"Occult\":{}}"));
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
