namespace CultSimulator.Game;

public enum BuildingType { Shrine, Cathedral, Monolith, Treasury, Bank, Observatory, Reliquary, Undercroft, ShadowGuild, Safehouse }

public enum UpgradeId
{
    // Original Sacred Rites
    Hymnal, Relics, Visions, Ascendance,
    // Original Bank Rites
    BankVault, OffshoreAccounts, DarkLedger, SoulEndowment,
    // New Sacred Rites
    SacredFlame, TitheSystem, PilgrimNetwork, DivineMandate,
    GoldenIdol, SoulHarvest, EternalFlame, Apotheosis
}

public enum ResourceKind { Faith, Gold }
