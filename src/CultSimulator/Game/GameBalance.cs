namespace CultSimulator.Game;

public static class GameBalance
{
    public const int RecruitCost = 10;
    public const string SaveKey = "cult_simulator_save_v1";

    // Passive income — reduced so progression takes longer
    public const double FollowerFaithPerSec = 0.05;
    public const double FollowerGoldPerSec = 0.02;
    public const double ShrineFaithPerSec = 0.3;
    public const double CathedralGoldPerSec = 0.2;
    public const double MonolithFaithBonus = 0.08;
    public const double TreasuryGoldBonus = 0.08;

    // Preaching — reduced follower scaling so faith gain is slower
    public const double PreachFollowerScaling = 0.004;

    // Recruit — cost scales up as the coven grows
    public const int RecruitBaseCost = 10;
    public const double RecruitCostGrowth = 1.08;

    public const int EventIntervalSeconds = 30;
    public const double EventTriggerChance = 0.5;
    public const int EventMinFollowers = 5;

    public const double CovenTakeoverFaithPercent = 0.5;
    public const double CovenTakeoverGoldPercent = 0.5;
    public const double CovenTakeoverFollowerPercent = 0.5;

    // Bank building — idle/offline income cap tiers (in hours)
    public static readonly double[] BankCapHours = { 1.0, 2.0, 5.0, 24.0, 48.0 };
    public const int BankBaseCost = 200;
    public const double BankCostGrowth = 1.25;
}
