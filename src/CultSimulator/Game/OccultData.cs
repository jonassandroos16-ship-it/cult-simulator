using System.Collections.Immutable;

namespace CultSimulator.Game;

public static class OccultData
{
    // ─────────────────────────────────────────────────────────────
    //  Tech Tree — 30 techs across 4 branches (doubled from 15)
    //  Mind & Coercion fully remade: no Suspicion, all new effects
    // ─────────────────────────────────────────────────────────────
    public static readonly ImmutableArray<TechDef> Techs = ImmutableArray.Create(
        // ── Blood & Flesh (8) ──
        new TechDef(TechId.SanguineAutomata, "Sanguine Automata", "🩸", 50, TechBranch.BloodFlesh, "Acolytes auto-sermon 1/sec"),
        new TechDef(TechId.OsmoticExtraction, "Osmotic Extraction", "🌙", 250, TechBranch.BloodFlesh, "Sermons during Blood Moon grant +0.5% total Faith", new[] { TechId.SanguineAutomata }),
        new TechDef(TechId.AutophagousCult, "Autophagous Cult", "💀", 1200, TechBranch.BloodFlesh, "Auto-sacrifices excess Acolytes when cap is hit", new[] { TechId.OsmoticExtraction }),
        new TechDef(TechId.ExsanguinationEngine, "Exsanguination Engine", "⚙️", 10000, TechBranch.BloodFlesh, "Unlocks 15s Frenzy mode (10x sermon) via minion sacrifice", new[] { TechId.AutophagousCult }),
        new TechDef(TechId.CrimsonTide, "Crimson Tide", "🌊", 30000, TechBranch.BloodFlesh, "Sacrifices yield +100% Faith", new[] { TechId.ExsanguinationEngine }),
        new TechDef(TechId.FleshBinding, "Flesh Binding", "🧬", 80000, TechBranch.BloodFlesh, "Acolyte cap +200", new[] { TechId.CrimsonTide }),
        new TechDef(TechId.MarrowTransfusion, "Marrow Transfusion", "🦴", 250000, TechBranch.BloodFlesh, "Acolytes produce +50% Faith", new[] { TechId.FleshBinding }),
        new TechDef(TechId.BloodApocalypse, "Blood Apocalypse", "🌋", 750000, TechBranch.BloodFlesh, "Frenzy lasts 30s and gives 20x sermon power", new[] { TechId.MarrowTransfusion }),

        // ── Mind & Coercion (8) — REMADE ──
        new TechDef(TechId.PropagandaNetwork, "Propaganda Network", "📡", 100, TechBranch.MindCoercion, "Preaching yields +50% Faith"),
        new TechDef(TechId.CognitiveSedation, "Cognitive Sedation", "🧠", 500, TechBranch.MindCoercion, "Acolyte passive Faith +50%", new[] { TechId.PropagandaNetwork }),
        new TechDef(TechId.NeuralChoir, "Neural Choir", "🎵", 2500, TechBranch.MindCoercion, "Unlocks Whisper Choir ritual (3x sermon power, 60s)", new[] { TechId.CognitiveSedation }),
        new TechDef(TechId.MassHysteria, "Mass Hysteria", "😱", 15000, TechBranch.MindCoercion, "Unlocks Mass Hysteria ritual (2x Faith generation, 30s)", new[] { TechId.NeuralChoir }),
        new TechDef(TechId.SubliminalBroadcast, "Subliminal Broadcast", "📺", 50000, TechBranch.MindCoercion, "All follower income +25%", new[] { TechId.MassHysteria }),
        new TechDef(TechId.ZealotConditioning, "Zealot Conditioning", "⚡", 150000, TechBranch.MindCoercion, "Agent strength in Shadow War +25%", new[] { TechId.SubliminalBroadcast }),
        new TechDef(TechId.IndoctrinationRites, "Indoctrination Rites", "📿", 500000, TechBranch.MindCoercion, "Recruit cost -30%", new[] { TechId.ZealotConditioning }),
        new TechDef(TechId.CollectiveTrance, "Collective Trance", "🌀", 1500000, TechBranch.MindCoercion, "All Faith generation +50%", new[] { TechId.IndoctrinationRites }),

        // ── Void & Astral (8) ──
        new TechDef(TechId.SecondSocket, "Second Socket", "🔮", 200, TechBranch.VoidAstral, "Unlocks Grimoire Slot 2"),
        new TechDef(TechId.TransmutationCrucible, "Transmutation Crucible", "⚗️", 750, TechBranch.VoidAstral, "Unlocks Cauldron crafting", new[] { TechId.SecondSocket }),
        new TechDef(TechId.ThirdSocket, "Third Socket", "🔯", 5000, TechBranch.VoidAstral, "Unlocks Grimoire Slot 3", new[] { TechId.TransmutationCrucible }),
        new TechDef(TechId.ResonanceMastery, "Resonance Mastery", "✨", 30000, TechBranch.VoidAstral, "Doubles 3-set artifact suit bonuses", new[] { TechId.ThirdSocket }),
        new TechDef(TechId.VoidTwin, "Void Twin", "🪞", 100000, TechBranch.VoidAstral, "Each socketed artifact also grants +5% Faith", new[] { TechId.ResonanceMastery }),
        new TechDef(TechId.AstralDistillation, "Astral Distillation", "⚗️", 300000, TechBranch.VoidAstral, "Elixir duration +50%", new[] { TechId.VoidTwin }),
        new TechDef(TechId.ElderSign, "Elder Sign", "🔱", 800000, TechBranch.VoidAstral, "Tap power +100%", new[] { TechId.AstralDistillation }),
        new TechDef(TechId.CosmicConvergence, "Cosmic Convergence", "🌌", 2000000, TechBranch.VoidAstral, "All artifact bonuses doubled", new[] { TechId.ElderSign }),

        // ── The Outer Gate (6) ──
        new TechDef(TechId.MemoriesOfTheDeep, "Memories of the Deep", "🧠", 8000, TechBranch.OuterGate, "Retain 10% unspent Faith on Grand Sacrifice"),
        new TechDef(TechId.AstralAnchor, "Astral Anchor", "⚓", 25000, TechBranch.OuterGate, "High Priest slot remains unlocked post-reset", new[] { TechId.MemoriesOfTheDeep }),
        new TechDef(TechId.TheStarEatersFeast, "The Star-Eater's Feast", "🌟", 100000, TechBranch.OuterGate, "+100% Eldritch Favor per fully corrupted continent", new[] { TechId.AstralAnchor }),
        new TechDef(TechId.EchoesOfCreation, "Echoes of Creation", "📜", 500000, TechBranch.OuterGate, "Retain 25% unspent Faith on Grand Sacrifice", new[] { TechId.TheStarEatersFeast }),
        new TechDef(TechId.VoidHeart, "Void Heart", "🖤", 1500000, TechBranch.OuterGate, "Grand Sacrifice keeps all tech unlocks", new[] { TechId.EchoesOfCreation }),
        new TechDef(TechId.AscensionProtocol, "Ascension Protocol", "🚀", 5000000, TechBranch.OuterGate, "Global production ×2 permanently", new[] { TechId.VoidHeart }));

    // ─────────────────────────────────────────────────────────────
    //  Artifacts — 24 (doubled from 12), suspicion effects removed
    // ─────────────────────────────────────────────────────────────
    public static readonly ImmutableArray<ArtifactDef> Artifacts = ImmutableArray.Create(
        // Blood suit (6)
        new ArtifactDef("blood_chalice", "Chalice of Blood", "🍷", ArtifactSuit.Blood, "+15% sermon power"),
        new ArtifactDef("blood_blade", "Crimson Blade", "🗡️", ArtifactSuit.Blood, "+20% raid power"),
        new ArtifactDef("blood_heart", "Heart of Sacrifice", "❤️", ArtifactSuit.Blood, "+25% sacrifice yield"),
        new ArtifactDef("blood_altar", "Altar of Blood", "🩸", ArtifactSuit.Blood, "+10% tap power"),
        new ArtifactDef("blood_fang", "Vampire Fang", "🦷", ArtifactSuit.Blood, "+15% agent combat strength"),
        new ArtifactDef("blood_relic", "Blood Reliquary", "⚱️", ArtifactSuit.Blood, "+20% sacrifice yield"),
        // Void suit (6) — suspicion effects replaced
        new ArtifactDef("void_orb", "Void Orb", "🔮", ArtifactSuit.Void, "+10% Faith generation"),
        new ArtifactDef("void_cloak", "Cloak of Emptiness", "🩵", ArtifactSuit.Void, "+10% tap power"),
        new ArtifactDef("void_mirror", "Mirror of Null", "🪞", ArtifactSuit.Void, "+15% Acolyte passive"),
        new ArtifactDef("void_lens", "Void Lens", "🔍", ArtifactSuit.Void, "+10% map node Faith"),
        new ArtifactDef("void_crystal", "Void Crystal", "💎", ArtifactSuit.Void, "+20% elixir duration"),
        new ArtifactDef("void_crown", "Crown of the Void", "👑", ArtifactSuit.Void, "+15% all production"),
        // Mind suit (6) — suspicion/inquisitor effects replaced
        new ArtifactDef("mind_eye", "Third Eye", "👁️", ArtifactSuit.Mind, "+20% Scholar Faith output"),
        new ArtifactDef("mind_tongue", "Whispering Tongue", "👅", ArtifactSuit.Mind, "+15% recruit efficiency"),
        new ArtifactDef("mind_crown", "Crown of Madness", "👑", ArtifactSuit.Mind, "+25% agent combat strength"),
        new ArtifactDef("mind_glyph", "Glyph of Will", "🔣", ArtifactSuit.Mind, "+10% preaching power"),
        new ArtifactDef("mind_totem", "Totem of Devotion", "🗿", ArtifactSuit.Mind, "+30% follower Faith"),
        new ArtifactDef("mind_spiral", "Spiral of Madness", "🌀", ArtifactSuit.Mind, "+20% all Faith generation"),
        // Flesh suit (6)
        new ArtifactDef("flesh_golem", "Flesh Golem", "🧟", ArtifactSuit.Flesh, "+50 Acolyte cap"),
        new ArtifactDef("flesh_graft", "Sinew Graft", "🩠", ArtifactSuit.Flesh, "+15% all production"),
        new ArtifactDef("flesh_seed", "Seed of Flesh", "🌱", ArtifactSuit.Flesh, "+5% global multiplier per socketed artifact"),
        new ArtifactDef("flesh_heart", "Living Heart", "💗", ArtifactSuit.Flesh, "+100 Acolyte cap"),
        new ArtifactDef("flesh_muscle", "Grafted Muscle", "💪", ArtifactSuit.Flesh, "+20% tap power"),
        new ArtifactDef("flesh_root", "Root of Flesh", "🦴", ArtifactSuit.Flesh, "+10% agent production speed"));

    // ─────────────────────────────────────────────────────────────
    //  Minion Traits — 16 (doubled from 8), suspicion effects removed
    // ─────────────────────────────────────────────────────────────
    public static readonly ImmutableArray<MinionTraitDef> Traits = ImmutableArray.Create(
        new MinionTraitDef("pyromaniac", "Pyromaniac", "+15% raid power, +5% tap power", 1.15, 1.05, 1.0),
        new MinionTraitDef("scholarly", "Scholarly", "+20% Faith output, -10% raid power", 0.90, 1.0, 1.20),
        new MinionTraitDef("stealthy", "Stealthy", "+10% tap power, -10% raid power", 0.90, 1.10, 1.0),
        new MinionTraitDef("fanatic", "Fanatic", "+25% raid power, +10% tap power", 1.25, 1.10, 1.0),
        new MinionTraitDef("cunning", "Cunning", "+15% Faith output, +5% tap power", 1.0, 1.05, 1.15),
        new MinionTraitDef("zealous", "Zealous", "+30% sermon power, +5% raid power", 1.05, 1.30, 1.0),
        new MinionTraitDef("voidtouched", "Voidtouched", "+25% Faith output, -15% raid power", 0.85, 1.0, 1.25),
        new MinionTraitDef("fleshspeaker", "Fleshspeaker", "+50 Acolyte cap, +10% Faith output", 1.0, 1.0, 1.10),
        // 8 new traits
        new MinionTraitDef("bloodthirsty", "Bloodthirsty", "+30% raid power, +15% sacrifice yield", 1.30, 1.0, 1.0),
        new MinionTraitDef("mystic", "Mystic", "+25% tap power, -10% raid power", 0.90, 1.25, 1.10),
        new MinionTraitDef("proselytizer", "Proselytizer", "+20% recruit efficiency, +15% Faith output", 1.0, 1.0, 1.20),
        new MinionTraitDef("warlord", "Warlord", "+40% raid power, +10% agent production", 1.40, 1.0, 1.0),
        new MinionTraitDef("oracle", "Oracle", "+30% Faith output, +10% tap power", 1.0, 1.10, 1.30),
        new MinionTraitDef("heretic", "Heretic", "+35% sermon power, -15% raid power", 0.85, 1.35, 1.0),
        new MinionTraitDef("zeal_master", "Zeal Master", "+20% all Faith generation", 1.0, 1.10, 1.20),
        new MinionTraitDef("fleshbound", "Fleshbound", "+100 Acolyte cap, +20% tap power", 1.0, 1.20, 1.0));

    // ─────────────────────────────────────────────────────────────
    //  Cauldron Recipes — Mind Philtre remade (no suspicion)
    // ─────────────────────────────────────────────────────────────
    public static readonly ImmutableArray<CauldronRecipeDef> Recipes = ImmutableArray.Create(
        new CauldronRecipeDef(CauldronRecipeId.CrimsonElixir, "Crimson Elixir", "🧪", 10, "+100% sermon power for 60s", false),
        new CauldronRecipeDef(CauldronRecipeId.VoidTincture, "Void Tincture", "💧", 10, "+50% Faith generation for 60s", false),
        new CauldronRecipeDef(CauldronRecipeId.MindPhiltre, "Mind Philtre", "🧠", 10, "+100% preaching power for 60s", false),
        new CauldronRecipeDef(CauldronRecipeId.FleshBrew, "Flesh Brew", "🥕", 15, "+100 Acolytes instantly", false),
        new CauldronRecipeDef(CauldronRecipeId.WarElixir, "War Elixir", "⚗️", 20, "+50% battle agent attack for 120s", false),
        new CauldronRecipeDef(CauldronRecipeId.BloodForge, "Blood Forge", "🍷", 30, "Forge a random Blood artifact", true),
        new CauldronRecipeDef(CauldronRecipeId.VoidForge, "Void Forge", "🔮", 30, "Forge a random Void artifact", true),
        new CauldronRecipeDef(CauldronRecipeId.MindForge, "Mind Forge", "👁️", 30, "Forge a random Mind artifact", true),
        new CauldronRecipeDef(CauldronRecipeId.FleshForge, "Flesh Forge", "🧟", 40, "Forge a random Flesh artifact", true));

    // ─────────────────────────────────────────────────────────────
    //  Map Nodes — SuspicionPerSec field removed from MapNodeDef
    // ─────────────────────────────────────────────────────────────
    public static readonly ImmutableArray<MapNodeDef> MapNodes = ImmutableArray.Create(
        new MapNodeDef("skanor_runestone", "Viking Runestone", "🪨", 150, 10, 0.5, 55.63, 13.07, "Stora Köpinge, Skåne", new() { { MaterialKind.AstralMercury, 1 } }, "skanor"),
        new MapNodeDef("skanor_bog", "Ageröd Bog Sacrifice", "🔦", 500, 30, 0.8, 56.01, 12.74, "Ageröd Mosse, Skåne", new() { { MaterialKind.GraveDust, 2 } }, "skanor"),
        new MapNodeDef("skanor_mound", "Kivik Royal Mound", "⛰️", 1500, 100, 1.5, 55.31, 14.23, "Kivik Grave Mound, Skåne", new() { { MaterialKind.AstralMercury, 2 }, { MaterialKind.GraveDust, 1 } }, "skanor"),
        new MapNodeDef("chiloe_cave", "Warlock's Cave of Chiloé", "🦇", 200, 15, 0.5, -42.18, -73.90, "Huilliche Cave, Chiloé", new() { { MaterialKind.GraveDust, 1 } }, "la_recta_provincia"),
        new MapNodeDef("chiloe_forest", "Ancient Alder Grove", "🌲", 600, 40, 0.8, -42.65, -73.85, "Tepual Forest, Chiloé", new() { { MaterialKind.AstralMercury, 2 } }, "la_recta_provincia"),
        new MapNodeDef("chiloe_island", "Isla de las Almas", "👻", 1800, 120, 1.5, -43.10, -73.50, "Isla Chaullín, Chiloé", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.AstralMercury, 1 } }, "la_recta_provincia"),
        new MapNodeDef("friuli_oak", "Sacred Oak of the Benandanti", "🌳", 180, 12, 0.5, 46.07, 13.10, "Cividale del Friuli", new() { { MaterialKind.AstralMercury, 1 } }, "benandanti"),
        new MapNodeDef("friuli_crossroads", "Crossroads of the Night Battles", "⚔️", 550, 35, 0.8, 46.20, 12.90, "Gemona del Friuli", new() { { MaterialKind.GraveDust, 2 } }, "benandanti"),
        new MapNodeDef("friuli_church", "Rovine della Chiesa Stregata", "⛪", 1600, 110, 1.5, 45.90, 13.20, "Aquileia Basilica Ruins", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.GraveDust, 1 } }, "benandanti"),
        new MapNodeDef("malkin_tower", "Malkin Tower Ruins", "🏚️", 150, 10, 0.5, 53.87, -2.31, "Malkin Tower, Lancashire", new() { { MaterialKind.GraveDust, 1 } }, "malkin_tower_coven"),
        new MapNodeDef("pendle_hill", "Pendle Hill Summit", "🏔️", 500, 35, 0.8, 53.87, -2.30, "Pendle Hill, Lancashire", new() { { MaterialKind.AstralMercury, 2 } }, "malkin_tower_coven"),
        new MapNodeDef("lancaster_court", "Lancaster Assizes Dungeon", "⛓️", 1700, 120, 1.5, 54.05, -2.80, "Lancaster Castle Dungeon", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.GraveDust, 1 } }, "malkin_tower_coven"),
        new MapNodeDef("berwick_kirk", "St. Andrew's Auld Kirk", "教堂", 160, 12, 0.5, 56.05, -2.72, "North Berwick Kirk", new() { { MaterialKind.GraveDust, 1 } }, "north_berwick_coven"),
        new MapNodeDef("berwick_hill", "Berwick Law Hillfort", "⛰️", 550, 38, 0.8, 56.04, -2.69, "North Berwick Law", new() { { MaterialKind.AstralMercury, 2 } }, "north_berwick_coven"),
        new MapNodeDef("berwick_cove", "Cove of the Storm Witches", "🌊", 1650, 115, 1.5, 56.00, -2.75, "Milsey Bay, North Berwick", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.AstralMercury, 1 } }, "north_berwick_coven"),
        new MapNodeDef("triora_cabotina", "La Cabotina Witch House", "🏚️", 150, 10, 0.5, 43.98, 7.71, "Triora, Liguria", new() { { MaterialKind.GraveDust, 1 } }, "la_cabotina"),
        new MapNodeDef("triora_megalith", "Argenten Megalith Stone", "🗿", 550, 35, 0.8, 44.02, 7.65, "Argenten Valley, Liguria", new() { { MaterialKind.AstralMercury, 2 } }, "la_cabotina"),
        new MapNodeDef("triora_gorge", "Gorge of the accused", "⛰️", 1700, 120, 1.5, 43.95, 7.75, "Argentina River Gorge", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.GraveDust, 1 } }, "la_cabotina"),
        new MapNodeDef("cozumel_cenote", "Sacred Cenote of Ixchel", "💧", 200, 15, 0.5, 20.50, -86.95, "San Gervasio Cenote, Cozumel", new() { { MaterialKind.AstralMercury, 1 } }, "ixchel_priestesses"),
        new MapNodeDef("yucatan_pyramid", "Moon Pyramid of Ixchel", "🏜️", 600, 40, 0.8, 20.68, -88.57, "Chichen Itza Outskirts", new() { { MaterialKind.GraveDust, 2 } }, "ixchel_priestesses"),
        new MapNodeDef("yucatan_temple", "Tulum Cliffside Temple", "🛕", 1800, 130, 1.5, 20.21, -87.46, "Tulum Ruins, Quintana Roo", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.AstralMercury, 1 } }, "ixchel_priestesses"),
        new MapNodeDef("newforest_altar", "Bronze Age Sacrificial Altar", "🗿", 150, 10, 0.5, 50.85, -1.60, "New Forest Bronze Altar", new() { { MaterialKind.GraveDust, 1 } }, "new_forest_coven"),
        new MapNodeDef("newforest_standing", "Stonestanding Circle", "🪨", 550, 35, 0.8, 50.88, -1.55, "New Forest Standing Stones", new() { { MaterialKind.AstralMercury, 2 } }, "new_forest_coven"),
        new MapNodeDef("newforest_barrows", "Dark Barrow Burial Mounds", "⚰️", 1700, 120, 1.5, 50.82, -1.65, "New Forest Barrows", new() { { MaterialKind.DemonBile, 2 }, { MaterialKind.GraveDust, 1 } }, "new_forest_coven"));

    public static readonly ImmutableArray<PromotedRole> PromotedRoles = ImmutableArray.Create(PromotedRole.Zealot, PromotedRole.Scholar, PromotedRole.Infiltrator, PromotedRole.Mage);
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
