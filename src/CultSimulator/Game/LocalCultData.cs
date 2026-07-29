namespace CultSimulator.Game;

/// <summary>
/// Data-driven local cult definitions — 3 per parent coven, themed to
/// nearby real cities/locations. These are easier rivals that spawn
/// periodically on the local map for quick conversion opportunities.
/// </summary>
public static class LocalCultData
{
    public static IReadOnlyList<LocalCultDef> All =>
        new[]
        {
            // ── Skanör (Sweden) — Skåne region ──
            new LocalCultDef("falsterbo", "skanor", "Falsterbo Heathens",
                "Salt-worn fisher-folk who whisper to the seals and read omens in herring bones.",
                55.43, 12.82, 8, 15),
            new LocalCultDef("malmo", "skanor", "Malmö Street Circle",
                "Urban occultists meeting in basement clubs, blending old Norse runes with modern chaos magic.",
                55.60, 13.00, 12, 25),
            new LocalCultDef("lund", "skanor", "Lund Cathedral Cult",
                "Renegade scholars who stole forbidden manuscripts from the cathedral's crypt.",
                55.70, 13.19, 10, 20),

            // ── La Recta Provincia (Chile, Chiloé) ──
            new LocalCultDef("ancud", "la_recta_provincia", "Ancud Tide-Callers",
                "Coastal brujos who control the tides and fog around the fortress port of Ancud.",
                -41.87, -73.82, 18, 35),
            new LocalCultDef("castro", "la_recta_provincia", "Castro Stilt-Witches",
                "Witches who live in palafitos on the waterfront, weaving curses into their wooden houses.",
                -42.48, -73.76, 20, 40),
            new LocalCultDef("quellon", "la_recta_provincia", "Quellón Deep Ones",
                "Deep-water sorcerers at the island's southern tip who commune with things beneath the waves.",
                -43.12, -73.61, 22, 45),

            // ── Benandanti (Italy, Friuli) ──
            new LocalCultDef("udine", "benandanti", "Udine Night-Walkers",
                "Urban spirit-walkers who leave their bodies on quarter-nights to battle over the city's harvest.",
                46.07, 13.24, 50, 90),
            new LocalCultDef("gorizia", "benandanti", "Gorizia Fennel-Bearers",
                "Borderland healers who carry fennel bundles and guard the mountain passes against Malandanti.",
                45.95, 13.61, 55, 100),
            new LocalCultDef("trieste", "benandanti", "Trieste Caul-Born",
                "Adriatic sea-witches born with the caul, who read storms and navigate the spirit of the gulf.",
                45.65, 13.78, 60, 110),

            // ── Malkin Tower (England, Lancashire) ──
            new LocalCultDef("burnley", "malkin_tower_coven", "Burnley Moor-Witches",
                "Heathland cunning-folk who sell charms at the market and hex their competitors.",
                53.79, -2.24, 70, 130),
            new LocalCultDef("colne", "malkin_tower_coven", "Colne Familiar-Binders",
                "Rural hedge-witches who bind spirits to local animals — hares, cats, and black dogs.",
                53.86, -2.17, 75, 140),
            new LocalCultDef("clitheroe", "malkin_tower_coven", "Clitheroe Alchemists",
                "Castle-town alchemists who dabble in lead-to-gold transmutation and darker transformations.",
                53.87, -2.39, 80, 150),

            // ── North Berwick (Scotland, East Lothian) ──
            new LocalCultDef("haddington", "north_berwick_coven", "Haddington Storm-Callers",
                "Market-town weather-witches who brew tempests in copper cauldrons.",
                55.95, -2.78, 100, 190),
            new LocalCultDef("dunbar", "north_berwick_coven", "Dunbar Sea-Witches",
                "Harbor hags who send fog to wreck ships and collect the drowned's belongings.",
                56.00, -2.51, 110, 210),
            new LocalCultDef("prestonpans", "north_berwick_coven", "Prestonpans Salt-Sorcerers",
                "Salt-pan workers who use crystallized sea-water for scrying and binding spells.",
                55.96, -2.98, 120, 230),

            // ── La Cabotina (Italy, Liguria, Triora) ──
            new LocalCultDef("sanremo", "la_cabotina", "Sanremo Garden-Witches",
                "Riviera herbalists who grow poisonous plants among the flowers and trade in subtle deaths.",
                43.82, 7.78, 150, 280),
            new LocalCultDef("imperia", "la_cabotina", "Imperia Olive-Casters",
                "Olive-grove witches who read the future in the oil's patterns on water.",
                43.88, 7.85, 160, 300),
            new LocalCultDef("taggia", "la_cabotina", "Taggia Shape-Shifters",
                "Mountain-village shifters who become wolves to raid the lowland flocks.",
                43.85, 7.85, 170, 320),

            // ── Ixchel Priestesses (Mexico, Yucatán) ──
            new LocalCultDef("tizimin", "ixchel_priestesses", "Tizimin Bone-Readers",
                "Maya diviners who read omens in cracked tortoise shells and scattered maize seeds.",
                21.14, -88.15, 220, 420),
            new LocalCultDef("valladolid", "ixchel_priestesses", "Valladolid Cenote-Guardians",
                "Cenote-keepers who guard the sacred sinkholes and the spirits that dwell in their depths.",
                20.69, -88.20, 240, 460),
            new LocalCultDef("merida", "ixchel_priestesses", "Mérida Moon-Weavers",
                "City priestesses who weave lunar energy into cloth and sell blessings in the colonial market.",
                20.97, -89.62, 260, 500),

            // ── New Forest (England, Hampshire) ──
            new LocalCultDef("lyndhurst", "new_forest_coven", "Lyndhurst Grove-Keepers",
                "Woodland druids who tend ancient groves and speak with the old oaks.",
                50.87, -1.61, 340, 650),
            new LocalCultDef("brockenhurst", "new_forest_coven", "Brockenhurst Wild-Callers",
                "Forest witches who command the wild ponies and deer of the New Forest.",
                50.82, -1.57, 360, 700),
            new LocalCultDef("ringwood", "new_forest_coven", "Ringwood River-Hags",
                "Riverbank hags who control the Avon's flow and demand tolls from passing boats.",
                50.85, -1.79, 380, 750),
        };

    public static IReadOnlyList<LocalCultDef> ForCoven(string parentCovenId) =>
        All.Where(c => c.ParentCovenId == parentCovenId).ToList();

    public static LocalCultDef? Find(string cultId) =>
        All.FirstOrDefault(c => c.Id == cultId);
}
