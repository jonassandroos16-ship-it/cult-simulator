namespace CultSimulator.Game;

public static class GameBalance
{
    public const int RecruitCost = 10;
    public const string SaveKey = "cult_simulator_save_v2";
    public const string BackupSaveKey = "cult_simulator_save_v2_backup";
    public const string BackupSaveKey2 = "cult_simulator_save_v2_backup2";

    public const double FollowerFaithPerSec = 0.05;
    public const double FollowerGoldPerSec = 0.02;
    public const double ShrineFaithPerSec = 0.3;
    public const double CathedralGoldPerSec = 0.2;
    public const double MonolithFaithBonus = 0.08;
    public const double TreasuryGoldBonus = 0.08;
    public const double ObservatoryFaithBonus = 0.06;
    public const double ReliquaryGoldBonus = 0.06;
    public const double UndercroftAcolyteBonus = 2;
    public const double ShadowGuildAgentSpeedBonus = 0.15;
    public const double SafehouseAgentCapBonus = 5;

    public const double PreachFollowerScaling = 0.004;

    public const int RecruitBaseCost = 10;
    public const double RecruitCostGrowth = 1.08;

    public const int EventIntervalSeconds = 60;
    public const double EventTriggerChance = 0.5;
    public const int EventMinFollowers = 5;

    public const double CovenTakeoverFaithPercent = 0.5;
    public const double CovenTakeoverGoldPercent = 0.5;
    public const double CovenTakeoverFollowerPercent = 0.5;

    public static readonly double[] BankCapHours = { 1.0, 2.0, 5.0, 24.0, 48.0 };
    public const int BankBaseCost = 500;
    public const double BankCostGrowth = 1.55;

    public const int LocalCultSpawnIntervalSeconds = 3600;
    public const int LocalCultMaxActive = 3;

    public const int CovensPerContinent = 5;
    public const int TotalContinents = 7;
    public const int TotalCovens = 35;

    public const int AgentPoolBaseCap = 10;
}
