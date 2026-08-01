namespace CultSimulator.Game;

public static class OccultBalance
{
    public const double SermonCostGrowth = 1.15;
    public const double SermonCostBase = 100.0;

    public const double InitiateCostBase = 50.0;
    public const double InitiateCostGrowth = 1.04;
    public const double ScholarFaithPerSec = 0.5;
    public const double InfiltratorFaithPerSec = 0.3;
    public const double ZealotAgentProdBonusPerSec = 0.02;
    public const double NodeFaithBasePerSec = 0.2;
    public const double FavorDivisor = 1_000_000.0;
    public const double SuspicionMax = 100.0;
    public const double SuspicionRaidThreshold = 80.0;
    public const double SuspicionDecayPerSec = 0.1;
    public const double SuspicionHarvestBase = 0.3;
    public const int RecruitUnitCost = 100;
    public const int InitiateCapBase = 200;
    public const double SacrificeFaithBase = 10.0;
    public const double SacrificeSermonMult = 50.0;
    public const int BaseSockets = 1;
    public const int MaxSockets = 3;
    public const double Blood3TapBonus = 2.0;
    public const double BloodVoidTapToTimer = 1.0;
    public const double GreatSealMultiplier = 1.5;
    public const double FrenzyMultiplier = 10.0;
    public const int FrenzyDurationSec = 15;
    public const int MassHysteriaDurationSec = 30;
    public const int ElixirDurationSec = 60;
    public const int WarElixirDurationSec = 120;
    public const double InitiateSacrificeSuspicionReduction = 10.0;
    public const int DarkVigilDurationSec = 45;
    public const int WhisperChoirDurationSec = 60;
    public const int CovenBlessingDurationSec = 90;

    // Legacy aliases so any existing code that referenced Acolyte names still compiles
    public const int PromoteAcolyteCost = RecruitUnitCost;
    public const double AcolyteSacrificeSuspicionReduction = InitiateSacrificeSuspicionReduction;
    public const int AcolyteCapBase = InitiateCapBase;
    public const double AcolyteCostBase = InitiateCostBase;
    public const double AcolyteCostGrowth = InitiateCostGrowth;
}
