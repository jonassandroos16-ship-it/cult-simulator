using System.Collections.Immutable;

namespace CultSimulator.Game;

public record EnemyUnitDef(
    string RoleKey,
    string Name,
    string Icon,
    double Attack,
    double Defense,
    double Stealth,
    bool IsSupport = false,
    double FaithRegen = 0);

public static class EnemyUnitData
{
    public static readonly EnemyUnitDef Inquisitor = new(
        "inquisitor", "Inquisitor", "🗡️", 7.0, 3.5, 0.5);

    public static readonly EnemyUnitDef Templar = new(
        "templar", "Templar", "🛡️", 4.0, 7.0, 0.2);

    public static readonly EnemyUnitDef Adept = new(
        "adept", "Adept", "🧙", 2.5, 2.5, 1.0);

    public static readonly EnemyUnitDef WarPriest = new(
        "war_priest", "War Priest", "📖", 0.8, 2.0, 1.0, true, 1.8);

    public static readonly EnemyUnitDef Archmage = new(
        "archmage", "Archmage", "🔥", 14.0, 1.5, 0.3);

    public static readonly EnemyUnitDef Cultist = new(
        "cultist", "Cultist", "🕯️", 2.0, 3.0, 0.8);

    public static readonly EnemyUnitDef BloodAcolyte = new(
        "blood_acolyte", "Blood Acolyte", "🩸", 5.5, 3.0, 0.4);

    public static readonly EnemyUnitDef FleshBrute = new(
        "flesh_brute", "Flesh Brute", "🧟", 8.0, 6.0, 0.1);

    public static readonly EnemyUnitDef ShadowAgent = new(
        "shadow_agent", "Shadow Agent", "🤫", 3.5, 2.0, 3.5);

    public static readonly EnemyUnitDef MindWeaver = new(
        "mind_weaver", "Mind Weaver", "🧠", 6.0, 2.5, 2.0);

    public static readonly ImmutableArray<EnemyUnitDef> AllUnits = ImmutableArray.Create(
        Inquisitor, Templar, Adept, WarPriest, Archmage, Cultist, BloodAcolyte, FleshBrute, ShadowAgent, MindWeaver);

    public static EnemyUnitDef? Find(string roleKey) =>
        AllUnits.FirstOrDefault(u => u.RoleKey == roleKey);
}

public static class EnemyCompositionBuilder
{
    public static List<EnemyUnitSlot> BuildComposition(RivalCultArchetype archetype, double scale, double rivalPower)
    {
        var units = new List<EnemyUnitSlot>();
        int baseCount = Math.Max(2, (int)(rivalPower * 0.1 * scale));

        switch (archetype)
        {
            case RivalCultArchetype.TheOrderOfTheDawn:
                units.Add(MakeSlot(EnemyUnitData.Inquisitor, baseCount + 2, scale));
                units.Add(MakeSlot(EnemyUnitData.Templar, baseCount + 1, scale));
                units.Add(MakeSlot(EnemyUnitData.WarPriest, Math.Max(1, baseCount / 2), scale));
                if (scale > 2.0)
                    units.Add(MakeSlot(EnemyUnitData.Archmage, Math.Max(1, baseCount / 3), scale));
                break;

            case RivalCultArchetype.TheCrimsonConclave:
                units.Add(MakeSlot(EnemyUnitData.BloodAcolyte, baseCount + 3, scale));
                units.Add(MakeSlot(EnemyUnitData.FleshBrute, Math.Max(1, baseCount / 2), scale));
                units.Add(MakeSlot(EnemyUnitData.Cultist, baseCount, scale));
                break;

            case RivalCultArchetype.TheSilentChoir:
                units.Add(MakeSlot(EnemyUnitData.ShadowAgent, baseCount + 2, scale));
                units.Add(MakeSlot(EnemyUnitData.MindWeaver, baseCount + 1, scale));
                units.Add(MakeSlot(EnemyUnitData.Adept, baseCount, scale));
                if (scale > 2.0)
                    units.Add(MakeSlot(EnemyUnitData.WarPriest, Math.Max(1, baseCount / 2), scale));
                break;

            case RivalCultArchetype.TheObsidianCircle:
                units.Add(MakeSlot(EnemyUnitData.FleshBrute, baseCount + 2, scale));
                units.Add(MakeSlot(EnemyUnitData.Archmage, Math.Max(1, baseCount / 2), scale));
                units.Add(MakeSlot(EnemyUnitData.Templar, baseCount + 1, scale));
                units.Add(MakeSlot(EnemyUnitData.WarPriest, Math.Max(1, baseCount / 3), scale));
                break;
        }

        if (units.Count == 0)
        {
            units.Add(MakeSlot(EnemyUnitData.Cultist, baseCount, scale));
            units.Add(MakeSlot(EnemyUnitData.Adept, baseCount, scale));
        }

        return units;
    }

    private static EnemyUnitSlot MakeSlot(EnemyUnitDef def, int count, double scale) => new()
    {
        Name = def.Name,
        Icon = def.Icon,
        Count = Math.Max(1, count),
        MaxCount = Math.Max(1, count),
        Attack = def.Attack * scale,
        Defense = def.Defense * scale,
        Stealth = def.Stealth,
        IsSupport = def.IsSupport,
        FaithRegen = def.FaithRegen,
        RoleKey = def.RoleKey
    };

    public static double TotalAttack(List<EnemyUnitSlot> units)
    {
        double total = 0;
        foreach (var u in units) total += u.Attack * u.Count;
        return total;
    }

    public static double TotalDefense(List<EnemyUnitSlot> units)
    {
        double total = 0;
        foreach (var u in units) total += u.Defense * u.Count;
        return total;
    }

    public static double TotalStealth(List<EnemyUnitSlot> units)
    {
        int total = units.Sum(u => u.Count);
        if (total == 0) return 0;
        double stealth = 0;
        foreach (var u in units) stealth += u.Stealth * u.Count;
        return stealth / total;
    }
}
