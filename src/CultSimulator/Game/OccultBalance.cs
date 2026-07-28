namespace CultSimulator.Game;

public static class OccultBalance
{
    // --- Devotion (Tier 1) scaling ---
    public const double DevotionCostGrowth = 1.15;
    public const double DevotionCostBase = 100.0;

    // --- Forbidden Knowledge (Tier 2) ---
    public const double ScholarFkPerSec = 0.5;
    public const double InfiltratorFkPerSec = 0.3;
    public const double NodeFkBasePerSec = 0.2;

    // --- Eldritch Favor (Tier 3) prestige ---
    public const double FavorDivisor = 1_000_000.0;

    // --- Suspicion ---
    public const double SuspicionMax = 100.0;
    public const double SuspicionRaidThreshold = 80.0;
    public const double SuspicionDecayPerSec = 0.5;
    public const double SuspicionHarvestBase = 0.3;

    // --- Cultist hierarchy ---
    public const int PromoteAcolyteCost = 100;
    public const int AcolyteCapBase = 200;
    public const int SacrificeFkBase = 10;
    public const double SacrificeDevotionMult = 50.0;

    // --- Grimoire sockets ---
    public const int BaseSockets = 1;
    public const int MaxSockets = 3;

    // --- Set bonuses ---
    public const double Blood3TapBonus = 2.0; // +200% click power
    public const double BloodVoidTapToTimer = 1.0; // taps reduce conquest timers

    // --- Great Seal ---
    public const double GreatSealMultiplier = 1.5;

    // --- Frenzy ---
    public const double FrenzyMultiplier = 10.0;
    public const int FrenzyDurationSec = 15;

    // --- Mass Hysteria ---
    public const int MassHysteriaDurationSec = 30;

    // --- Cauldron ---
    public const int ElixirDurationSec = 60;
}
