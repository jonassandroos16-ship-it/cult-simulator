using System.Collections.Immutable;

namespace CultSimulator.Game;

/// <summary>
/// A real-world location tied to historical covens, magical orders, and sorcery guilds.
/// Coordinates are provided so a world map can place markers for each location.
/// </summary>
public record WorldLocationDef(
    string Id,
    string Name,
    string Location,
    string Country,
    string CountryFlag,
    string Era,
    double Latitude,
    double Longitude,
    string Summary,
    string Lore);

public static class WorldLocations
{
    public static readonly ImmutableArray<WorldLocationDef> Locations = ImmutableArray.Create(
        new WorldLocationDef(
            "la_recta_provincia",
            "La Recta Provincia",
            "Chiloé Archipelago",
            "Chile",
            "🇨🇱",
            "18th – late 19th century",
            -42.5000,
            -73.8000,
            "A shadowy brotherhood of indigenous Huilliche and mestizo sorcerers (brujos) that operated as an underground legal system on the remote island of Chiloé.",
            "La Recta Provincia functioned like a shadowy government. According to trial documents from 1880, its leaders gathered in a hidden underground chamber called the Cueva de Quicaví. Legend says the cave was guarded by the Imbunche—a stolen child transformed through dark magic into a twisted, deformed beast. The order used magical grimoires (revisorios), communicated via glowing nocturnal signals, and levied taxes or 'protection insurance' on locals until the Chilean state brought them to trial."),
        new WorldLocationDef(
            "benandanti",
            "The Benandanti",
            "Friuli Region, Northeastern Italy",
            "Italy",
            "🇮🇹",
            "16th – 17th century",
            46.0500,
            13.0000,
            "An agrarian spirit-guild who believed they were chosen at birth to battle malevolent sorcerers in the astral plane.",
            "The Benandanti were born with a caul (amniotic sac) over their heads—a sign they possessed innate spiritual powers. Four times a year, during the Ember Days, their souls were said to leave their sleeping bodies in the forms of small animals (cats, hares, or butterflies). They flew to distant spiritual battlefields to clash with the Malandanti (evil witches). The Benandanti fought with bundles of fennel, while the bad witches wielded sorghum brooms. If the Good Walkers won, the harvest was saved; if they lost, famine followed."),
        new WorldLocationDef(
            "malkin_tower_coven",
            "The Malkin Tower Coven",
            "Lancashire, England",
            "United Kingdom",
            "🇬🇧",
            "1612",
            53.8700,
            -2.3100,
            "The site of one of the most famous alleged coven gatherings in English history, set against the moody backdrop of Pendle Hill.",
            "On Good Friday in 1612, a group gathered at Malkin Tower, the home of Elizabeth Southerns ('Old Demdike'). Two rival families of folk healers and cunning folk had assembled for a feast. Local magistrate Roger Nowell claimed it was a secret coven plotting to blow up Lancaster Castle and free imprisoned matriarchs. The meeting triggered the notorious Pendle Witch Trials, resulting in ten executions and shaping the archetype of the English witch."),
        new WorldLocationDef(
            "north_berwick_coven",
            "The North Berwick Coven",
            "East Lothian, Scotland",
            "United Kingdom",
            "🏴󠁧󠁢󠁳󠁣󠁴󠁿",
            "1590–1592",
            56.0000,
            -2.7200,
            "A massive gathering accused of using high-seas weather magic to target royal shipping.",
            "Over 70 people were accused of assembling at the ruined St Andrew's Auld Kirk on the Scottish coast. Led by a respected local midwife and healer named Agnes Sampson and a schoolmaster named John Fian, the circle was accused of brewing storms by drowning cats in sea-water to sink the ship carrying King James VI and his bride, Anne of Denmark. King James became so terrified by the trial details that he personally interrogated suspects and authored Daemonologie, his famous book on hunting witches."),
        new WorldLocationDef(
            "la_cabotina",
            "The Circle of La Cabotina",
            "Triora, Liguria",
            "Italy",
            "🇮🇹",
            "1587–1589",
            43.9800,
            7.7000,
            "Perched high in the Ligurian Alps, the medieval village of Triora is often called the 'Salem of Italy.'",
            "When a devastating famine struck the region in 1587, local authorities blamed a gathering of women who met at La Cabotina—a desolate, cavernous rock formation outside the village walls. The women were accused of holding late-night rites, manipulating crop yields, and shape-shifting. Today, Triora embraces this dark chapter of history with preserved medieval architecture and local lore dedicated to the Cabotina circle."),
        new WorldLocationDef(
            "ixchel_priestesses",
            "The Ixchel Priestesses of Cozumel",
            "Isla Cozumel & Yucatán Peninsula",
            "Mexico",
            "🇲🇽",
            "Pre-Columbian to Early Colonial Period",
            20.5000,
            -86.9500,
            "Sacred female circles dedicated to the Maya goddess of medicine, midwifery, weaving, and the moon.",
            "Long before Spanish contact, women traveled from across Mesoamerica to the coastal island of Cozumel (Isla Mujeres or 'Island of Women'). The priestess circles maintained secret knowledge of botanical medicine, venom-neutralizing remedies, and lunar rituals. Following the Spanish conquest, these indigenous ritual networks went underground, blending ancient Maya healing with European folk practices to form early syncretic brujería traditions."),
        new WorldLocationDef(
            "new_forest_coven",
            "The New Forest Coven",
            "Hampshire, England",
            "United Kingdom",
            "🇬🇧",
            "Early 20th century",
            50.8500,
            -1.6000,
            "The mysterious group credited with launching the modern Wiccan revival.",
            "According to Gerald Gardner (the founder of modern Wicca), he was initiated into a secret, surviving coven in the New Forest region during the late 1930s. Led by a high priestess known in lore as 'Old Dorothy,' the group claimed to be practicing an unbroken lineage of ancient Pagan religion preserved through centuries of secrecy. In 1940, the coven reportedly performed 'Operation Cone of Power'—a massive collective ritual in the woods intended to psychically stop Hitler's forces from invading Britain."));
}
