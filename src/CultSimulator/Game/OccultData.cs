using System.Collections.Immutable;

namespace CultSimulator.Game;

public static class OccultData
{
    public static readonly ImmutableArray<TechDef> Techs = ImmutableArray.Create(
        new TechDef(TechId.SanguineAutomata, "Sanguine Automata", "🩸", 50, TechBranch.BloodFlesh, "Acolytes auto-sermon 1/sec"),
        new TechDef(TechId.OsmoticExtraction, "Osmotic Extraction", "🌙", 250, TechBranch.BloodFlesh, "Sermons during Blood Moon grant +0.5% total Faith", new[] { TechId.SanguineAutomata }),
        new TechDef(TechId.AutophagousCult, "Autophagous Cult", "💀", 1200, TechBranch.BloodFlesh, "Auto-sacrifices excess Acolytes when cap is hit", new[] { TechId.OsmoticExtraction }),
        new TechDef(TechId.ExsanguinationEngine, "Exsanguination Engine", "⚙️", 10000, TechBranch.BloodFlesh, "Unlocks 15s Frenzy mode (10x sermon) via minion sacrifice", new[] { TechId.AutophagousCult }),
        new TechDef(TechId.WhispersInTheDark, "Whispers in the Dark", "🗣️", 100, TechBranch.MindCoercion, "-15% global Suspicion generation"),
        new TechDef(TechId.InquisitorsBlindfold, "Inquisitor's Blindfold", "👁️", 500, TechBranch.MindCoercion, "Infiltrators auto-suppress raids", new[] { TechId.WhispersInTheDark }),
        new TechDef(TechId.ShadowTactics, "Shadow Tactics", "📐", 2500, TechBranch.MindCoercion, "+25% agent strength in Shadow War", new[] { TechId.InquisitorsBlindfold }),
        new TechDef(TechId.MassHysteria, "Mass Hysteria", "😱", 15000, TechBranch.MindCoercion, "Tapping map regions generates Faith directly for 30s", new[] { TechId.ShadowTactics }),
        new TechDef(TechId.SecondSocket, "Second Socket", "🔮", 200, TechBranch.VoidAstral, "Unlocks Grimoire Slot 2"),
        new TechDef(TechId.TransmutationCrucible, "Transmutation Crucible", "⚗️", 750, TechBranch.VoidAstral, "Unlocks Cauldron crafting", new[] { TechId.SecondSocket }),
        new TechDef(TechId.ThirdSocket, "Third Socket", "🔯", 5000, TechBranch.VoidAstral, "Unlocks Grimoire Slot 3", new[] { TechId.TransmutationCrucible }),
        new TechDef(TechId.ResonanceMastery, "Resonance Mastery", "✨", 30000, TechBranch.VoidAstral, "Doubles 3-set artifact suit bonuses", new[] { TechId.ThirdSocket }),
        new TechDef(TechId.MemoriesOfTheDeep, "Memories of the Deep", "🧠", 8000, TechBranch.OuterGate, "Retain 10% unspent Faith on Grand Sacrifice"),
        new TechDef(TechId.AstralAnchor, "Astral Anchor", "⚓", 25000, TechBranch.OuterGate, "High Priest slot remains unlocked post-reset", new[] { TechId.MemoriesOfTheDeep }),
        new TechDef(TechId.TheStarEatersFeast, "The Star-Eater's Feast", "🌟", 100000, TechBranch.OuterGate, "+100% Eldritch Favor per fully corrupted continent", new[] { TechId.AstralAnchor }));

    public static readonly ImmutableArray<ArtifactDef> Artifacts = ImmutableArray.Create(
        new ArtifactDef("blood_chalice", "Chalice of Blood", "🍷", ArtifactSuit.Blood, "+15% sermon power"),
        new ArtifactDef("blood_blade", "Crimson Blade", "🗡️", ArtifactSuit.Blood, "+20% raid power"),
        new ArtifactDef("blood_heart", "Heart of Sacrifice", "❤️", ArtifactSuit.Blood, "+25% sacrifice yield"),
        new ArtifactDef("void_orb", "Void Orb", "🔮", ArtifactSuit.Void, "+10% Faith generation"),
        new ArtifactDef("void_cloak", "Cloak of Emptiness", "🩵", ArtifactSuit.Void, "-10% Suspicion generation"),
        new ArtifactDef("void_mirror", "Mirror of Null", "🪞", ArtifactSuit.Void, "+15% Acolyte passive"),
        new ArtifactDef("mind_eye", "Third Eye", "👁️", ArtifactSuit.Mind, "+20% Scholar Faith output"),
        new ArtifactDef("mind_tongue", "Whispering Tongue", "👅", ArtifactSuit.Mind, "-15% Inquisitor raid chance"),
        new ArtifactDef("mind_crown", "Crown of Madness", "👑", ArtifactSuit.Mind, "+25% agent combat strength"),
        new ArtifactDef("flesh_golem", "Flesh Golem", "🧟", ArtifactSuit.Flesh, "+50 Acolyte cap"),
        new ArtifactDef("flesh_graft", "Sinew Graft", "🩠", ArtifactSuit.Flesh, "+15% all production"),
        new ArtifactDef("flesh_seed", "Seed of Flesh", "🌱", ArtifactSuit.Flesh, "+5% global multiplier per socketed artifact"));

    public static readonly ImmutableArray<MinionTraitDef> Traits = ImmutableArray.Create(
        new MinionTraitDef("pyromaniac", "Pyromaniac", "+15% raid power, +5% suspicion", 1.15, 1.05, 1.0),
        new MinionTraitDef("scholarly", "Scholarly", "+20% Faith output, -5% raid power", 0.95, 1.0, 1.20),
        new MinionTraitDef("stealthy", "Stealthy", "-20% suspicion generation, -10% raid power", 0.90, 0.80, 1.0),
        new MinionTraitDef("fanatic", "Fanatic", "+25% raid power, +10% suspicion", 1.25, 1.10, 1.0),
        new MinionTraitDef("cunning", "Cunning", "+15% Faith output, -10% suspicion", 1.0, 0.90, 1.15),
        new MinionTraitDef("zealous", "Zealous", "+30% sermon power, +5% suspicion", 1.0, 1.05, 1.0),
        new MinionTraitDef("voidtouched", "Voidtouched", "+25% Faith output, -15% raid power", 0.85, 1.0, 1.25),
        new MinionTraitDef("fleshspeaker", "Fleshspeaker", "+50 Acolyte cap, +5% suspicion", 1.0, 1.05, 1.0));

    public static readonly ImmutableArray<CauldronRecipeDef> Recipes = ImmutableArray.Create(
        new CauldronRecipeDef(CauldronRecipeId.CrimsonElixir, "Crimson Elixir", "🧪", new() { { MaterialKind.GraveDust, 3 } }, "+100% sermon power for 60s", false),
        new CauldronRecipeDef(CauldronRecipeId.VoidTincture, "Void Tincture", "💧", new() { { MaterialKind.DemonBile, 3 } }, "+50% Faith generation for 60s", false),
        new CauldronRecipeDef(CauldronRecipeId.MindPhiltre, "Mind Philtre", "🧠", new() { { MaterialKind.AstralMercury, 3 } }, "-50% Suspicion for 60s", false),
        new CauldronRecipeDef(CauldronRecipeId.FleshBrew, "Flesh Brew", "🥕", new() { { MaterialKind.GraveDust, 1 }, { MaterialKind.DemonBile, 1 }, { MaterialKind.AstralMercury, 1 } }, "+100 Acolytes instantly", false),
        new CauldronRecipeDef(CauldronRecipeId.BloodForge, "Blood Forge", "🍷", new() { { MaterialKind.GraveDust, 5 }, { MaterialKind.DemonBile, 2 } }, "Forge a random Blood artifact", true),
        new CauldronRecipeDef(CauldronRecipeId.VoidForge, "Void Forge", "🔮", new() { { MaterialKind.DemonBile, 5 }, { MaterialKind.AstralMercury, 2 } }, "Forge a random Void artifact", true),
        new CauldronRecipeDef(CauldronRecipeId.MindForge, "Mind Forge", "👁️", new() { { MaterialKind.AstralMercury, 5 }, { MaterialKind.GraveDust, 2 } }, "Forge a random Mind artifact", true),
        new CauldronRecipeDef(CauldronRecipeId.FleshForge, "Flesh Forge", "🧟", new() { { MaterialKind.GraveDust, 3 }, { MaterialKind.DemonBile, 3 }, { MaterialKind.AstralMercury, 3 } }, "Forge a random Flesh artifact", true));

    public static readonly ImmutableArray<MapNodeDef> MapNodes = ImmutableArray.Create(
        new MapNodeDef("skanor_runestone", "Viking Runestone", "🪨", 150, 10, 0.5, 0.2, 55.63, 13.07, "Stora Köpinge, Skåne", new() { { MaterialKind.AstralMercury, 1 } }, "skanor"),
        new MapNodeDef("skanor_bog", "Ageröd Bog Sacrifice", "🔦", 500, 30, 0.8, 0.3, 56.01, 12.74, "Ageröd Mosse, Skåne", new() { { MaterialKind.GraveDust, 2 } }, "skanor"),
        new MapNodeDef("skanor_mound", "Kivik Royal Mound", "⛰️", 1500, 100, 1.5, 0.5, 55.31, 14.23, "Kivik Grave Mound, Skåne", new() { { MaterialKind.AstralMercury, 2 }, { MaterialKind.GraveDust, 1 } }, "skanor"),
        new MapNodeDef("chiloe_cave", "Warlock's Cave of Chiloé", "🦇", 200, 15, 0.5, 0.2, -42.18, -73.90, "Huiliche Cave, Chiloé", new() { { MaterialKind.GraveDust, 1 } }, "la_recta_provincia"),
        new MapNodeDef("chiloe_forest", "Ancient Alder Grove", "🌲", 600, 40, 0.8, 0.3, -42.65, -73.85, "Tepual Forest, Chiloé", new() { { MaterialKind.AstralMercury, 2 } }, "la_recta_provincia"),
        new MapNodeDef("chiloe_island", "Isla de las Almas", "👻", 1800, 120, 1.5, 0.5, -43.10, -73.50, "Isla Chaullín, Chiloé", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.AstralMercury, 1 } }, "la_recta_provincia"),
        new MapNodeDef("friuli_oak", "Sacred Oak of the Benandanti", "🌳", 180, 12, 0.5, 0.2, 46.07, 13.10, "Cividale del Friuli", new() { { MaterialKind.AstralMercury, 1 } }, "benandanti"),
        new MapNodeDef("friuli_crossroads", "Crossroads of the Night Battles", "⚔️", 550, 35, 0.8, 0.3, 46.20, 12.90, "Gemona del Friuli", new() { { MaterialKind.GraveDust, 2 } }, "benandanti"),
        new MapNodeDef("friuli_church", "Rovine della Chiesa Stregata", "⛪", 1600, 110, 1.5, 0.5, 45.90, 13.20, "Aquileia Basilica Ruins", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.GraveDust, 1 } }, "benandanti"),
        new MapNodeDef("malkin_tower", "Malkin Tower Ruins", "🏚️", 150, 10, 0.5, 0.2, 53.87, -2.31, "Malkin Tower, Lancashire", new() { { MaterialKind.GraveDust, 1 } }, "malkin_tower_coven"),
        new MapNodeDef("pendle_hill", "Pendle Hill Summit", "🏔️", 500, 35, 0.8, 0.3, 53.87, -2.30, "Pendle Hill, Lancashire", new() { { MaterialKind.AstralMercury, 2 } }, "malkin_tower_coven"),
        new MapNodeDef("lancaster_court", "Lancaster Assizes Dungeon", "⛓️", 1700, 120, 1.5, 0.5, 54.05, -2.80, "Lancaster Castle Dungeon", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.GraveDust, 1 } }, "malkin_tower_coven"),
        new MapNodeDef("berwick_kirk", "St. Andrew's Auld Kirk", "⛪", 160, 12, 0.5, 0.2, 56.05, -2.72, "North Berwick Kirk", new() { { MaterialKind.GraveDust, 1 } }, "north_berwick_coven"),
        new MapNodeDef("berwick_hill", "Berwick Law Hillfort", "⛰️", 550, 38, 0.8, 0.3, 56.04, -2.69, "North Berwick Law", new() { { MaterialKind.AstralMercury, 2 } }, "north_berwick_coven"),
        new MapNodeDef("berwick_cove", "Cove of the Storm Witches", "🌊", 1650, 115, 1.5, 0.5, 56.00, -2.75, "Milsey Bay, North Berwick", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.AstralMercury, 1 } }, "north_berwick_coven"),
        new MapNodeDef("triora_cabotina", "La Cabotina Witch House", "🏚️", 150, 10, 0.5, 0.2, 43.98, 7.71, "Triora, Liguria", new() { { MaterialKind.GraveDust, 1 } }, "la_cabotina"),
        new MapNodeDef("triora_megalith", "Argenten Megalith Stone", "🗿", 550, 35, 0.8, 0.3, 44.02, 7.65, "Argenten Valley, Liguria", new() { { MaterialKind.AstralMercury, 2 } }, "la_cabotina"),
        new MapNodeDef("triora_gorge", "Gorge of the accused", "⛰️", 1700, 120, 1.5, 0.5, 43.95, 7.75, "Argentina River Gorge", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.GraveDust, 1 } }, "la_cabotina"),
        new MapNodeDef("cozumel_cenote", "Sacred Cenote of Ixchel", "💧", 200, 15, 0.5, 0.2, 20.50, -86.95, "San Gervasio Cenote, Cozumel", new() { { MaterialKind.AstralMercury, 1 } }, "ixchel_priestesses"),
        new MapNodeDef("yucatan_pyramid", "Moon Pyramid of Ixchel", "🏜️", 600, 40, 0.8, 0.3, 20.68, -88.57, "Chichen Itza Outskirts", new() { { MaterialKind.GraveDust, 2 } }, "ixchel_priestesses"),
        new MapNodeDef("yucatan_temple", "Tulum Cliffside Temple", "🛕", 1800, 130, 1.5, 0.5, 20.21, -87.46, "Tulum Ruins, Quintana Roo", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.AstralMercury, 1 } }, "ixchel_priestesses"),
        new MapNodeDef("newforest_altar", "Bronze Age Sacrificial Altar", "🗿", 150, 10, 0.5, 0.2, 50.85, -1.60, "New Forest Bronze Altar", new() { { MaterialKind.GraveDust, 1 } }, "new_forest_coven"),
        new MapNodeDef("newforest_standing", "Stonestanding Circle", "🪨", 550, 35, 0.8, 0.3, 50.88, -1.55, "New Forest Standing Stones", new() { { MaterialKind.AstralMercury, 2 } }, "new_forest_coven"),
        new MapNodeDef("newforest_barrows", "Dark Barrow Burial Mounds", "⚰️", 1700, 120, 1.5, 0.5, 50.82, -1.65, "New Forest Barrows", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.GraveDust, 1 } }, "new_forest_coven"));

    public static readonly ImmutableArray<PromotedRole> PromotedRoles = ImmutableArray.Create(PromotedRole.Zealot, PromotedRole.Scholar, PromotedRole.Infiltrator);
    public static readonly ImmutableArray<CouncilRole> CouncilRoles = ImmutableArray.Create(CouncilRole.Inquisitor, CouncilRole.Archon, CouncilRole.HighPriest);

    public static TechDef Tech(TechId id) => Techs.First(t => t.Id == id);
    public static ArtifactDef? Artifact(string id) => Artifacts.FirstOrDefault(a => a.Id == id);
    public static MinionTraitDef? Trait(string id) => Traits.FirstOrDefault(t => t.Id == id);
    public static CauldronRecipeDef Recipe(CauldronRecipeId id) => Recipes.First(r => r.Id == id);
    public static MapNodeDef? MapNode(string id) => MapNodes.FirstOrDefault(n => n.Id == id);

    public static IReadOnlyList<MapNodeDef> NodesForCoven(string covenId) =>
        MapNodes.Where(n => n.CovenId == covenId).ToList();

    public static IReadOnlyList<MapNodeDef> NodesForActiveCoven(GameState state) =>
        NodesForCoven(state.ActiveCovenId);
}
