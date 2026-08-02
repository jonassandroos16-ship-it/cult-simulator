using System.Collections.Immutable;

namespace CultSimulator.Game;

public static class GameData
{
    public static readonly ImmutableArray<BuildingDef> Buildings = ImmutableArray.Create(
        new BuildingDef(BuildingType.Shrine, "Shrine", "🕯️", 40, ResourceKind.Faith, 1.15, "+0.3 Faith/s"),
        new BuildingDef(BuildingType.Cathedral, "Cathedral", "⛪", 80, ResourceKind.Gold, 1.18, "+0.2 Gold/s"),
        new BuildingDef(BuildingType.Monolith, "Monolith", "🗿", 300, ResourceKind.Faith, 1.20, "+8% Faith generation"),
        new BuildingDef(BuildingType.Treasury, "Treasury", "💰", 500, ResourceKind.Gold, 1.20, "+8% Gold generation"),
        new BuildingDef(BuildingType.Bank, "Bank", "🏦", 500, ResourceKind.Gold, 1.55, "Increases idle income cap"),
        new BuildingDef(BuildingType.Observatory, "Observatory", "🔭", 800, ResourceKind.Gold, 1.22, "+6% Faith generation"),
        new BuildingDef(BuildingType.Reliquary, "Reliquary", "📿", 1500, ResourceKind.Gold, 1.24, "+6% Gold generation"),
        new BuildingDef(BuildingType.Undercroft, "Undercroft", "⚰️", 3000, ResourceKind.Gold, 1.28, "+2 Acolyte cap per level"),
        new BuildingDef(BuildingType.Barracks, "Barracks", "🏰", 600, ResourceKind.Gold, 1.20, "+5 Agent pool cap per level"),
        new BuildingDef(BuildingType.TrainingCamp, "Training Camp", "🗡️", 1200, ResourceKind.Gold, 1.22, "+50% Agent production speed per level"));

    public static readonly ImmutableArray<UpgradeDef> Upgrades = ImmutableArray.Create(
        new UpgradeDef(UpgradeId.Hymnal, "Sacred Hymnal", "📜", 120, 0, 0, "Preaching yields 2× Faith", 0),
        new UpgradeDef(UpgradeId.Relics, "Golden Relics", "🏺", 0, 250, 0, "Followers give 2× Gold", 15),
        new UpgradeDef(UpgradeId.Visions, "Prophetic Visions", "🔮", 600, 0, 0, "Followers give 2× Faith", 40),
        new UpgradeDef(UpgradeId.Ascendance, "Rite of Ascendance", "🌟", 1500, 1000, 0, "All production ×1.5", 120),
        new UpgradeDef(UpgradeId.BankVault, "Iron Vault", "🔒", 500, 2000, 5, "Bank idle cap ×1.5", 10),
        new UpgradeDef(UpgradeId.OffshoreAccounts, "Shadow Ledger", "📓", 2000, 6000, 15, "Bank idle cap ×1.5", 30),
        new UpgradeDef(UpgradeId.DarkLedger, "Blood Pact", "🩸", 5000, 20000, 40, "Bank idle cap ×2", 60),
        new UpgradeDef(UpgradeId.SoulEndowment, "Eternal Reserves", "💀", 15000, 60000, 100, "Bank idle cap ×2", 120));

    public static readonly ImmutableArray<UpgradeId> BankUpgrades = ImmutableArray.Create(
        UpgradeId.BankVault,
        UpgradeId.OffshoreAccounts,
        UpgradeId.DarkLedger,
        UpgradeId.SoulEndowment);

    public static readonly ImmutableArray<BuildingType> PreachBuildings = ImmutableArray.Create(
        BuildingType.Shrine,
        BuildingType.Cathedral,
        BuildingType.Monolith,
        BuildingType.Treasury,
        BuildingType.Observatory,
        BuildingType.Reliquary,
        BuildingType.Undercroft,
        BuildingType.Barracks,
        BuildingType.TrainingCamp);

public static readonly Dictionary<string, Dictionary<BuildingType, string>> CovenBuildingNames = new()
    {
        ["skanor"] = new()
        {
            { BuildingType.Shrine, "Seidr Altar" },
            { BuildingType.Cathedral, "Hof" },
            { BuildingType.Monolith, "Runestone" },
            { BuildingType.Treasury, "Silver Hoard" },
            { BuildingType.Observatory, "Star Watch" },
            { BuildingType.Reliquary, "Bone Reliquary" },
            { BuildingType.Undercroft, "Frost Crypt" },
            { BuildingType.Barracks, "Viking War Camp" },
            { BuildingType.TrainingCamp, "Viking Drill Ground" },
        },
        ["uppsala_gothi"] = new()
        {
            { BuildingType.Shrine, "Temple Hearth" },
            { BuildingType.Cathedral, "Great Hof" },
            { BuildingType.Monolith, "Mound Stone" },
            { BuildingType.Treasury, "Sacrificial Hoard" },
            { BuildingType.Observatory, "Sky Pillar" },
            { BuildingType.Reliquary, "King's Reliquary" },
            { BuildingType.Undercroft, "Burial Mound" },
            { BuildingType.Barracks, "Royal Guard Hall" },
            { BuildingType.TrainingCamp, "Gothi Training Field" },
        },
        ["hedeby_vikings"] = new()
        {
            { BuildingType.Shrine, "Raid Altar" },
            { BuildingType.Cathedral, "Longhouse" },
            { BuildingType.Monolith, "Boundary Stone" },
            { BuildingType.Treasury, "Plunder Chest" },
            { BuildingType.Observatory, "Mast Watch" },
            { BuildingType.Reliquary, "Amber Reliquary" },
            { BuildingType.Undercroft, "Keel Vault" },
            { BuildingType.Barracks, "Raiding Camp" },
            { BuildingType.TrainingCamp, "Raid Training Ground" },
        },
        ["trossky_berserkers"] = new()
        {
            { BuildingType.Shrine, "Bear Altar" },
            { BuildingType.Cathedral, "Berserker Hall" },
            { BuildingType.Monolith, "Rage Stone" },
            { BuildingType.Treasury, "War Hoard" },
            { BuildingType.Observatory, "Cliff Watch" },
            { BuildingType.Reliquary, "Pelt Reliquary" },
            { BuildingType.Undercroft, "Ice Cellar" },
            { BuildingType.Barracks, "Berserker Den" },
            { BuildingType.TrainingCamp, "Berserker Pit" },
        },
        ["jomsborg_elite"] = new()
        {
            { BuildingType.Shrine, "Oath Altar" },
            { BuildingType.Cathedral, "Jom Hall" },
            { BuildingType.Monolith, "Code Stone" },
            { BuildingType.Treasury, "Mercenary Coffer" },
            { BuildingType.Observatory, "Harbor Watch" },
            { BuildingType.Reliquary, "Oath Reliquary" },
            { BuildingType.Undercroft, "Sea Vault" },
            { BuildingType.Barracks, "Jomsviking Camp" },
            { BuildingType.TrainingCamp, "Jomsviking Drill Yard" },
        },
        ["salem_remnant"] = new()
        {
            { BuildingType.Shrine, "Witch Altar" },
            { BuildingType.Cathedral, "Tourist Temple" },
            { BuildingType.Monolith, "Gallows Stone" },
            { BuildingType.Treasury, "Gift Shop Vault" },
            { BuildingType.Observatory, "Paranormal Lab" },
            { BuildingType.Reliquary, "Trial Reliquary" },
            { BuildingType.Undercroft, "Colonial Cellar" },
            { BuildingType.Barracks, "Witch Hunter Cabin" },
            { BuildingType.TrainingCamp, "Witch Training Lodge" },
        },
        ["voodoo_quarter"] = new()
        {
            { BuildingType.Shrine, "Voodoo Altar" },
            { BuildingType.Cathedral, "Spirit House" },
            { BuildingType.Monolith, "Crossroads Stone" },
            { BuildingType.Treasury, "Graveyard Coffer" },
            { BuildingType.Observatory, "Bayou Watch" },
            { BuildingType.Reliquary, "Gris-Gris Reliquary" },
            { BuildingType.Undercroft, "Crypt Vault" },
            { BuildingType.Barracks, "Voodoo Lodge" },
            { BuildingType.TrainingCamp, "Spirit Training Ground" },
        },
        ["silicon_circle"] = new()
        {
            { BuildingType.Shrine, "Data Altar" },
            { BuildingType.Cathedral, "Server Temple" },
            { BuildingType.Monolith, "Quantum Stone" },
            { BuildingType.Treasury, "Crypto Vault" },
            { BuildingType.Observatory, "Neural Watch" },
            { BuildingType.Reliquary, "Code Reliquary" },
            { BuildingType.Undercroft, "Cooling Crypt" },
            { BuildingType.Barracks, "Private Security Hub" },
            { BuildingType.TrainingCamp, "Cyber Training Lab" },
        },
        ["hudson_witches"] = new()
        {
            { BuildingType.Shrine, "Estate Altar" },
            { BuildingType.Cathedral, "Manor Hall" },
            { BuildingType.Monolith, "Estate Stone" },
            { BuildingType.Treasury, "Old Money Vault" },
            { BuildingType.Observatory, "Rooftop Watch" },
            { BuildingType.Reliquary, "Colonial Reliquary" },
            { BuildingType.Undercroft, "Wine Cellar" },
            { BuildingType.Barracks, "Estate Guard House" },
            { BuildingType.TrainingCamp, "Estate Training Ground" },
        },
        ["montreal_night"] = new()
        {
            { BuildingType.Shrine, "Underground Altar" },
            { BuildingType.Cathedral, "Tunnel Temple" },
            { BuildingType.Monolith, "Frost Stone" },
            { BuildingType.Treasury, "Quebec Coffer" },
            { BuildingType.Observatory, "Sewer Watch" },
            { BuildingType.Reliquary, "Saint Reliquary" },
            { BuildingType.Undercroft, "Ice Vault" },
            { BuildingType.Barracks, "Underground Bunker" },
            { BuildingType.TrainingCamp, "Underground Fight Club" },
        },
        ["la_recta_provincia"] = new()
        {
            { BuildingType.Shrine, "Capilla" },
            { BuildingType.Cathedral, "Catedral" },
            { BuildingType.Monolith, "Piedra de Sacrificio" },
            { BuildingType.Treasury, "Cofre del Santo Oficio" },
            { BuildingType.Observatory, "Observatorio Austral" },
            { BuildingType.Reliquary, "Relicario Patagón" },
            { BuildingType.Undercroft, "Bóveda de Chiloé" },
            { BuildingType.Barracks, "Guarnición Secreta" },
            { BuildingType.TrainingCamp, "Campo de Entrenamiento" },
        },
        ["amazon_curanderos"] = new()
        {
            { BuildingType.Shrine, "Forest Altar" },
            { BuildingType.Cathedral, "Canopy Temple" },
            { BuildingType.Monolith, "Ayahuasca Stone" },
            { BuildingType.Treasury, "Jungle Coffer" },
            { BuildingType.Observatory, "Canopy Watch" },
            { BuildingType.Reliquary, "Plant Reliquary" },
            { BuildingType.Undercroft, "Root Vault" },
            { BuildingType.Barracks, "Jungle Outpost" },
            { BuildingType.TrainingCamp, "Jungle Training Camp" },
        },
        ["andean_pacha"] = new()
        {
            { BuildingType.Shrine, "Pacha Altar" },
            { BuildingType.Cathedral, "Sun Temple" },
            { BuildingType.Monolith, "Intihuatana" },
            { BuildingType.Treasury, "Inca Coffer" },
            { BuildingType.Observatory, "Andean Watch" },
            { BuildingType.Reliquary, "Mummy Reliquary" },
            { BuildingType.Undercroft, "Mountain Crypt" },
            { BuildingType.Barracks, "Warrior Barracks" },
            { BuildingType.TrainingCamp, "Warrior Training Ground" },
        },
        ["pantanal_feiticeira"] = new()
        {
            { BuildingType.Shrine, "Swamp Altar" },
            { BuildingType.Cathedral, "Waterside Temple" },
            { BuildingType.Monolith, "Flood Stone" },
            { BuildingType.Treasury, "River Coffer" },
            { BuildingType.Observatory, "Wetland Watch" },
            { BuildingType.Reliquary, "Caiman Reliquary" },
            { BuildingType.Undercroft, "Mud Vault" },
            { BuildingType.Barracks, "Swamp Hideout" },
            { BuildingType.TrainingCamp, "Swamp Training Camp" },
        },
        ["guarani_shadows"] = new()
        {
            { BuildingType.Shrine, "Shadow Altar" },
            { BuildingType.Cathedral, "Council House" },
            { BuildingType.Monolith, "Ancient Stone" },
            { BuildingType.Treasury, "Tribal Coffer" },
            { BuildingType.Observatory, "Sky Watch" },
            { BuildingType.Reliquary, "Ancestor Reliquary" },
            { BuildingType.Undercroft, "Earth Vault" },
            { BuildingType.Barracks, "Shadow Camp" },
            { BuildingType.TrainingCamp, "Shadow Training Ground" },
        },
        ["kush_sorcerers"] = new()
        {
            { BuildingType.Shrine, "Nubian Altar" },
            { BuildingType.Cathedral, "Pyramid Temple" },
            { BuildingType.Monolith, "Meroë Stele" },
            { BuildingType.Treasury, "Nubian Gold" },
            { BuildingType.Observatory, "Desert Watch" },
            { BuildingType.Reliquary, "Pharaoh Reliquary" },
            { BuildingType.Undercroft, "Tomb Vault" },
            { BuildingType.Barracks, "Nubian War Camp" },
            { BuildingType.TrainingCamp, "Nubian Training Camp" },
        },
        ["ifa_oracles"] = new()
        {
            { BuildingType.Shrine, "Ifá Altar" },
            { BuildingType.Cathedral, "Divination Temple" },
            { BuildingType.Monolith, "Palm Nut Stone" },
            { BuildingType.Treasury, "Oracle Coffer" },
            { BuildingType.Observatory, "Destiny Watch" },
            { BuildingType.Reliquary, "Orunmila Reliquary" },
            { BuildingType.Undercroft, "Ancestral Vault" },
            { BuildingType.Barracks, "Oracle Guard" },
            { BuildingType.TrainingCamp, "Oracle Training Lodge" },
        },
        ["dogon_star_priests"] = new()
        {
            { BuildingType.Shrine, "Star Altar" },
            { BuildingType.Cathedral, "Cliff Temple" },
            { BuildingType.Monolith, "Sirius Stone" },
            { BuildingType.Treasury, "Cosmic Coffer" },
            { BuildingType.Observatory, "Star Watch" },
            { BuildingType.Reliquary, "Nommo Reliquary" },
            { BuildingType.Undercroft, "Cliff Vault" },
            { BuildingType.Barracks, "Cliff Garrison" },
            { BuildingType.TrainingCamp, "Star Training Ground" },
        },
        ["zulu_sangoma"] = new()
        {
            { BuildingType.Shrine, "Bone Altar" },
            { BuildingType.Cathedral, "Ancestor Temple" },
            { BuildingType.Monolith, "Throwing Stone" },
            { BuildingType.Treasury, "Tribal Coffer" },
            { BuildingType.Observatory, "Spirit Watch" },
            { BuildingType.Reliquary, "Bone Reliquary" },
            { BuildingType.Undercroft, "Ancestor Vault" },
            { BuildingType.Barracks, "Warrior Kraal" },
            { BuildingType.TrainingCamp, "Warrior Training Kraal" },
        },
        ["axum_guardians"] = new()
        {
            { BuildingType.Shrine, "Obelisk Altar" },
            { BuildingType.Cathedral, "Stele Temple" },
            { BuildingType.Monolith, "Axum Stone" },
            { BuildingType.Treasury, "Ark Coffer" },
            { BuildingType.Observatory, "Highland Watch" },
            { BuildingType.Reliquary, "Covenant Reliquary" },
            { BuildingType.Undercroft, "Ancient Vault" },
            { BuildingType.Barracks, "Guardian Fortress" },
            { BuildingType.TrainingCamp, "Guardian Training Ground" },
        },
        ["babylon_mages"] = new()
        {
            { BuildingType.Shrine, "Ziggurat Altar" },
            { BuildingType.Cathedral, "Star Temple" },
            { BuildingType.Monolith, "Cuneiform Stone" },
            { BuildingType.Treasury, "Babylonian Gold" },
            { BuildingType.Observatory, "Astrology Watch" },
            { BuildingType.Reliquary, "Tablet Reliquary" },
            { BuildingType.Undercroft, "Clay Vault" },
            { BuildingType.Barracks, "Ziggurat Guard" },
            { BuildingType.TrainingCamp, "Mage Training Tower" },
        },
        ["djinn_binders"] = new()
        {
            { BuildingType.Shrine, "Brass Altar" },
            { BuildingType.Cathedral, "Tent Temple" },
            { BuildingType.Monolith, "Desert Stone" },
            { BuildingType.Treasury, "Djinn Vessel Vault" },
            { BuildingType.Observatory, "Dune Watch" },
            { BuildingType.Reliquary, "Brass Reliquary" },
            { BuildingType.Undercroft, "Sand Vault" },
            { BuildingType.Barracks, "Djinn Guard Camp" },
            { BuildingType.TrainingCamp, "Djinn Training Camp" },
        },
        ["hashashin_shadow"] = new()
        {
            { BuildingType.Shrine, "Shadow Altar" },
            { BuildingType.Cathedral, "Eagle Nest" },
            { BuildingType.Monolith, "Assassin Stone" },
            { BuildingType.Treasury, "Tribute Coffer" },
            { BuildingType.Observatory, "Mountain Watch" },
            { BuildingType.Reliquary, "Dagger Reliquary" },
            { BuildingType.Undercroft, "Alamut Vault" },
            { BuildingType.Barracks, "Assassin Stronghold" },
            { BuildingType.TrainingCamp, "Assassin Training Ground" },
        },
        ["sumerian_deep"] = new()
        {
            { BuildingType.Shrine, "Deep Altar" },
            { BuildingType.Cathedral, "Ziggurat Temple" },
            { BuildingType.Monolith, "Cuneiform Stele" },
            { BuildingType.Treasury, "Ancient Gold" },
            { BuildingType.Observatory, "Deep Watch" },
            { BuildingType.Reliquary, "Abyss Reliquary" },
            { BuildingType.Undercroft, "Subterranean Vault" },
            { BuildingType.Barracks, "Deep Garrison" },
            { BuildingType.TrainingCamp, "Deep Training Pit" },
        },
        ["qabbalah_masters"] = new()
        {
            { BuildingType.Shrine, "Sefirot Altar" },
            { BuildingType.Cathedral, "Meditation Temple" },
            { BuildingType.Monolith, "Sacred Geometry Stone" },
            { BuildingType.Treasury, "Mystic Coffer" },
            { BuildingType.Observatory, "Emanation Watch" },
            { BuildingType.Reliquary, "Letter Reliquary" },
            { BuildingType.Undercroft, "Safed Vault" },
            { BuildingType.Barracks, "Mystic Guard House" },
            { BuildingType.TrainingCamp, "Mystic Training Lodge" },
        },
        ["iga_shinobi"] = new()
        {
            { BuildingType.Shrine, "Shadow Altar" },
            { BuildingType.Cathedral, "Hidden Dojo" },
            { BuildingType.Monolith, "Shuriken Stone" },
            { BuildingType.Treasury, "Shinobi Coffer" },
            { BuildingType.Observatory, "Mist Watch" },
            { BuildingType.Reliquary, "Ninjutsu Reliquary" },
            { BuildingType.Undercroft, "Shadow Vault" },
            { BuildingType.Barracks, "Shinobi Dojo" },
            { BuildingType.TrainingCamp, "Shinobi Training Ground" },
        },
        ["koga_nightblades"] = new()
        {
            { BuildingType.Shrine, "Illusion Altar" },
            { BuildingType.Cathedral, "Poison Dojo" },
            { BuildingType.Monolith, "Genjutsu Stone" },
            { BuildingType.Treasury, "Night Coffer" },
            { BuildingType.Observatory, "Night Watch" },
            { BuildingType.Reliquary, "Venom Reliquary" },
            { BuildingType.Undercroft, "Poison Vault" },
            { BuildingType.Barracks, "Nightblade Camp" },
            { BuildingType.TrainingCamp, "Nightblade Training Pit" },
        },
        ["takeda_ronin"] = new()
        {
            { BuildingType.Shrine, "Blade Altar" },
            { BuildingType.Cathedral, "Ghost Dojo" },
            { BuildingType.Monolith, "Katana Stone" },
            { BuildingType.Treasury, "Ronin Coffer" },
            { BuildingType.Observatory, "Battlefield Watch" },
            { BuildingType.Reliquary, "Spirit Blade Reliquary" },
            { BuildingType.Undercroft, "Warrior Vault" },
            { BuildingType.Barracks, "Ronin Barracks" },
            { BuildingType.TrainingCamp, "Ronin Training Field" },
        },
        ["wu_dang_immortals"] = new()
        {
            { BuildingType.Shrine, "Chi Altar" },
            { BuildingType.Cathedral, "Mountain Temple" },
            { BuildingType.Monolith, "Immortal Stone" },
            { BuildingType.Treasury, "Elixir Coffer" },
            { BuildingType.Observatory, "Peak Watch" },
            { BuildingType.Reliquary, "Alchemy Reliquary" },
            { BuildingType.Undercroft, "Cave Vault" },
            { BuildingType.Barracks, "Immortal Training Hall" },
            { BuildingType.TrainingCamp, "Immortal Training Peak" },
        },
        ["shadow_shogun"] = new()
        {
            { BuildingType.Shrine, "Throne Altar" },
            { BuildingType.Cathedral, "Edo Castle" },
            { BuildingType.Monolith, "Shogun Stone" },
            { BuildingType.Treasury, "Imperial Coffer" },
            { BuildingType.Observatory, "Castle Watch" },
            { BuildingType.Reliquary, "Shogun Reliquary" },
            { BuildingType.Undercroft, "Castle Vault" },
            { BuildingType.Barracks, "Shogun Barracks" },
            { BuildingType.TrainingCamp, "Shogun Training Dojo" },
        },
        ["maori_tohunga"] = new()
        {
            { BuildingType.Shrine, "Moko Altar" },
            { BuildingType.Cathedral, "Marae Temple" },
            { BuildingType.Monolith, "Sacred Stone" },
            { BuildingType.Treasury, "Tribal Coffer" },
            { BuildingType.Observatory, "Coastal Watch" },
            { BuildingType.Reliquary, "Tiki Reliquary" },
            { BuildingType.Undercroft, "Burial Cave" },
            { BuildingType.Barracks, "War Canoe Lodge" },
            { BuildingType.TrainingCamp, "War Canoe Training Ground" },
        },
        ["dreamtime_elders"] = new()
        {
            { BuildingType.Shrine, "Dreamtime Altar" },
            { BuildingType.Cathedral, "Songline Temple" },
            { BuildingType.Monolith, "Uluru Stone" },
            { BuildingType.Treasury, "Songline Coffer" },
            { BuildingType.Observatory, "Desert Watch" },
            { BuildingType.Reliquary, "Ancestor Reliquary" },
            { BuildingType.Undercroft, "Dreamtime Vault" },
            { BuildingType.Barracks, "Outback Camp" },
            { BuildingType.TrainingCamp, "Outback Training Camp" },
        },
        ["polynesian_navigators"] = new()
        {
            { BuildingType.Shrine, "Wayfinder Altar" },
            { BuildingType.Cathedral, "Ocean Temple" },
            { BuildingType.Monolith, "Star Path Stone" },
            { BuildingType.Treasury, "Voyage Coffer" },
            { BuildingType.Observatory, "Star Path Watch" },
            { BuildingType.Reliquary, "Navigation Reliquary" },
            { BuildingType.Undercroft, "Deep Vault" },
            { BuildingType.Barracks, "Voyager Camp" },
            { BuildingType.TrainingCamp, "Voyager Training Camp" },
        },
        ["papuan_spirits"] = new()
        {
            { BuildingType.Shrine, "Mask Altar" },
            { BuildingType.Cathedral, "Spirit House" },
            { BuildingType.Monolith, "Highland Stone" },
            { BuildingType.Treasury, "Jungle Coffer" },
            { BuildingType.Observatory, "Ridge Watch" },
            { BuildingType.Reliquary, "Mask Reliquary" },
            { BuildingType.Undercroft, "Highland Vault" },
            { BuildingType.Barracks, "Highland Garrison" },
            { BuildingType.TrainingCamp, "Highland Training Ground" },
        },
        ["pacific_abyss"] = new()
        {
            { BuildingType.Shrine, "Abyss Altar" },
            { BuildingType.Cathedral, "Trench Temple" },
            { BuildingType.Monolith, "Pressure Stone" },
            { BuildingType.Treasury, "Deep Coffer" },
            { BuildingType.Observatory, "Abyssal Watch" },
            { BuildingType.Reliquary, "Deep One Reliquary" },
            { BuildingType.Undercroft, "Mariana Vault" },
            { BuildingType.Barracks, "Abyssal Garrison" },
            { BuildingType.TrainingCamp, "Abyssal Training Pit" },
        },
    };


    public static string BuildingNameFor(string covenId, BuildingType type)
    {
        if (CovenBuildingNames.TryGetValue(covenId, out var names) && names.TryGetValue(type, out var name))
            return name;
        return GameData.Buildings.First(b => b.Type == type).Name;
    }

    public static readonly ImmutableArray<RankDef> Ranks = ImmutableArray.Create(
        new RankDef("Novice", 0, "#94a3b8", "You light your first candle in the dark. The shadows listen, curious and patient."),
        new RankDef("Adept", 25, "#7dd3fc", "Whispers of your name travel on the wind. The faithful gather, hungry for truth."),
        new RankDef("Mystic", 100, "#c4b5fd", "Your voice bends the veil between worlds. What was hidden now answers your call."),
        new RankDef("Prophet", 250, "#fbbf24", "Nations tremble at your proclamations. You no longer speak for the faithful — the faithful speak for you."),
        new RankDef("Demigod", 600, "#fb7185", "Mortality frays at the edges of your being. The line between worship and reality blurs in your presence."),
        new RankDef("Ascended", 1500, "#f472b6", "You have crossed the threshold. Flesh, doubt, and death are memories. The world reshapes itself around your will."));

    public static readonly ImmutableArray<EventDef> Events = ImmutableArray.Create(
        new EventDef("lost_wanderer", "A Lost Wanderer",
            "A gaunt figure stumbles into your circle, eyes wide with desperation. \"I have walked for forty days. I seek only purpose.\"",
            new EventChoice("Welcome them into the fold", "+3 Followers, −20 Faith", s => { s.Followers += 3; s.Faith -= 20; return null; }),
            new EventChoice("Take their meager coin", "+50 Gold", s => { s.Gold += 50; return null; })),
        new EventDef("wealthy_patron", "A Wealthy Patron",
            "A noble in silk robes arrives with a heavy chest. \"I am drawn to your teachings. Perhaps we can... help each other.\"",
            new EventChoice("Accept their donation", "+120 Gold", s => { s.Gold += 120; return null; }),
            new EventChoice("Convert them to the faith", "+5 Followers, −40 Gold", s => { s.Followers += 5; s.Gold -= 40; return null; })),
        new EventDef("voice_of_doubt", "A Voice of Doubt",
            "A former priest stands at the edge of your gathering, voice trembling with conviction. \"Your doctrine is hollow. I challenge you to debate.\"",
            new EventChoice("Debate publicly", "+100 Faith if you win, −5 Followers if you lose", s =>
            {
                bool won = Random.Shared.NextDouble() < 0.5;
                if (won)
                {
                    s.Faith += 100;
                    return "You dismantled his arguments before the crowd. The faithful cheer, and the priest slinks away in silence. +100 Faith.";
                }
                s.Followers -= 5;
                return "His words cut deep. Doubt spreads through the crowd as five followers drift away into the night. −5 Followers.";
            }),
            new EventChoice("Perform a miracle", "+60 Faith, −30 Gold", s => { s.Faith += 60; s.Gold -= 30; return null; })),
        new EventDef("rival_cult", "A Rival Cult",
            "Word reaches you of a competing order gaining followers nearby. Their leader mocks your teachings from the market square.",
            new EventChoice("Outshine their ritual", "+150 Faith, −50 Gold", s => { s.Faith += 150; s.Gold -= 50; return null; }),
            new EventChoice("Ignore the distraction", "+4 Followers (quiet growth)", s => { s.Followers += 4; return null; })),
        new EventDef("blood_moon", "A Blood Moon Rises",
            "The sky bleeds crimson. Your followers whisper that a great ritual is possible beneath the cursed moon.",
            new EventChoice("Perform the blood ritual", "+8 Followers, −100 Faith", s => { s.Followers += 8; s.Faith -= 100; return null; }),
            new EventChoice("Prophesy the omen", "+200 Faith", s => { s.Faith += 200; return null; })));
}
