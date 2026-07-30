namespace CultSimulator.Game;

public enum InstitutionType
{
    Police,
    Media,
    Government,
    Military,
    Finance,
    Intelligence
}

public enum InstitutionTier { Tier1, Tier2, Tier3 }

public enum InstitutionStatus
{
    Locked,
    Unlocked,
    Recon,
    Infiltrating,
    Controlled,
    Alerted,
    Investigated
}
