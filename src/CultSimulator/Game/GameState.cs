using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public class GameState
{
    public List<CovenState> Covens { get; set; } = new();
    public string ActiveCovenId { get; set; } = "skanor";
    public ConversionState? Conversion { get; set; }
    public ShadowWarState? ShadowWar { get; set; }
    public RivalCultState? RivalCults { get; set; }
    public List<LocalCultInstance> ActiveLocalCults { get; set; } = new();
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;
    public bool StoryShown { get; set; }
    public bool FirstPreachDone { get; set; }
    public bool FirstFollowerGained { get; set; }
    public bool FirstBuildingPurchased { get; set; }
    public bool FirstUpgradePurchased { get; set; }
    public bool FirstCovenConverted { get; set; }
    public bool FirstRankUp { get; set; }
    public bool FirstShadowWarAction { get; set; }
    public bool FirstGrandSacrifice { get; set; }
    public bool FirstRivalCultEncounter { get; set; }
    public bool FirstLocalCultSpawn { get; set; }
    public bool FirstLocalCultConvert { get; set; }
    public bool FirstNamingScreen { get; set; }
    public bool FirstOccultPanel { get; set; }
    public bool FirstGrimoirePanel { get; set; }
    public bool FirstCauldronPanel { get; set; }
    public bool FirstTechTree { get; set; }
    public bool FirstCouncil { get; set; }
    public bool FirstPromotion { get; set; }
    public bool FirstMinion { get; set; }
    public bool FirstSocketedArtifact { get; set; }
    public bool FirstMapNode { get; set; }
    public bool FirstRaid { get; set; }
    public bool FirstFrenzy { get; set; }
    public bool FirstWhisperChoir { get; set; }
    public bool FirstCovenBlessing { get; set; }
    public bool FirstElixir { get; set; }
    public bool FirstDarkVigil { get; set; }
    public bool FirstShadowWarVictory { get; set; }
    public bool FirstShadowWarDefeat { get; set; }
    public bool FirstRivalCultDefeat { get; set; }
    public bool FirstRivalCultVictory { get; set; }
    public bool FirstBattleVictory { get; set; }
    public bool FirstBattleDefeat { get; set; }
    public bool FirstGrandSacrificeEldritch { get; set; }
    public bool FirstGrandSacrificeRank { get; set; }
    public bool FirstGrandSacrificeFaith { get; set; }
    public bool FirstGrandSacrificeGold { get; set; }
    public bool FirstGrandSacrificeFollowers { get; set; }
    public bool FirstGrandSacrificeAcolytes { get; set; }
    public bool FirstGrandSacrificeMinions { get; set; }
    public bool FirstGrandSacrificeArtifacts { get; set; }
    public bool FirstGrandSacrificeTechs { get; set; }
    public bool FirstGrandSacrificeCouncil { get; set; }
    public bool FirstGrandSacrificeMapNodes { get; set; }
    public bool FirstGrandSacrificeShadowWar { get; set; }
    public bool FirstGrandSacrificeRivalCults { get; set; }
    public bool FirstGrandSacrificeLocalCults { get; set; }
    public bool FirstGrandSacrificeEldritchFavor { get; set; }
    public bool FirstGrandSacrificeEldritchRank { get; set; }
    public bool FirstGrandSacrificeEldritchArtifact { get; set; }
    public bool FirstGrandSacrificeEldritchTech { get; set; }
    public bool FirstGrandSacrificeEldritchCouncil { get; set; }
    public bool FirstGrandSacrificeEldritchMapNode { get; set; }
    public bool FirstGrandSacrificeEldritchShadowWar { get; set; }
    public bool FirstGrandSacrificeEldritchRivalCult { get; set; }
    public bool FirstGrandSacrificeEldritchLocalCult { get; set; }
    public bool FirstGrandSacrificeEldritchFavor2 { get; set; }
    public bool FirstGrandSacrificeEldritchRank2 { get; set; }
    public bool FirstGrandSacrificeEldritchArtifact2 { get; set; }
    public bool FirstGrandSacrificeEldritchTech2 { get; set; }
    public bool FirstGrandSacrificeEldritchCouncil2 { get; set; }
    public bool FirstGrandSacrificeEldritchMapNode2 { get; set; }
    public bool FirstGrandSacrificeEldritchShadowWar2 { get; set; }
    public bool FirstGrandSacrificeEldritchRivalCult2 { get; set; }
    public bool FirstGrandSacrificeEldritchLocalCult2 { get; set; }
    public bool FirstGrandSacrificeEldritchFavor3 { get; set; }
    public bool FirstGrandSacrificeEldritchRank3 { get; set; }
    public bool FirstGrandSacrificeEldritchArtifact3 { get; set; }
    public bool FirstGrandSacrificeEldritchTech3 { get; set; }
    public bool FirstGrandSacrificeEldritchCouncil3 { get; set; }
    public bool FirstGrandSacrificeEldritchMapNode3 { get; set; }
    public bool FirstGrandSacrificeEldritchShadowWar3 { get; set; }
    public bool FirstGrandSacrificeEldritchRivalCult3 { get; set; }
    public bool FirstGrandSacrificeEldritchLocalCult3 { get; set; }
    public BattleSystemState? BattleSystem { get; set; }

    [JsonIgnore]
    public BattleSystemState BattleSystemOrInit => BattleSystem ??= BattleEngine.CreateInitialState();

    /// <summary>
    /// Ids of foothold covens revealed by completing the previous continent.
    /// Each entry unlocks the next continent so the player can expand into it.
    /// </summary>
    public List<string> RevealedFootholds { get; set; } = new();

    /// <summary>
    /// Continent whose completion story is currently pending display.
    /// Set when a continent is fully conquered; cleared after the player
    /// dismisses the story beat and the foothold is granted.
    /// </summary>
    public string? PendingContinentStory { get; set; }

    public CovenState HomeCoven => FindCoven("skanor")!;
    public CovenState ActiveCoven => FindCoven(ActiveCovenId) ?? HomeCoven;
    public CovenState? FindCoven(string id) => Covens.FirstOrDefault(c => c.Id == id);
}
