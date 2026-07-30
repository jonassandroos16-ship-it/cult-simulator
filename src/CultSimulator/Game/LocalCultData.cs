using System.Collections.Generic;

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
            // ── Skanör (Sweden) — Skåne region ── Viking
            new LocalCultDef("falsterbo", "skanor", "Falsterbo Heathens",
                "Salt-worn fisher-folk who whisper to the seals and read omens in herring bones.",
                55.43, 12.82, 8, 15),
            new LocalCultDef("malmo", "skanor", "Malmö Street Circle",
                "Urban occultists meeting in basement clubs, blending old Norse runes with modern chaos magic.",
                55.60, 13.00, 12, 25),
            new LocalCultDef("lund", "skanor", "Lund Cathedral Cult",
                "Renegade scholars who stole forbidden manuscripts from the cathedral's crypt.",
                55.70, 13.19, 10, 20),

            // ── Uppsala Gothi (Sweden) ── Viking
            new LocalCultDef("gavle", "uppsala_gothi", "Gävle River-Readers",
                "River-mouth diviners who read the future in the pattern of salmon runs and ice floes.",
                60.68, 17.16, 18, 35),
            new LocalCultDef("vasteras", "uppsala_gothi", "Västerås Mound-Keepers",
                "Custodians of burial mounds who speak with the dead kings entombed within.",
                59.61, 16.55, 20, 40),
            new LocalCultDef("orebro", "uppsala_gothi", "Örebro Iron-Cult",
                "Smith-cultists who forge runic blades and believe iron holds the memory of blood.",
                59.27, 15.21, 22, 45),

            // ── Hedeby Vikings (Denmark) ── Viking
            new LocalCultDef("schleswig", "hedeby_vikings", "Schleswig Traders",
                "Merchant-cultists who worship profit and the dark bargains that come with it.",
                54.52, 9.57, 35, 70),
            new LocalCultDef("ripsen", "hedeby_vikings", "Ribe Bone-Readers",
                "Denmark's oldest town harbors diviners who cast animal bones in the market square.",
                55.33, 8.76, 40, 80),
            new LocalCultDef("aarhus", "hedeby_vikings", "Aarhus Harbor-Witches",
                "Port-witches who curse ships that refuse to pay tribute and bless those that do.",
                56.16, 10.21, 45, 90),

            // ── Trossky Berserkers (Norway) ── Viking
            new LocalCultDef("larvik", "trossky_berserkers", "Larvik Bear-Channelers",
                "Coastal warriors who channel bear-spirits and fight with inhuman strength.",
                59.05, 10.05, 55, 110),
            new LocalCultDef("notodden", "trossky_berserkers", "Notodden Frost-Mages",
                "Highland mages who command frost and ice, freezing rivers with a word.",
                59.55, 9.27, 60, 120),
            new LocalCultDef("skien", "trossky_berserkers", "Skien Wolf-Brothers",
                "Wolf-cultists who run with packs in the deep forests and return changed.",
                59.21, 9.61, 65, 130),

            // ── Jomsborg Elite (Poland) ── Viking
            new LocalCultDef("kamien", "jomsborg_elite", "Kamień Coastal-Guardians",
                "Cliff-top sentinels who ward the coast with old sea-magic and iron resolve.",
                53.97, 14.27, 80, 160),
            new LocalCultDef("szczecin", "jomsborg_elite", "Szczecin River-Witches",
                "River-witches who control the Oder's flow and the trade that moves along it.",
                53.43, 14.55, 90, 180),
            new LocalCultDef("kolobrzeg", "jomsborg_elite", "Kołobrzeg Salt-Sorcerers",
                "Salt-pan workers who crystallize magic into preserving jars of power.",
                54.18, 15.58, 100, 200),

            // ── Salem Remnant (USA, Massachusetts) ── Modern Occult
            new LocalCultDef("beverly", "salem_remnant", "Beverly Suburb-Coven",
                "Suburban witches who hide their craft behind PTA meetings and book clubs.",
                42.56, -70.88, 120, 240),
            new LocalCultDef("marblehead", "salem_remnant", "Marblehead Yacht-Club Occultists",
                "Old-money occultists who weave spells into their regatta traditions.",
                42.50, -70.85, 130, 260),
            new LocalCultDef("peabody", "salem_remnant", "Peabody Mall-Witches",
                "Retail-coven members who enchant products and curse competitors' inventory.",
                42.53, -70.93, 140, 280),

            // ── Voodoo Quarter (USA, New Orleans) ── Modern Occult
            new LocalCultDef("metairie", "voodoo_quarter", "Metairie Cemetery-Keepers",
                "Cemetery custodians who commune with the spirits interred in the old tombs.",
                29.99, -90.15, 160, 320),
            new LocalCultDef("algiers", "voodoo_quarter", "Algiers Point Crossroads-Society",
                "Riverside society that guards a crossroads where three loa are said to meet.",
                29.92, -90.05, 170, 340),
            new LocalCultDef("gentilly", "voodoo_quarter", "Gentilly Swamp-Walkers",
                "Swamp-edge walkers who navigate the bayou's spirit-paths at midnight.",
                30.03, -90.08, 180, 360),

            // ── Silicon Circle (USA, California) ── Modern Occult
            new LocalCultDef("mountainview", "silicon_circle", "Mountain View Data-Coven",
                "Tech-cultists who mine user data for sacrificial algorithms.",
                37.39, -122.08, 220, 440),
            new LocalCultDef("menlopark", "silicon_circle", "Menlo Park Venture-Warlocks",
                "VC warlocks who trade in soul-equity and demonic term sheets.",
                37.45, -122.18, 240, 480),
            new LocalCultDef("stanford", "silicon_circle", "Stanford Crypto-Cult",
                "Academic occultists who publish papers on consciousness and practice the real thing in secret.",
                37.43, -122.17, 260, 520),

            // ── Hudson Witches (USA, New York) ── Modern Occult
            new LocalCultDef("kingston", "hudson_witches", "Kingston Stone-Circle",
                "Colonial-era witches who found a stone circle older than the city itself.",
                41.93, -73.99, 320, 640),
            new LocalCultDef("rhinebeck", "hudson_witches", "Rhinebeck Estate-Coven",
                "Old-money coven in a crumbling estate with a cellar that goes deeper than the foundation.",
                41.93, -73.91, 340, 680),
            new LocalCultDef("newburgh", "hudson_witches", "Newburgh Urban-Mystics",
                "Urban mystics who find power in abandoned buildings and forgotten infrastructure.",
                41.50, -74.01, 360, 720),

            // ── Montréal Night Coven (Canada) ── Modern Occult
            new LocalCultDef("laval", "montreal_night", "Laval Subterranean-Cult",
                "Suburban cultists who discovered a passage to the old tunnels beneath their basements.",
                45.57, -73.75, 420, 840),
            new LocalCultDef("longueuil", "montreal_night", "Longueuil Frost-Witches",
                "South-shore witches who bind spirits into ice sculptures that last through spring.",
                45.53, -73.53, 440, 880),
            new LocalCultDef("lachine", "montreal_night", "Lachine Canal-Spirits",
                "Canal-side spirit-talkers who read the future in the frozen water's cracks.",
                45.43, -73.68, 460, 920),

            // ── La Recta Provincia (Chile, Chiloé) ── Jungle
            new LocalCultDef("ancud", "la_recta_provincia", "Ancud Tide-Callers",
                "Coastal brujos who control the tides and fog around the fortress port of Ancud.",
                -41.87, -73.82, 520, 1040),
            new LocalCultDef("castro", "la_recta_provincia", "Castro Stilt-Witches",
                "Witches who live in palafitos on the waterfront, weaving curses into their wooden houses.",
                -42.48, -73.76, 540, 1080),
            new LocalCultDef("quellon", "la_recta_provincia", "Quellón Deep Ones",
                "Deep-water sorcerers at the island's southern tip who commune with things beneath the waves.",
                -43.12, -73.61, 560, 1120),

            // ── Amazon Curanderos (Peru) ── Jungle
            new LocalCultDef("pebas", "amazon_curanderos", "Pebas River-Spirits",
                "River-village shamans who speak with the pink dolphins and the spirits of the tributaries.",
                -3.32, -72.53, 680, 1360),
            new LocalCultDef("nauta", "amazon_curanderos", "Nauta Ayahuasca-Guides",
                "Vision-guides who navigate the spirit-forest and bring back knowledge from the other side.",
                -4.50, -73.59, 720, 1440),
            new LocalCultDef("contamana", "amazon_curanderos", "Contamana Plant-Speakers",
                "Deep-jungle herbalists who speak the language of every plant and tree in their territory.",
                -5.94, -75.28, 760, 1520),

            // ── Andean Pachakuna (Peru, Cusco) ── Jungle
            new LocalCultDef("ollantaytambo", "andean_pacha", "Ollantaytambo Stone-Readers",
                "Valley priests who read the future in the carved stones of the sacred fortress.",
                -13.26, -72.26, 900, 1800),
            new LocalCultDef("pisac", "andean_pacha", "Pisac Terraced-Cult",
                "Terrace-farmers who grow sacred coca and channel the mountain's power through their crops.",
                -13.42, -71.87, 950, 1900),
            new LocalCultDef("urubamba", "andean_pacha", "Urubamba River-Spirits",
                "River-spirits who guard the sacred valley and test those who would pass through.",
                -13.31, -72.12, 1000, 2000),

            // ── Pantanal Feiticeira (Brazil) ── Jungle
            new LocalCultDef("corumba", "pantanal_feiticeira", "Corumbá Swamp-Witches",
                "Border-town witches who blend Brazilian and Bolivian magic in the flooded plains.",
                -19.01, -57.65, 1200, 2400),
            new LocalCultDef("aquidauana", "pantanal_feiticeira", "Aquidauana Caiman-Callers",
                "Swamp-cultists who command the caimans and ride them through the flooded fields.",
                -20.47, -55.79, 1300, 2600),
            new LocalCultDef("miranda", "pantanal_feiticeira", "Miranda Spirit-Hunters",
                "Wetland hunters who track spirits through the reeds and bind them in bone traps.",
                -20.14, -56.39, 1400, 2800),

            // ── Guarani Shadow Council (Paraguay) ── Jungle
            new LocalCultDef("encarnacion", "guarani_shadows", "Encarnación River-Mystics",
                "Riverside mystics who read the future in the Paraná's current and the birds that cross it.",
                -27.33, -55.87, 1700, 3400),
            new LocalCultDef("villarrica", "guarani_shadows", "Villarrica Forest-Speakers",
                "Forest-cultists who speak the first language of the trees and command the land itself.",
                -25.75, -56.47, 1800, 3600),
            new LocalCultDef("ciudad_del_este", "guarani_shadows", "Ciudad del Este Shadow-Traders",
                "Border-market occultists who trade in cursed artifacts and smuggled spirits.",
                -25.51, -54.61, 1900, 3800),

            // ── Kush Sorcerers (Sudan, Meroë) ── Savanna
            new LocalCultDef("shendi", "kush_sorcerers", "Shendi River-Priests",
                "Nile-priests who perform rites older than the pyramids along the riverbank.",
                16.70, 33.72, 2400, 4800),
            new LocalCultDef("berber", "kush_sorcerers", "Berber Stone-Channelers",
                "Desert-channelers who draw power from the black stones scattered across the landscape.",
                17.99, 33.99, 2600, 5200),
            new LocalCultDef("atbara", "kush_sorcerers", "Atbara Sand-Mages",
                "Confluence-city mages who read the meeting of waters and the meeting of worlds.",
                17.70, 34.97, 2800, 5600),

            // ── Ifá Oracles (Nigeria, Ile-Ife) ── Savanna
            new LocalCultDef("ibadan", "ifa_oracles", "Ibadan Divination-Circle",
                "Urban diviners who cast palm nuts in the market square and read destiny's pattern.",
                7.38, 3.91, 3500, 7000),
            new LocalCultDef("oshogbo", "ifa_oracles", "Oshogbo River-Priestesses",
                "Sacred-grove priestesses who tend the Osun river shrine and its ancient powers.",
                7.54, 4.57, 3800, 7600),
            new LocalCultDef("ife_odai", "ifa_oracles", "Modakeke Ancestor-Speakers",
                "Ancestral speakers who commune with the first kings of Ife through sacred groves.",
                7.40, 4.52, 4000, 8000),

            // ── Dogon Star Priests (Mali, Bandiagara) ── Savanna
            new LocalCultDef("bandiagara", "dogon_star_priests", "Bandiagara Cliff-Walkers",
                "Cliff-dwelling priests who navigate the vertical world between earth and sky.",
                14.35, -3.62, 5500, 11000),
            new LocalCultDef("sangha", "dogon_star_priests", "Sangha Mask-Spirits",
                "Mask-cultists who become the spirits they represent during the dama ceremonies.",
                14.45, -3.30, 6000, 12000),
            new LocalCultDef("dourou", "dogon_star_priests", "Dourou Star-Readers",
                "Remote-village astronomers who track Sirius B with naked-eye precision.",
                14.70, -3.90, 6500, 13000),

            // ── Zulu Sangoma (South Africa) ── Savanna
            new LocalCultDef("durban", "zulu_sangoma", "Durban Urban-Sangoma",
                "City-sangoma who bridges traditional healing and modern urban spiritual needs.",
                -29.86, 31.03, 8000, 16000),
            new LocalCultDef("pietermaritzburg", "zulu_sangoma", "Pietermaritzburg Bone-Casters",
                "Highland bone-casters who read the ancestors' will in the pattern of thrown bones.",
                -29.62, 30.40, 8500, 17000),
            new LocalCultDef("richards_bay", "zulu_sangoma", "Richards Bay Sea-Sangoma",
                "Coastal sangoma who commands the Indian Ocean's spirits and the creatures within.",
                -28.80, 32.10, 9000, 18000),

            // ── Axum Guardians (Ethiopia) ── Savanna
            new LocalCultDef("adwa", "axum_guardians", "Adwa Mountain-Priests",
                "Mountain-priests who guard the battlefield where Ethiopian forces repelled invaders.",
                14.17, 38.90, 12000, 24000),
            new LocalCultDef("mekelle", "axum_guardians", "Mekelle Stone-Keepers",
                "Highland keepers who maintain the ancient stone libraries of the north.",
                13.49, 39.47, 13000, 26000),
            new LocalCultDef("gondar", "axum_guardians", "Gondar Castle-Spirits",
                "Castle-city spirits who guard the old imperial capital's occult secrets.",
                12.60, 37.47, 14000, 28000),

            // ── Babylon Mages (Iraq) ── Desert
            new LocalCultDef("hillah", "babylon_mages", "Hillah Ruin-Readers",
                "Town-diviners who read the future in the cracked bricks of Babylon's ruins.",
                32.48, 44.43, 18000, 36000),
            new LocalCultDef("karbala", "babylon_mages", "Karbala Star-Mages",
                "Pilgrimage-city mages who read the heavens during the sacred observances.",
                32.61, 44.02, 19000, 38000),
            new LocalCultDef("najaf", "babylon_mages", "Najaf Shadow-Scholars",
                "Cemetery-city scholars who study the boundary between death and knowledge.",
                31.99, 44.33, 20000, 40000),

            // ── Djinn Binders (Saudi Arabia) ── Desert
            new LocalCultDef("najran", "djinn_binders", "Najran Brass-Smiths",
                "Desert-smiths who forge brass vessels specifically designed to trap djinn.",
                17.49, 44.13, 26000, 52000),
            new LocalCultDef("abha", "djinn_binders", "Abha Mountain-Binders",
                "Highland binders who command the mountain djinn that guard the passes.",
                18.22, 42.51, 28000, 56000),
            new LocalCultDef("jazan", "djinn_binders", "Jazan Sea-Djinn",
                "Coastal sorcerers who bargain with the djinn that dwell in the Red Sea's depths.",
                16.89, 42.55, 30000, 60000),

            // ── Hashashin Shadow (Iran) ── Desert
            new LocalCultDef("qazvin", "hashashin_shadow", "Qazvin Shadow-Walkers",
                "City-walkers who blend into crowds and strike from the anonymity of the market.",
                36.27, 50.00, 40000, 80000),
            new LocalCultDef("rasht", "hashashin_shadow", "Rasht Forest-Assassins",
                "Caspian-forest assassins who use the dense canopy as cover for their dark work.",
                37.28, 49.58, 42000, 84000),
            new LocalCultDef("tehran_old", "hashashin_shadow", "Old Tehran Hidden-Blade",
                "Urban-cult descendants who maintain the old ways in the capital's ancient quarters.",
                35.70, 51.42, 44000, 88000),

            // ── Sumerian Deep Ones (Iraq, Ur) ── Desert
            new LocalCultDef("nasiriyah", "sumerian_deep", "Nasiriyah Marsh-Spirits",
                "Marsh-dwellers who speak with the ancient things that still lurk in the reed beds.",
                31.05, 46.27, 60000, 120000),
            new LocalCultDef("basra", "sumerian_deep", "Basra Deep-Channelers",
                "Port-city channelers who open doors to the deep places beneath the Tigris-Euphrates delta.",
                30.51, 47.83, 65000, 130000),
            new LocalCultDef("ubaid", "sumerian_deep", "Ubaid Mound-Priests",
                "Mound-priests who guard the oldest settlement in Mesopotamia and what sleeps beneath it.",
                30.96, 46.10, 70000, 140000),

            // ── Qabbalah Masters (Israel, Safed) ── Desert
            new LocalCultDef("tiberias", "qabbalah_masters", "Tiberias Lake-Mystics",
                "Galilee-shore mystics who read the Torah's hidden codes in the lake's reflections.",
                32.79, 35.53, 90000, 180000),
            new LocalCultDef("meron", "qabbalah_masters", "Meron Sacred-Geometers",
                "Mountain-geometers who map the divine structure in the arrangement of ancient stones.",
                33.01, 35.50, 95000, 190000),
            new LocalCultDef("zefat_old", "qabbalah_masters", "Old Safed Letter-Weavers",
                "Alley-way weavers who reshape reality through the permutation of sacred letters.",
                32.96, 35.50, 100000, 200000),

            // ── Iga Shinobi (Japan) ── Ninja/Samurai
            new LocalCultDef("nabari", "iga_shinobi", "Nabari Valley-Shadows",
                "Valley-shinobi who use the mountain mist as cover for their silent operations.",
                34.77, 136.10, 130000, 260000),
            new LocalCultDef("igaueno", "iga_shinobi", "Iga Ueno Castle-Shadows",
                "Castle-town shinobi who hide in plain sight among the tourists and merchants.",
                34.77, 136.19, 140000, 280000),
            new LocalCultDef("yamato", "iga_shinobi", "Yamato Plain-Walkers",
                "Plain-walkers who cross open land without being seen, a feat they attribute to shadow-jutsu.",
                34.65, 135.77, 150000, 300000),

            // ── Koga Nightblades (Japan) ── Ninja/Samurai
            new LocalCultDef("otsu", "koga_nightblades", "Ōtsu Lake-Blades",
                "Lake-side assassins who strike from the water and vanish into the mist.",
                35.00, 135.87, 200000, 400000),
            new LocalCultDef("hikone", "koga_nightblades", "Hikone Castle-Poisoners",
                "Castle-town poisoners who maintain a garden of deadly plants behind the moat.",
                35.28, 136.25, 220000, 440000),
            new LocalCultDef("koka_old", "koga_nightblades", "Old Kōka Illusionists",
                "Village illusionists who can make an entire building appear empty to the untrained eye.",
                34.97, 136.17, 240000, 480000),

            // ── Takeda Ghost Ronin (Japan) ── Ninja/Samurai
            new LocalCultDef("kofu", "takeda_ronin", "Kōfu Blade-Spirits",
                "Castle-town swordsmen who channel the spirits of the Takeda clan's fallen warriors.",
                35.66, 138.57, 320000, 640000),
            new LocalCultDef("enzan", "takeda_ronin", "Enzan Mountain-Ronin",
                "Mountain-ronin who wander the peaks, testing their blade-spirits against the cold.",
                35.70, 138.73, 340000, 680000),
            new LocalCultDef("hokuto", "takeda_ronin", "Hokuto Ghost-Duelists",
                "Northern-duelists who fight spirit-battles that leave no visible wounds but break the soul.",
                35.71, 138.59, 360000, 720000),

            // ── Wu Dang Immortals (China) ── Ninja/Samurai
            new LocalCultDef("shiyan", "wu_dang_immortals", "Shiyan River-Immortals",
                "River-city cultivators who extend their lives through chi absorption from the water.",
                32.63, 110.80, 480000, 960000),
            new LocalCultDef("danjiangkou", "wu_dang_immortals", "Danjiangkou Reservoir-Sages",
                "Reservoir-sages who meditate beneath the water and emerge only on the solstice.",
                32.54, 111.51, 520000, 1040000),
            new LocalCultDef("fangxian", "wu_dang_immortals", "Fangxian Mountain-Alchemists",
                "Mountain-alchemists who refine the elixir of immortality in caves above the clouds.",
                32.40, 111.00, 560000, 1120000),

            // ── Shadow Shogun (Japan, Edo) ── Ninja/Samurai
            new LocalCultDef("yokohama", "shadow_shogun", "Yokohama Port-Shadows",
                "Port-city shadow-agents who control the flow of information and goods through the harbor.",
                35.44, 139.64, 700000, 1400000),
            new LocalCultDef("kawasaki", "shadow_shogun", "Kawasaki Industrial-Cult",
                "Industrial-cultists who weave dark magic into the machinery of the factories.",
                35.52, 139.73, 750000, 1500000),
            new LocalCultDef("chiba", "shadow_shogun", "Chiba Shadow-Guard",
                "Eastern-guard shinobi who protect the approaches to the Shogun's seat of power.",
                35.61, 140.12, 800000, 1600000),

            // ── Maori Tohunga (New Zealand) ── Ocean
            new LocalCultDef("tauranga", "maori_tohunga", "Tauranga Coastal-Spirits",
                "Coastal-spirit speakers who command the tides and the creatures of the shallows.",
                -37.69, 176.17, 1000000, 2000000),
            new LocalCultDef("whakatane", "maori_tohunga", "Whakātane River-Guardians",
                "River-guardians who protect the sacred waters and the ancient carvings along their banks.",
                -37.96, 177.00, 1100000, 2200000),
            new LocalCultDef("rotorua_old", "maori_tohunga", "Old Rotorua Geothermal-Cult",
                "Geothermal-cultists who read the future in the eruption of geysers and the color of boiling mud.",
                -38.14, 176.25, 1200000, 2400000),

            // ── Dreamtime Elders (Australia, Uluru) ── Ocean
            new LocalCultDef("alice_springs", "dreamtime_elders", "Alice Springs Desert-Walkers",
                "Desert-walkers who follow the songlines across the red center of the continent.",
                -23.70, 133.88, 1500000, 3000000),
            new LocalCultDef("kings_canyon", "dreamtime_elders", "Kings Canyon Stone-Singers",
                "Canyon-singers whose songs reshape the stone itself, opening and closing paths.",
                -24.25, 131.55, 1600000, 3200000),
            new LocalCultDef("mount_conner", "dreamtime_elders", "Mount Conner Dream-Guides",
                "Mesa-guides who lead initiates through the Dreamtime and bring them back changed.",
                -25.32, 132.10, 1700000, 3400000),

            // ── Polynesian Navigators (Tahiti) ── Ocean
            new LocalCultDef("moorea", "polynesian_navigators", "Moorea Lagoon-Readers",
                "Lagoon-readers who navigate by reading the spirit-currents beneath the surface.",
                -17.53, -149.83, 2200000, 4400000),
            new LocalCultDef("huahine", "polynesian_navigators", "Huahine Island-Spirits",
                "Island-spirits who guard the ancient marae and the power stored within their stones.",
                -16.74, -151.00, 2400000, 4800000),
            new LocalCultDef("raiatea", "polynesian_navigators", "Raiatea Star-Pathfinders",
                "Sacred-island pathfinders who maintain the ancient star-paths across the Pacific.",
                -16.83, -151.47, 2600000, 5200000),

            // ── Papuan Spirit Callers (Papua New Guinea) ── Ocean
            new LocalCultDef("mt_hagen", "papuan_spirits", "Mount Hagen Mask-Cult",
                "Highland mask-cultists who become the spirits their masks represent during ceremonies.",
                -5.86, 144.25, 3500000, 7000000),
            new LocalCultDef("goroka", "papuan_spirits", "Goroka Spirit-Dancers",
                "Valley-dancers who channel ancestor-spirits through elaborate body paint and movement.",
                -6.08, 145.40, 3800000, 7600000),
            new LocalCultDef("wabag", "papuan_spirits", "Wabag Highland-Channelers",
                "Remote-highland channelers who open doors to the spirit world through sacred caves.",
                -5.48, 143.70, 4000000, 8000000),

            // ── Pacific Abyss (Mariana Trench) ── Ocean
            new LocalCultDef("guam", "pacific_abyss", "Guam Trench-Watchers",
                "Island-watchers who monitor the deep and report what they sense stirring below.",
                13.44, 144.79, 5500000, 11000000),
            new LocalCultDef("saipan", "pacific_abyss", "Saipan Deep-Listeners",
                "Northern-listeners who hear the pressure-songs of things that dwell in the abyss.",
                15.18, 145.75, 6000000, 12000000),
            new LocalCultDef("palau", "pacific_abyss", "Palau Abyss-Speakers",
                "Island-speakers who maintain dialogue with the vast intelligence in the trench.",
                7.50, 134.62, 6500000, 13000000),
        };

    public static IReadOnlyList<LocalCultDef> ForCoven(string parentCovenId) =>
        All.Where(c => c.ParentCovenId == parentCovenId).ToList();

    public static LocalCultDef? Find(string cultId) =>
        All.FirstOrDefault(c => c.Id == cultId);
}
