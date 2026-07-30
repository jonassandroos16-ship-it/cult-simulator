namespace CultSimulator.Game;

public enum ContinentId
{
    Europe,
    NorthAmerica,
    SouthAmerica,
    Asia,
    Oceania,
    Africa,
    MiddleEast
}

public record ContinentTheme(
    ContinentId Id,
    string Name,
    string Icon,
    string ThemeClass,
    string PreachIcon,
    string PreachLabel,
    string AltarCore,
    string AltarMid,
    string AltarEdge,
    string AccentColor,
    string PanelBorder,
    string ActiveBorder,
    string GoldPale);

public static class ContinentThemes
{
    public static readonly Dictionary<string, ContinentTheme> ByContinent = new()
    {
        ["europe"] = new(ContinentId.Europe, "Europe", "⚔️", "theme-viking",
            "ᚦ", "INSCRIBE", "#1e3a5f", "#0c1e3d", "#050f1f",
            "#60a5fa", "rgba(96,165,250,0.15)", "rgba(96,165,250,0.3)", "#dbeafe"),

        ["north_america"] = new(ContinentId.NorthAmerica, "North America", "🗽", "theme-occult",
            "🕯️", "PREACH", "#4c1d95", "#1e1b4b", "#0f0a24",
            "#a78bfa", "rgba(139,92,246,0.15)", "rgba(245,158,11,0.3)", "#fef3c7"),

        ["south_america"] = new(ContinentId.SouthAmerica, "South America", "🌿", "theme-jungle",
            "🜃", "SUMMON", "#14532d", "#052e16", "#021c10",
            "#4ade80", "rgba(74,222,128,0.15)", "rgba(74,222,128,0.3)", "#dcfce7"),

        ["asia"] = new(ContinentId.Asia, "Asia", "🏯", "theme-ninja",
            "禅", "MEDITATE", "#7f1d1d", "#450a0a", "#1a0505",
            "#f87171", "rgba(248,113,113,0.15)", "rgba(248,113,113,0.3)", "#fee2e2"),

        ["oceania"] = new(ContinentId.Oceania, "Oceania", "🌊", "theme-ocean",
            "🌀", "CHANNEL", "#0c4a6e", "#082f49", "#031926",
            "#38bdf8", "rgba(56,189,248,0.15)", "rgba(56,189,248,0.3)", "#e0f2fe"),

        ["africa"] = new(ContinentId.Africa, "Africa", "🌍", "theme-savanna",
            "☉", "INVOKE", "#78350f", "#451a03", "#1c0a02",
            "#fbbf24", "rgba(251,191,36,0.15)", "rgba(251,191,36,0.3)", "#fef3c7"),

        ["middle_east"] = new(ContinentId.MiddleEast, "Middle East", "🕌", "theme-desert",
            "☥", "COMMUNE", "#713f12", "#422006", "#1a1003",
            "#f59e0b", "rgba(245,158,11,0.15)", "rgba(245,158,11,0.3)", "#fef3c7")
    };

    public static ContinentTheme For(string continent)
    {
        var key = continent?.ToLowerInvariant() ?? "";
        return ByContinent.TryGetValue(key, out var theme) ? theme : ByContinent["europe"];
    }

    public static readonly string[] ProgressionOrder =
    {
        "europe", "north_america", "south_america", "africa",
        "middle_east", "asia", "oceania"
    };
}