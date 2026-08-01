using System.Collections.Immutable;

namespace CultSimulator.Game;

public static class GameData
{
    public static readonly ImmutableArray<BuildingDef> Buildings = ImmutableArray.Create(
        new BuildingDef(BuildingType.Shrine, "Shrine", "⛩️", 50, ResourceKind.Faith, 1.15, "Generates passive Faith"),
        new BuildingDef(BuildingType.Cathedral, "Cathedral", "⛪", 200, ResourceKind.Faith, 1.18, "Generates passive Gold"),
        new BuildingDef(BuildingType.Bank, "Bank", "🏦", 500, ResourceKind.Gold, 1.55, "Increases idle income cap"),
        new BuildingDef(BuildingType.Monolith, "Monolith", "🗿", 500, ResourceKind.Faith, 1.22, "+25% Faith production"),
        new BuildingDef(BuildingType.Treasury, "Treasury", "💰", 350, ResourceKind.Gold, 1.20, "+25% Gold production"),
        new BuildingDef(BuildingType.Observatory, "Observatory", "🔭", 800, ResourceKind.Faith, 1.25, "+20% Faith production"),
        new BuildingDef(BuildingType.Reliquary, "Reliquary", "🏺", 600, ResourceKind.Gold, 1.22, "+20% Gold production"),
        new BuildingDef(BuildingType.Undercroft, "Undercroft", "🕳️", 1200, ResourceKind.Faith, 1.30, "+50% offline income"));

    public static readonly ImmutableArray<UpgradeDef> Upgrades = ImmutableArray.Create(
        new UpgradeDef(UpgradeId.Hymnal, "Sacred Hymnal", "📜", 120, 0, "Preaching yields 2× Faith", 0),
        new UpgradeDef(UpgradeId.Relics, "Golden Relics", "🏺", 0, 250, "Followers give 2× Gold", 15),
        new UpgradeDef(UpgradeId.Visions, "Prophetic Visions", "🔮", 600, 0, "Followers give 2× Faith", 40),
        new UpgradeDef(UpgradeId.Ascendance, "Rite of Ascendance", "🌟", 1500, 1000, "All production ×1.5", 120),
        new UpgradeDef(UpgradeId.BankVault, "Iron Vault", "🔒", 500, 2000, "Bank idle cap ×1.5", 10),
        new UpgradeDef(UpgradeId.OffshoreAccounts, "Shadow Ledger", "📓", 2000, 6000, "Bank idle cap ×1.5", 30),
        new UpgradeDef(UpgradeId.DarkLedger, "Blood Pact", "🩸", 5000, 20000, "Bank idle cap ×2", 60),
        new UpgradeDef(UpgradeId.SoulEndowment, "Eternal Reserves", "💀", 15000, 60000, "Bank idle cap ×2", 120));

    public static readonly ImmutableArray<UpgradeId> BankUpgrades = ImmutableArray.Create(
        UpgradeId.BankVault,
        UpgradeId.OffshoreAccounts,
        UpgradeId.DarkLedger,
        UpgradeId.SoulEndowment);
}
