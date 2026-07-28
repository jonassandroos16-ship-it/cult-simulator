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
    string Lore,
    int FollowersRequired,
    double BaseMultiplier);
