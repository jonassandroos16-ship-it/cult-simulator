using System.Collections.Generic;

namespace CultSimulator.Game;

/// <summary>
/// A hidden coven that is revealed as a "foothold" when the previous continent
/// in the progression is fully conquered. Each foothold unlocks the next
/// continent so the player can begin converting covens there.
/// </summary>
public record FootholdDef(
    string CovenId,
    string Continent,
    string Name,
    string Location,
    string Country,
    string CountryFlag,
    string Era,
    double Latitude,
    double Longitude,
    string Summary,
    string Lore,
    string StoryTitle,
    string StoryBody);

public static class ContinentFootholds
{
    /// <summary>
    /// Maps a completed continent to the foothold that unlocks the next one.
    /// Key = the continent that must be completed; Value = the foothold revealed.
    /// </summary>
    public static readonly Dictionary<string, FootholdDef> ForCompletedContinent = new()
    {
        ["europe"] = new FootholdDef(
            "vinland_outpost",
            "north_america",
            "Vinland Outpost",
            "L'Anse aux Meadows, Newfoundland",
            "Canada",
            "🇨🇦",
            "Viking Age",
            51.56, -55.53,
            "A longhouse of weathered timber at the edge of the known world — the first shore your Vikings touched in the west.",
            "When the last European runestone fell silent, your berserkers did not rest. They sailed west, chasing the old saga routes to Vinland. On a wind-scoured shore they raised a longhouse of sod and driftwood, planting a single bloodied runestone in the frozen earth. This is your bridge to the New World.",
            "The Vinland Shore",
            "Europe is yours. Every runestone from Skanör to Uppsala answers to your name. But the old sagas whisper of a land to the west — Vinland, where the sea ends and a new world begins.\n\nYour longest ships are loaded. Your bravest berserkers stand at the oars. They will not return. They are not meant to. Across the grey Atlantic they sail, through storms that would drown lesser men, until the fog parts and a rocky shore rises from the mist.\n\nThey wade ashore at L'Anse aux Meadows. A longhouse rises. A runestone is planted, stained red. Your foothold in the New World is claimed — and the covens of North America do not yet know what is coming for them."),

        ["north_america"] = new FootholdDef(
            "river_confluence",
            "south_america",
            "The River Confluence",
            "Confluence of the Mississippi and Ohio Rivers",
            "United States",
            "🇺🇸",
            "Present Day",
            36.96, -89.13,
            "A hidden trading post at the crossing of great rivers — the artery through which your influence flows south.",
            "Your Hudson witches traced the ancient river-trade routes used for millennia before borders existed. At the confluence of two great rivers they built a hidden waystation — a place where messages, artifacts, and acolytes flow south toward the Amazon.",
            "The River Road South",
            "North America has fallen — from Salem to Montréal, every coven bows to your order. But your witches have read the old river maps, the trade routes that connected nations long before Europeans drew lines on parchment.\n\nThe rivers flow south. So does power. You send a small cell of acolytes down the Mississippi, through bayou and delta, past the Pantanal wetlands, until they reach a confluence where two great rivers meet. There they build a hidden waystation — a node in a network that will carry your influence deep into South America.\n\nThe shamans of the Amazon will not see you coming. Not until it is too late."),

        ["south_america"] = new FootholdDef(
            "crossroads_temple",
            "africa",
            "The Crossroads Temple",
            "Ouidah, Dahomey Coast",
            "Benin",
            "🇧🇯",
            "Present Day",
            6.36, 2.09,
            "A temple of red earth and iron at the heart of the old slave-trade coast — where two worlds' magic met and bled together.",
            "During a blood-moon ritual in the Andes, your curanderos made contact with diasporic practitioners who traced their lineage across the Atlantic. The old trade routes carried people in chains — but they also carried magic. At Ouidah, where the Door of No Return stands, your agents raised a temple of red earth.",
            "The Door of Return",
            "South America is yours — from the Recta Provincia to the Andean peaks. But during a blood-moon ritual, your curanderos heard something across the water: a rhythm, a pulse, a call.\n\nThe old slave-trade routes carried people in chains across the Atlantic. But chains could not bind the magic that traveled with them. Your agents follow the current back, across the Middle Passage, to the Dahomey Coast — to Ouidah, where the Door of No Return still stands.\n\nThere they raise a temple of red earth and iron at the crossroads. The orishas of Africa have been waiting. They remember what was taken. And they are ready to deal."),

        ["africa"] = new FootholdDef(
            "incense_outpost",
            "middle_east",
            "The Incense Outpost",
            "Ancient Port of Adulis, Eritrean Coast",
            "Eritrea",
            "🇪🇷",
            "Antiquity",
            15.35, 39.40,
            "A stone relay station on the ancient incense road — where frankincense, myrrh, and darker things once flowed north.",
            "Your Axum Guardians uncovered scrolls in the old Ge'ez tongue, pointing to the Queen of Sheba's lost network — a chain of relay stations that once connected Ethiopia to Solomon's Jerusalem. Your agents traveled the ancient incense road north and rebuilt one such station at the old port of Adulis.",
            "The Incense Road",
            "Africa is conquered — from Kush to the Zulu, from the Dogon star-priests to Axum. But your Axum Guardians found something in the old ruins: scrolls in Ge'ez, describing a network of relay stations built by the Queen of Sheba herself, connecting the African coast to Solomon's kingdom in the north.\n\nThe incense road. Frankincense and myrrh once flowed along it — and darker things. Your agents travel the ancient route north, rebuilding a stone relay station at the old port of Adulis. From here, the djinn-binders and mages of the Middle East are within reach.\n\nSolomon's seal was said to bind demons. You intend to bind those who wield it."),

        ["middle_east"] = new FootholdDef(
            "caravan_spire",
            "asia",
            "The Caravan Spire",
            "Merv, Ancient Silk Road Crossroads",
            "Turkmenistan",
            "🇹🇲",
            "Medieval",
            37.66, 62.18,
            "A watchtower of fired brick on the Silk Road — the easternmost outpost of your order before the lands of the shinobi.",
            "The Hashashin shadows in your Middle Eastern covens knew the Silk Road's hidden paths — the routes that carried not just silk and spice, but secrets, assassins, and forbidden texts. At Merv, the great crossroads of the ancient world, your agents raised a spire of fired brick to watch the eastern roads.",
            "The Silk Road East",
            "The Middle East has fallen — Babylon's mages, the djinn-binders, the Hashashin shadows, all kneel to your order. But the Hashashin left you a gift before they bent: a map of the Silk Road's hidden paths, the routes that carried secrets and assassins across the ancient world.\n\nEastward the road runs, through Merv — the greatest crossroads of the medieval world — and beyond, into lands where the ninja clans and the Wu Dang immortals have ruled for a thousand years. Your agents travel the route and raise a spire of fired brick at Merv, watching the eastern roads.\n\nThe shinobi of Iga do not know that shadows can travel both directions."),

        ["asia"] = new FootholdDef(
            "wayfinder_shrine",
            "oceania",
            "The Wayfinder's Shrine",
            "Rapa Nui (Easter Island)",
            "Chile",
            "🇨🇱",
            "Pre-Colonial",
            -27.11, -109.36,
            "A shrine of carved moai stone at the most isolated inhabited place on Earth — the gateway to the dream-walkers' ocean.",
            "Your Wu Dang immortals, meditating at the Pacific's edge, made contact with Polynesian wayfinders who had sailed the world's greatest ocean for millennia using only the stars, currents, and the songs of the deep. At Rapa Nui, the most isolated point of land on Earth, they raised a shrine among the moai — a bridge between the old magic of Asia and the dream-walking of Oceania.",
            "The Endless Ocean",
            "Asia is yours — from the Iga shinobi to the Shadow Shogun, from the Kōga nightblades to the Wu Dang immortals. But your immortals, meditating at the Pacific's edge, heard something in the waves: the songs of wayfinders who had sailed the world's greatest ocean for millennia, using only stars and currents.\n\nThey followed the songs to Rapa Nui — the most isolated inhabited place on Earth — where the great stone moai stand with their backs to the sea, faces to the land. There your agents raise a shrine among the ancient heads. From here, the dream-walkers and tohunga of Oceania are within reach.\n\nThe Pacific has swallowed empires. It will not swallow yours.")
    };

    public static FootholdDef? ForContinent(string continent) =>
        ForCompletedContinent.Values.FirstOrDefault(f =>
            string.Equals(f.Continent, continent, StringComparison.OrdinalIgnoreCase));

    public static FootholdDef? ForCompleted(string continent) =>
        ForCompletedContinent.TryGetValue(continent, out var f) ? f : null;

    public static bool IsFoothold(string covenId) =>
        ForCompletedContinent.Values.Any(f => f.CovenId == covenId);

    public static WorldLocationDef ToLocation(FootholdDef f) => new(
        f.CovenId,
        f.Name,
        f.Location,
        f.Country,
        f.CountryFlag,
        f.Era,
        f.Continent,
        f.Latitude,
        f.Longitude,
        f.Summary,
        f.Lore,
        0,
        1.0,
        new List<CovenEventData>());
}
