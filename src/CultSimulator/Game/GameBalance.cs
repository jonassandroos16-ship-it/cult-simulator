namespace CultSimulator.Game;

public static class GameBalance
{
    public const double FollowerFaithPerSec = 0.1;
    public const double FollowerGoldPerSec = 0.02;

    public const double ShrineFaithPerSec = 0.5;
    public const double CathedralGoldPerSec = 0.3;

    public const double MonolithFaithBonus = 0.25;
    public const double TreasuryGoldBonus = 0.25;
    public const double ObservatoryFaithBonus = 0.20;
    public const double ReliquaryGoldBonus = 0.20;

    public const int BankBaseCost = 500;
    public const double BankCostGrowth = 1.55;

    public static readonly double[] BankCapHours = { 2, 4, 8, 12, 24, 48 };

    public const double CovenTakeoverFaithPercent = 0.5;
    public const double CovenTakeoverGoldPercent = 0.5;
    public const double CovenTakeoverFollowerPercent = 0.3;
}
