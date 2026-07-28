namespace CultSimulator.Game;

public static class GameBalance
{
    public const int RecruitCost = 10;
    public const string SaveKey = "cult_simulator_save_v1";

    public const double FollowerFaithPerSec = 0.2;
    public const double FollowerGoldPerSec = 0.1;
    public const double ShrineFaithPerSec = 1.0;
    public const double CathedralGoldPerSec = 0.6;
    public const double MonolithFaithBonus = 0.10;
    public const double TreasuryGoldBonus = 0.10;

    public const int EventIntervalSeconds = 30;
    public const double EventTriggerChance = 0.5;
    public const int EventMinFollowers = 5;
}
