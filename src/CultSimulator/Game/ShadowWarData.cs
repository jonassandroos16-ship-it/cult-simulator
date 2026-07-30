using System.Collections.Immutable;

namespace CultSimulator.Game;

public static class ShadowWarData
{
    public static readonly ImmutableArray<TerritoryDef> Territories = ImmutableArray.Create(
        new TerritoryDef("europe", "Europe", "🏰", "Doubles faith from European covens", 2.0, 1.2,
            new[] { "eu_police", "eu_media", "eu_gov" }),
        new TerritoryDef("north_america", "North America", "🗽", "Doubles faith from North American covens", 2.0, 1.3,
            new[] { "na_police", "na_media", "na_gov", "na_military" }),
        new TerritoryDef("south_america", "South America", "🌴", "Doubles faith from South American covens", 2.0, 1.15,
            new[] { "sa_police", "sa_media", "sa_intel" }),
        new TerritoryDef("asia", "Asia", "🏯", "Doubles faith from Asian covens", 2.0, 1.4,
            new[] { "as_police", "as_media", "as_gov", "as_military" }),
        new TerritoryDef("oceania", "Oceania", "🦘", "Doubles faith from Oceanian covens", 2.0, 1.1,
            new[] { "oc_police", "oc_media", "oc_finance" }),
        new TerritoryDef("africa", "Africa", "🌍", "Doubles faith from African covens", 2.0, 1.15,
            new[] { "af_police", "af_media", "af_gov" }),
        new TerritoryDef("middle_east", "Middle East", "🕌", "Doubles faith from Middle Eastern covens", 2.0, 1.2,
            new[] { "me_police", "me_media", "me_military", "me_intel" })
    );

    public static readonly ImmutableArray<InstitutionDef> Institutions = ImmutableArray.Create(
        // Europe
        new InstitutionDef("eu_police", "Lund Police Station", "europe", InstitutionType.Police, InstitutionTier.Tier1, 60, 1.5, 0.03, "Suspicion decay +0.5/s", 0.5, "A small-town precinct. Easy to infiltrate.", null),
        new InstitutionDef("eu_media", "Nordic Broadcasting Corp", "europe", InstitutionType.Media, InstitutionTier.Tier2, 280, 3.0, 0.08, "Global detection rate -10%", 0.1, "Regional news network.", new[] { "eu_police" }),
        new InstitutionDef("eu_gov", "European Parliament", "europe", InstitutionType.Government, InstitutionTier.Tier3, 600, 4.0, 0.12, "Agent recruitment +25%", 0.25, "The heart of European governance.", new[] { "eu_media" }),

        // North America
        new InstitutionDef("na_police", "NYPD Intelligence Bureau", "north_america", InstitutionType.Police, InstitutionTier.Tier1, 140, 2.8, 0.06, "Suspicion decay +0.8/s", 0.8, "A major-city police force.", null),
        new InstitutionDef("na_media", "Continental News Network", "north_america", InstitutionType.Media, InstitutionTier.Tier2, 350, 3.2, 0.09, "Global detection rate -12%", 0.12, "A coast-to-coast cable news empire.", new[] { "na_police" }),
        new InstitutionDef("na_gov", "Federal Government", "north_america", InstitutionType.Government, InstitutionTier.Tier3, 800, 4.5, 0.15, "Agent recruitment +30%", 0.3, "The executive branch of a superpower.", new[] { "na_media" }),
        new InstitutionDef("na_military", "Northern Command", "north_america", InstitutionType.Military, InstitutionTier.Tier3, 900, 5.0, 0.18, "Agent combat strength +40%", 0.4, "A unified military command.", new[] { "na_gov" }),

        // South America
        new InstitutionDef("sa_police", "Border Constabulary", "south_america", InstitutionType.Police, InstitutionTier.Tier1, 110, 2.3, 0.05, "Suspicion decay +0.6/s", 0.6, "A border patrol force.", null),
        new InstitutionDef("sa_media", "National Broadcaster", "south_america", InstitutionType.Media, InstitutionTier.Tier2, 260, 2.8, 0.07, "Global detection rate -8%", 0.08, "State-run media.", new[] { "sa_police" }),
        new InstitutionDef("sa_intel", "National Intelligence Service", "south_america", InstitutionType.Intelligence, InstitutionTier.Tier3, 550, 4.2, 0.14, "Recon risk -30% globally", 0.3, "A shadowy intelligence apparatus.", new[] { "sa_media" }),

        // Asia
        new InstitutionDef("as_police", "Metropolitan Police Bureau", "asia", InstitutionType.Police, InstitutionTier.Tier1, 160, 2.6, 0.06, "Suspicion decay +0.7/s", 0.7, "A massive metropolitan police force.", null),
        new InstitutionDef("as_media", "Digital Media Conglomerate", "asia", InstitutionType.Media, InstitutionTier.Tier2, 400, 3.5, 0.10, "Global detection rate -15%", 0.15, "A tech-driven media empire.", new[] { "as_police" }),
        new InstitutionDef("as_gov", "Central Committee", "asia", InstitutionType.Government, InstitutionTier.Tier3, 850, 4.8, 0.16, "Agent recruitment +35%", 0.35, "A centralized single-party government.", new[] { "as_media" }),
        new InstitutionDef("as_military", "Eastern Military Region", "asia", InstitutionType.Military, InstitutionTier.Tier3, 1000, 5.5, 0.20, "Agent combat strength +50%", 0.5, "The largest standing military on earth.", new[] { "as_gov" }),

        // Oceania
        new InstitutionDef("oc_police", "Federal Police Service", "oceania", InstitutionType.Police, InstitutionTier.Tier1, 120, 2.4, 0.05, "Suspicion decay +0.5/s", 0.5, "A continent-wide federal police agency.", null),
        new InstitutionDef("oc_media", "Southern Media Group", "oceania", InstitutionType.Media, InstitutionTier.Tier2, 300, 3.0, 0.08, "Global detection rate -10%", 0.1, "A media network spanning the southern hemisphere.", new[] { "oc_police" }),
        new InstitutionDef("oc_finance", "Pacific Financial Hub", "oceania", InstitutionType.Finance, InstitutionTier.Tier3, 650, 4.0, 0.12, "Faith production +20% globally", 0.2, "A global financial center.", new[] { "oc_media" }),

        // Africa
        new InstitutionDef("af_police", "Colonial Constabulary", "africa", InstitutionType.Police, InstitutionTier.Tier1, 100, 2.2, 0.04, "Suspicion decay +0.5/s", 0.5, "A legacy colonial-era police force.", null),
        new InstitutionDef("af_media", "Continental Radio Network", "africa", InstitutionType.Media, InstitutionTier.Tier2, 240, 2.7, 0.07, "Global detection rate -8%", 0.08, "A radio network reaching the entire continent.", new[] { "af_police" }),
        new InstitutionDef("af_gov", "Continental Union Assembly", "africa", InstitutionType.Government, InstitutionTier.Tier3, 580, 4.0, 0.13, "Agent recruitment +25%", 0.25, "A pan-continental political body.", new[] { "af_media" }),

        // Middle East
        new InstitutionDef("me_police", "Secret Police Directorate", "middle_east", InstitutionType.Police, InstitutionTier.Tier1, 150, 2.7, 0.07, "Suspicion decay +0.9/s", 0.9, "A feared secret police apparatus.", null),
        new InstitutionDef("me_media", "State Media Authority", "middle_east", InstitutionType.Media, InstitutionTier.Tier2, 320, 3.2, 0.09, "Global detection rate -12%", 0.12, "A state-controlled media authority.", new[] { "me_police" }),
        new InstitutionDef("me_military", "Central Military Command", "middle_east", InstitutionType.Military, InstitutionTier.Tier3, 750, 5.0, 0.17, "Agent combat strength +35%", 0.35, "A strategically critical military command.", new[] { "me_media" }),
        new InstitutionDef("me_intel", "Global Intelligence Bureau", "middle_east", InstitutionType.Intelligence, InstitutionTier.Tier3, 700, 4.5, 0.15, "Recon risk -25% globally", 0.25, "A cross-border intelligence network.", new[] { "me_military" })
    );

    public static TerritoryDef? Territory(string id) => Territories.FirstOrDefault(t => t.Id == id);
    public static InstitutionDef? Institution(string id) => Institutions.FirstOrDefault(i => i.Id == id);
    public static IReadOnlyList<InstitutionDef> InstitutionsForTerritory(string territoryId) =>
        Institutions.Where(i => i.TerritoryId == territoryId).ToList();
}
