namespace CultSimulator.Game;

public static class NumberFormat
{
    public static string Fmt(double value)
    {
        if (value < 0) return "-" + Fmt(-value);
        if (value < 10) return value.ToString("F1");
        if (value < 1000) return Math.Floor(value).ToString("F0");
        if (value < 1_000_000) return (value / 1000).ToString("F2") + "K";
        if (value < 1_000_000_000) return (value / 1_000_000).ToString("F2") + "M";
        return (value / 1_000_000_000).ToString("F2") + "B";
    }
}
