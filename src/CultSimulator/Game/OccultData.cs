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
        new TechDef(TechId.LeyLineWeaving, "Ley Line Weaving", "📐", 2500, TechBranch.MindCoercion, "+25% Great Seal multiplier", new[] { TechId.InquisitorsBlindfold }),
        new TechDef(TechId.MassHysteria, "Mass Hysteria", "😱", 15000, TechBranch.MindCoercion, "Tapping map regions generates Faith directly for 30s", new[] { TechId.LeyLineWeaving }),
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
        new ArtifactDef("mind_crown", "Crown of Madness", "👑", ArtifactSuit.Mind, "+30% Great Seal multiplier"),
        new ArtifactDef("flesh_golem", "Flesh Golem", "🧟", ArtifactSuit.Flesh, "+50 Acolyte cap"),
        new ArtifactDef("flesh_graft", "Sinew Graft", "🫀", ArtifactSuit.Flesh, "+15% all production"),
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
        new CauldronRecipeDef(CauldronRecipeId.FleshBrew, "Flesh Brew", "🫕", new() { { MaterialKind.GraveDust, 1 }, { MaterialKind.DemonBile, 1 }, { MaterialKind.AstralMercury, 1 } }, "+100 Acolytes instantly", false),
        new CauldronRecipeDef(CauldronRecipeId.BloodForge, "Blood Forge", "🍷", new() { { MaterialKind.GraveDust, 5 }, { MaterialKind.DemonBile, 2 } }, "Forge a random Blood artifact", true),
        new CauldronRecipeDef(CauldronRecipeId.VoidForge, "Void Forge", "🔮", new() { { MaterialKind.DemonBile, 5 }, { MaterialKind.AstralMercury, 2 } }, "Forge a random Void artifact", true),
        new CauldronRecipeDef(CauldronRecipeId.MindForge, "Mind Forge", "👁️", new() { { MaterialKind.AstralMercury, 5 }, { MaterialKind.GraveDust, 2 } }, "Forge a random Mind artifact", true),
        new CauldronRecipeDef(CauldronRecipeId.FleshForge, "Flesh Forge", "🧟", new() { { MaterialKind.GraveDust, 3 }, { MaterialKind.DemonBile, 3 }, { MaterialKind.AstralMercury, 3 } }, "Forge a random Flesh artifact", true));

    public static readonly ImmutableArray<MapNodeDef> MapNodes = ImmutableArray.Create(
        new MapNodeDef("old_library", "Forbidden Library", "📚", 150, 10, 0.5, 0.2, 41.9028, 12.4964, "Vatican Archives, Rome", new() { { MaterialKind.AstralMercury, 1 } }),
        new MapNodeDef("ancient_ruins", "Ancient Ruins", "🏚️", 600, 40, 0.8, 0.4, 37.2233, 38.9224, "Göbekli Tepe, Turkey", new() { { MaterialKind.GraveDust, 2 } }),
        new MapNodeDef("ley_intersection", "Ley Line Nexus", "⚡", 2000, 150, 1.5, 0.6, 27.1750, 78.0422, "Taj Mahal, Agra", new() { { MaterialKind.AstralMercury, 2 }, { MaterialKind.DemonBile, 1 } }),
        new MapNodeDef("blood_temple", "Blood Temple", "🩸", 5000, 400, 2.5, 1.0, 45.3744, 12.3260, "Poveglia Island, Italy", new() { { MaterialKind.DemonBile, 3 } }),
        new MapNodeDef("void_rift", "Void Rift", "🌀", 10000, 800, 5.0, 1.5, 25.0, -71.0, "Bermuda Triangle", new() { { MaterialKind.DemonBile, 5 }, { MaterialKind.AstralMercury, 3 } }),
        new MapNodeDef("flesh_pit", "Flesh Pit", "🦠", 20000, 1500, 10.0, 2.0, -13.1631, -72.5450, "Nazca Lines, Peru", new() { { MaterialKind.GraveDust, 10 }, { MaterialKind.DemonBile, 5 }, { MaterialKind.AstralMercury, 5 } }));

    public static readonly ImmutableArray<PromotedRole> PromotedRoles = ImmutableArray.Create(PromotedRole.Zealot, PromotedRole.Scholar, PromotedRole.Infiltrator);
    public static readonly ImmutableArray<CouncilRole> CouncilRoles = ImmutableArray.Create(CouncilRole.Inquisitor, CouncilRole.Archon, CouncilRole.HighPriest);

    public static readonly ImmutableArray<AgentDef> Agents = ImmutableArray.Create(
        new AgentDef(AgentType.Scout, "Scout", "🔍", 10, "+0.3 Faith/s per Scout, reveals nearby threats", 0.3, 0.0, 0.0),
        new AgentDef(AgentType.Saboteur, "Saboteur", "💣", 25, "-0.1 Suspicion/s per Saboteur, disrupts enemy raids", 0.0, -0.1, 0.0),
        new AgentDef(AgentType.Diplomat, "Diplomat", "🤝", 40, "+0.5 Faith/s and +0.1 Army Power/s per Diplomat", 0.5, 0.0, 0.1),
        new AgentDef(AgentType.Warmonger, "Warmonger", "⚔️", 80, "+1.0 Army Power/s per Warmonger, +2% raid power", 0.0, 0.0, 1.0),
        new AgentDef(AgentType.Corrupter, "Corrupter", "🌑", 150, "+1.5 Faith/s and -0.05 Suspicion/s per Corrupter", 1.5, -0.05, 0.0));

    public static TechDef Tech(TechId id) => Techs.First(t => t.Id == id);
    public static ArtifactDef? Artifact(string id) => Artifacts.FirstOrDefault(a => a.Id == id);
    public static MinionTraitDef? Trait(string id) => Traits.FirstOrDefault(t => t.Id == id);
    public static CauldronRecipeDef Recipe(CauldronRecipeId id) => Recipes.First(r => r.Id == id);
    public static MapNodeDef? MapNode(string id) => MapNodes.FirstOrDefault(n => n.Id == id);
    public static AgentDef Agent(AgentType type) => Agents.First(a => a.Type == type);
    public static ShadowAgent? GetShadowAgent(OccultState o, AgentType type) => o.ShadowAgents.FirstOrDefault(a => a.Type == type);
}
