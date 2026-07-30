namespace CultSimulator.Game;

/// <summary>
/// Data-driven conversion sequences — one per rival coven, each themed to
/// the coven's country, era, and historical lore. Every step is a narrative
/// dilemma with two choices that advance siege progress and modify resources.
/// New covens can be added here without touching any engine or UI code.
/// </summary>
public static class ConversionData
{
    public static IReadOnlyList<ConversionDef> All =>
        new[]
        {
            HedebySiegeOfDarkness,
            LaRectaProvincia,
            Benandanti,
            MalkinTower,
            NorthBerwick,
            LaCabotina,
            IxchelPriestesses,
            NewForest,
            UppsalaGothi,
        };

    public static ConversionDef? Find(string covenId) =>
        All.FirstOrDefault(c => c.CovenId == covenId);

    // ─────────────────────────────────────────────────────────────────────────
    // ─────────────────────────────────────────────────────────────────────────
    // Hedeby Vikings — Denmark, Viking Age
    // Theme: Norse raiders, dark bargains, seiðr magic, the Öresund strait.
    // This is the FIRST conversion — intentionally straightforward to ease
    // players into the mechanic.
    // ─────────────────────────────────────────────────────────────────────────
    private static ConversionDef HedebySiegeOfDarkness => new(
        "hedeby_vikings",
        "Blood of the Öresund",
        new[]
        {
            new ConversionStep(
                "hed_1",
                "The Longship Parley",
                "A dragon-prowed longship beaches on the shore near Skanör. Its crew — scarred veterans of Hedeby's dark trade — have heard of your growing power and come to test you. Their jarl, Egil One-Eye, stands on the gunwale. 'We heard a witch-cult grows in this marsh. We have our own gods and our own magic. Why should the Hedeby Raiders bow to anyone?' He spits into the cold water.",
                new ConversionChoice(
                    "Demonstrate your power openly",
                    "+progress — they respect strength",
                    s => { s.Faith += 50; return "You call the fog in from the sea in an instant, shrouding their longship in grey silence. Egil watches, then laughs — a deep, approving sound. 'Not just words, then.' +50 Faith."; }),
                new ConversionChoice(
                    "Offer them silver and a bargain",
                    "−40 Gold, they listen more readily",
                    s => { if (s.Gold < 40) { s.Gold = 0; return "Your coffers are nearly empty, but the silver you press into Egil's hand buys you a hearing. He weighs it, then nods. 'Speak, then.'"; } s.Gold -= 40; return "The silver lands heavy in Egil's palm. He tosses it to his crew and turns to you with genuine interest. 'A cult that pays in real coin. Speak.'"; }),
                0.35, 0.5),

            new ConversionStep(
                "hed_2",
                "The Seiðr Test",
                "Egil brings his völva — a seiðr witch named Ragnhild — to judge your worthiness. She sits on a high-seat strung with cat-gut and sings a long-spell, trying to read your true nature. 'The spirits say you carry old hunger,' she announces. 'They do not say it is evil. But it frightens them.' She offers you a carved rune: accept the reading, or challenge it.",
                new ConversionChoice(
                    "Accept the reading and speak truth",
                    "+followers — honesty wins their trust",
                    s => { s.Followers += 6; return "You meet Ragnhild's gaze and confirm it: yes, old hunger. You want what the world has hoarded away. She smiles for the first time. 'Then we are kin.' +6 Followers join from Hedeby's ranks."; }),
                new ConversionChoice(
                    "Challenge the reading with counter-magic",
                    "Risky — impress or offend her",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.55)
                        {
                            s.Faith += 80;
                            return "You weave a counter-charm that silences the cat-gut strings mid-song. Ragnhild's eyes go wide — then fill with respect. 'The spirits did not tell me everything.' +80 Faith.";
                        }
                        s.Followers -= 3;
                        return "Ragnhild's magic is older than yours. She unravels your counter-charm effortlessly and three of your own followers, unsettled, edge away. −3 Followers.";
                    }),
                0.35, 0.55),

            new ConversionStep(
                "hed_3",
                "The Blót Bargain",
                "Egil organises a blót — a sacrifice feast by the longship — to seal any alliance. A horse is led out. 'The gods need blood before we give our word,' he says simply. 'Your gods, our gods — all gods drink.' He hands you the ceremonial knife. The Hedeby warriors watch in firelit silence.",
                new ConversionChoice(
                    "Perform the blót alongside them",
                    "−30 Faith, they embrace you as one of their own",
                    s => { s.Faith -= 30; if (s.Faith < 0) s.Faith = 0; return "You complete the rite without flinching. Blood steams on the cold ground. Egil clasps your forearm. 'Now you are oathed.' The raiders cheer into the dark sky."; }),
                new ConversionChoice(
                    "Redirect the sacrifice to your own ritual",
                    "Risky — your doctrine may not satisfy them",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.6)
                        {
                            s.Followers += 4;
                            return "You transform the blót into your own rite, weaving Norse words into your liturgy. Ragnhild nods approvingly — the gods got their blood either way. +4 Followers.";
                        }
                        s.Faith -= 50; if (s.Faith < 0) s.Faith = 0;
                        return "The warriors murmur that you dishonoured the gods with a foreign rite. The evening grows cold. −50 Faith.";
                    }),
                0.30, 0.6),
        });

    // La Recta Provincia — Chile, Chiloé Archipelago
    private static ConversionDef LaRectaProvincia => new(
        "la_recta_provincia",
        "The Shadow Tribunal of Chiloé",
        new[]
        {
            new ConversionStep(
                "lrp_1",
                "The Brujos' Toll",
                "Your emissaries reach the misty shores of Chiloé, where La Recta Provincia operates as a shadow government — collecting 'protection insurance' from every village. A brujo blockade stops your followers at the port. 'You don't belong to the Provincia,' their spokesman sneers, hand resting on a revisionario grimoire. 'Pay the toll, or the Imbunche will find you in the night.'",
                new ConversionChoice(
                    "Pay the toll in gold",
                    "−80 Gold, safe passage gained",
                    s => { if (s.Gold < 80) { s.Gold = 0; return "You emptied your coffers before the brujos. They let you through — barely — but the humiliation stings."; } s.Gold -= 80; return null; }),
                new ConversionChoice(
                    "Refuse and preach defiance",
                    "Risky: +progress if they listen, −followers if not",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.55)
                        {
                            s.Followers += 5;
                            return "Your words cut through the fog. Five islanders abandon the Provincia and join your cause, drawn by something truer than fear. +5 Followers.";
                        }
                        s.Followers -= 4;
                        return "The brujos laugh in your face. Four followers, terrified by tales of the Imbunche, slip away into the night. −4 Followers.";
                    }),
                0.25, 0.55),

            new ConversionStep(
                "lrp_2",
                "The Cave of Quicaví",
                "Guided by a defector, you descend into the Cueva de Quicaví — the underground chamber where the Provincia's leaders hold court. Glowing nocturnal signals pulse on the walls. At the center sits a twisted, deformed creature: the Imbunche, a stolen child warped by dark magic into a guardian beast. It blocks the path deeper inside.",
                new ConversionChoice(
                    "Offer a blessing to free the Imbunche's soul",
                    "−100 Faith, the beast is pacified",
                    s => { s.Faith -= 100; if (s.Faith < 0) s.Faith = 0; return "You chant over the tormented creature. For a moment its eyes clear — a child's eyes — and it shuffles aside, weeping. The path is open."; }),
                new ConversionChoice(
                    "Distract it with a ritual of noise",
                    "−50 Gold for materials, risky",
                    s =>
                    {
                        s.Gold -= 50; if (s.Gold < 0) s.Gold = 0;
                        if (Random.Shared.NextDouble() < 0.5)
                            return "Your followers bang drums and torches. The Imbunche, confused by the cacophony, retreats into a side tunnel. You slip past.";
                        s.Followers -= 3;
                        return "The noise enrages the beast. It lunges — three followers are dragged screaming into the dark before you retreat past it. −3 Followers.";
                    }),
                0.25, 0.5),

            new ConversionStep(
                "lrp_3",
                "The Revisión of Grimoires",
                "Deep in the cave, you find the Provincia's archive of revisorios — magical grimoires that hold their laws, their spells, their leverage over every village on the island. The chief brujo, an old Huilliche man with milk-white eyes, guards them. 'You wish to read? Then prove your doctrine is stronger than ours. A contest of revision.'",
                new ConversionChoice(
                    "Out-argue their doctrine",
                    "Risky: +progress or lose ground",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.6)
                        {
                            s.Faith += 120;
                            return "For three hours you dismantle their laws verse by verse. The old brujo bows his head. 'Your word is stronger.' +120 Faith.";
                        }
                        s.Faith -= 60; if (s.Faith < 0) s.Faith = 0;
                        return "Their grimoires hold arguments you cannot counter. You lose the contest and retreat, humbled. −60 Faith.";
                    }),
                new ConversionChoice(
                    "Burn the grimoires",
                    "−40 Faith, the Provincia loses its power base",
                    s => { s.Faith -= 40; if (s.Faith < 0) s.Faith = 0; return "You set the archive ablaze. The old brujo wails as generations of dark law turn to ash. Without their revisorios, the Provincia has nothing to enforce."; }),
                0.25, 0.6),

            new ConversionStep(
                "lrp_4",
                "The Final Reckoning at Quicaví",
                "The Provincia's surviving leaders kneel before you in the smoke-filled cave. The chief brujo speaks: 'We were a law when Chile had no law for us. Now our grimoires are ash. If your doctrine is the new law, then we will follow it — but you must swear to protect the island as we did.' The remnants of the shadow tribunal await your word.",
                new ConversionChoice(
                    "Swear the oath and absorb them",
                    "Convert the coven, −15% resources",
                    s => { return null; }),
                new ConversionChoice(
                    "Take their treasury and let them scatter",
                    "+150 Gold, but they resist — risky",
                    s =>
                    {
                        s.Gold += 150;
                        if (Random.Shared.NextDouble() < 0.4)
                        {
                            s.Followers -= 6;
                            return "You seize their coffers, but the brujos scatter into the forests vowing revenge. Six followers are lost to their reprisal curses. +150 Gold, −6 Followers.";
                        }
                        return "You take their gold and the remainder submit, too broken to fight. +150 Gold.";
                    }),
                0.25, 0.4),
        });

    // The Benandanti — Italy, Friuli
    private static ConversionDef Benandanti => new(
        "benandanti",
        "The Battle of the Ember Days",
        new[]
        {
            new ConversionStep(
                "ben_1",
                "The Caul-Bearers",
                "In the Friulian hills, you encounter the Benandanti — agrarian spirit-warriors born with a caul over their heads, a mark of innate power. They do not see themselves as witches but as protectors. Their elder, a weathered woman named Lucia, meets you at the edge of a fennel field. 'We fight the Malandanti in the spirit world four times a year. We have no quarrel with you — unless you are like them.'",
                new ConversionChoice(
                    "Offer an alliance against the Malandanti",
                    "Risky: +progress if they trust you",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.6)
                        {
                            s.Faith += 80;
                            return "Lucia studies your followers and nods slowly. 'If you will fight beside us on the Ember Days, then we are kin.' +80 Faith.";
                        }
                        s.Faith -= 40; if (s.Faith < 0) s.Faith = 0;
                        return "Lucia senses ambition, not kinship. 'You wear the caul of the Malandanti.' She turns away. −40 Faith.";
                    }),
                new ConversionChoice(
                    "Demonstrate your spirit-walking power",
                    "−120 Faith to project your power",
                    s => { s.Faith -= 120; if (s.Faith < 0) s.Faith = 0; return "You project your consciousness before them, walking the spirit field in full view. The Benandanti gasp — they have never seen one outside their order do this. Respect, tinged with fear, takes hold."; }),
                0.25, 0.6),

            new ConversionStep(
                "ben_2",
                "The Ember Night Journey",
                "On the first Ember Day, Lucia invites you to join the spirit battle. 'Lie in the field with fennel in your hands. Your soul will leave your body as a cat, a hare, or a butterfly. We fly to the battlefield together.' As your followers lie in the grass, the air shimmers. The astral plane unfolds — a vast spectral field where the Benandanti, in spirit-animal forms, clash with the dark shapes of the Malandanti.",
                new ConversionChoice(
                    "Join the battle with fennel bundles",
                    "Risky: +progress if victorious, −followers if lost",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.55)
                        {
                            s.Followers += 4;
                            return "Your spirit form — a great moth of golden light — turns the tide. The Malandanti scatter. The Benandanti cheer you as a brother. +4 Followers.";
                        }
                        s.Followers -= 5;
                        return "Your spirit is untrained for astral combat. The Malandanti overwhelm your projection and five followers wake feverish and broken. −5 Followers.";
                    }),
                new ConversionChoice(
                    "Guard the sleeping bodies instead",
                    "Safe: small progress, protect the vulnerable",
                    s => { s.Faith += 60; return "While the Benandanti fight on the astral plane, you guard their sleeping bodies. A group of Malandanti tries to attack the defenseless forms — you turn them away. Gratitude flows. +60 Faith."; }),
                0.25, 0.55),

            new ConversionStep(
                "ben_3",
                "The Fennel and the Sorghum",
                "Victorious on the Ember Day, the Benandanti hold a feast. Lucia places a bundle of fennel and a bundle of sorghum before you. 'The Malandanti used sorghum brooms to beat us. We used fennel. If you would lead us, you must choose which to carry into the next battle — and what it means.' The field falls silent.",
                new ConversionChoice(
                    "Take the fennel — fight as protectors",
                    "+progress, the Benandanti embrace you",
                    s => { s.Followers += 6; return "You raise the fennel bundle high. The Benandanti erupt in approval — you have chosen the path of the protector, not the destroyer. Lucia weeps with joy. +6 Followers."; }),
                new ConversionChoice(
                    "Take both — protection and power",
                    "Risky: some resist the blending of ways",
                    s =>
                    {
                        s.Faith += 100;
                        if (Random.Shared.NextDouble() < 0.4)
                        {
                            s.Followers -= 3;
                            return "You grasp both bundles. Some Benandanti protest — the sorghum is the tool of evil. Three purists leave. But most see wisdom in balance. +100 Faith, −3 Followers.";
                        }
                        return "You grasp both bundles. A hush falls — then slow, uncertain nods. Most accept the new doctrine. +100 Faith.";
                    }),
                0.25, 0.4),

            new ConversionStep(
                "ben_4",
                "The Last Ember",
                "The Benandanti gather for the final Ember Day of the year. Lucia, old and frail, hands you her fennel bundle. 'My spirit can no longer fly. The fields need a new protector. Lead us — and swear the harvest will never fail while you draw breath.' The spirit-warriors, in their animal forms, circle you expectantly.",
                new ConversionChoice(
                    "Accept the mantle of protector",
                    "Convert the coven, −15% resources",
                    s => { return null; }),
                new ConversionChoice(
                    "Demand they serve your doctrine instead",
                    "Risky: +progress if they submit, lose ground if not",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.5)
                        {
                            s.Faith += 200;
                            return "Your will is iron. The Benandanti bow — they will protect your harvests now, not just Friuli's. +200 Faith.";
                        }
                        s.Followers -= 8;
                        return "Lucia's eyes harden. 'A protector does not demand service.' She leads half the order away. −8 Followers.";
                    }),
                0.25, 0.5),
        });

    // The Malkin Tower Coven — England, Lancashire, 1612
    private static ConversionDef MalkinTower => new(
        "malkin_tower_coven",
        "The Pendle Power Struggle",
        new[]
        {
            new ConversionStep(
                "malk_1",
                "The Rival Families",
                "On the fog-swept slopes of Pendle Hill, you find the Malkin Tower Coven — but it is not unified. Two rival families of cunning folk, the Demdikes and the Chattox clan, eye each other with generations of hatred. Old Demdike, the matriarch, sits by the fire. 'You come at a ripe time. We're too busy killing each other to notice outsiders. Pick a side, or pick neither — but pick fast, before the magistrate does it for us.'",
                new ConversionChoice(
                    "Side with Old Demdike's family",
                    "Risky: +progress if they're stronger",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.55)
                        {
                            s.Followers += 5;
                            return "The Demdikes are the stronger clan. With your backing they crush the Chattox faction, and five of their number join you gratefully. +5 Followers.";
                        }
                        s.Followers -= 3;
                        return "The Chattox clan was underestimated. They strike back in the night — three of your followers are cursed. −3 Followers.";
                    }),
                new ConversionChoice(
                    "Broker peace between the families",
                    "−60 Faith for the mediation ritual",
                    s => { s.Faith -= 60; if (s.Faith < 0) s.Faith = 0; return "You mediate through the long night, invoking old bonds. Both families grudgingly lower their knives. A unified Malkin Tower is easier to convert than a warring one."; }),
                0.25, 0.55),

            new ConversionStep(
                "malk_2",
                "Good Friday at the Tower",
                "The reunited coven gathers at Malkin Tower for a Good Friday feast. But word reaches you that the local magistrate, Roger Nowell, has been tipped off. His men are riding from Lancaster. 'He'll hang us all,' Old Demdike croaks. 'He's done it before. We need to decide — do we flee, or do we make our stand here?'",
                new ConversionChoice(
                    "Help them flee into the moors",
                    "Safe: small progress, they remember your mercy",
                    s => { s.Followers += 3; return "You guide the coven into the misty Pendle moors, beyond Nowell's reach. They scatter and regroup at a secret camp, grateful for your leadership. +3 Followers."; }),
                new ConversionChoice(
                    "Make a stand and confront the law",
                    "Risky: +progress if bold, −followers if captured",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.5)
                        {
                            s.Faith += 150;
                            return "You stand before Nowell's men and weave a terror so deep they flee back to Lancaster. The coven looks at you with awe. +150 Faith.";
                        }
                        s.Followers -= 7;
                        return "Nowell's men are not so easily spooked. Seven followers are seized and dragged to Lancaster Castle. The rest scatter. −7 Followers.";
                    }),
                0.25, 0.5),

            new ConversionStep(
                "malk_3",
                "The Familiar's Pact",
                "Safe in the moors, Old Demdike reveals the source of the coven's power: each member has a familiar — a spirit animal bonded to them. 'If you would lead us, you must forge a pact with a familiar of your own. But the binding is not gentle. It takes a piece of your faith and gives a piece of its power.' A spectral cat, a hare, and a toad sit in a triangle, waiting.",
                new ConversionChoice(
                    "Bind the cat — cunning and stealth",
                    "−100 Faith, gain the cat's cunning",
                    s => { s.Faith -= 100; if (s.Faith < 0) s.Faith = 0; return "The spectral cat sinks into your chest. Your thoughts sharpen — you see angles and schemes you never noticed. The coven recognizes the bond."; }),
                new ConversionChoice(
                    "Bind the toad — patience and the earth",
                    "−100 Faith, gain the toad's resilience",
                    s => { s.Faith -= 100; if (s.Faith < 0) s.Faith = 0; return "The spectral toad settles into your heart. A deep calm fills you — the patience of stone and mud. The coven bows to the earth-bonded one."; }),
                0.25, 0.0),

            new ConversionStep(
                "malk_4",
                "The Passing of Demdike",
                "Old Demdike lies dying by the campfire, her familiar — a black dog — fading beside her. She grips your wrist with surprising strength. 'The families follow you now, not me. But Pendle is ours. Swear you'll keep the old ways alive on this hill, or I'll haunt you from the grave.' The whole coven watches in the firelight.",
                new ConversionChoice(
                    "Swear to keep the old ways on Pendle",
                    "Convert the coven, −15% resources",
                    s => { return null; }),
                new ConversionChoice(
                    "Promise power beyond the old ways",
                    "Risky: +progress if inspired, lose ground if offended",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.5)
                        {
                            s.Faith += 200;
                            return "Your vision of a greater future lights the firelight in their eyes. Demdike smiles with her last breath. 'Then make it so.' +200 Faith.";
                        }
                        s.Followers -= 5;
                        return "Demdike's eyes flash with fury. 'You swear on a dying woman's word and lie?' Five followers, loyal to the old matriarch, vanish into the moors. −5 Followers.";
                    }),
                0.25, 0.5),
        });

    // The North Berwick Coven — Scotland, 1590
    private static ConversionDef NorthBerwick => new(
        "north_berwick_coven",
        "The Storm Over the Forth",
        new[]
        {
            new ConversionStep(
                "nb_1",
                "The Drowned Cats",
                "On the cliffs of East Lothian, you find the North Berwick Coven at the ruined Auld Kirk. Agnes Sampson, the midwife-healer who leads them, is drowning cats in a tub of sea-water. 'We brew storms,' she says without looking up. 'The king's ship sails tonight. We aim to sink it — he's a tyrant who hunts our kind. Will you add your faith to the storm, or will you stand aside?'",
                new ConversionChoice(
                    "Add your faith to the storm ritual",
                    "Risky: +progress if the storm rises",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.6)
                        {
                            s.Faith += 100;
                            return "Your faith pours into the churning water. The sky over the Firth of Forth blackens. Agnes nods approvingly. +100 Faith.";
                        }
                        s.Faith -= 80; if (s.Faith < 0) s.Faith = 0;
                        return "The ritual backlashes. Salt water floods your senses and the storm dissipates. Agnes frowns. 'Your faith is not yet strong enough for the sea.' −80 Faith.";
                    }),
                new ConversionChoice(
                    "Warn that the king will retaliate harder",
                    "Safe: small progress, caution noted",
                    s => { s.Faith += 40; return "You counsel restraint — a failed storm will only enrage King James. Agnes considers, then releases the cats. 'Perhaps you're wiser than the sea. We'll prepare first.' +40 Faith."; }),
                0.25, 0.6),

            new ConversionStep(
                "nb_2",
                "The Auld Kirk Gathering",
                "Over seventy accused witches now gather in the ruins of the Auld Kirk, the largest coven in Scottish history. Agnes introduces you to John Fian, the schoolmaster who coordinates their rites. 'We need a leader who can hold this many,' Fian says bluntly. 'Agnes is midwife to the dying, not general to the living. Prove you can command — organize our next rite.'",
                new ConversionChoice(
                    "Command the full rite yourself",
                    "−120 Faith, +progress if successful",
                    s => { s.Faith -= 120; if (s.Faith < 0) s.Faith = 0; s.Followers += 4; return "You stand before seventy witches and lead the rite with a voice that fills the ruined kirk. They follow your cadence perfectly. Fian bows. +4 Followers."; }),
                new ConversionChoice(
                    "Share command with Agnes and Fian",
                    "Safe: steady progress, trust built",
                    s => { s.Faith += 50; return "You co-lead the rite, weaving your voice with Agnes's and Fian's. The coven sees collaboration, not ego. Trust deepens. +50 Faith."; }),
                0.25, 0.0),

            new ConversionStep(
                "nb_3",
                "The King's Interrogation",
                "Disaster — King James VI himself has arrived, interrogating suspected witches personally. He's seized three of your coven and is torturing confessions from them. Agnes is distraught. 'He's writing a book about us — Daemonologie, he calls it. He'll make us legend and execution both. We must act: rescue our people, or silence the witnesses.'",
                new ConversionChoice(
                    "Rescue the captured witches",
                    "Risky: +progress if successful, −followers if caught",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.5)
                        {
                            s.Followers += 6;
                            return "Under cover of a conjured fog, you slip into the holding cells and free all three. They join your cause, forever loyal. +6 Followers.";
                        }
                        s.Followers -= 8;
                        return "The guards were waiting. Eight followers are caught in the rescue attempt and hanged at dawn. Agnes weeps. −8 Followers.";
                    }),
                new ConversionChoice(
                    "Silence the witnesses with a binding oath",
                    "−90 Faith, they cannot confess",
                    s => { s.Faith -= 90; if (s.Faith < 0) s.Faith = 0; return "You weave a binding that seals their tongues. Under torture they confess nothing. James, frustrated, releases them for lack of evidence."; }),
                0.25, 0.5),

            new ConversionStep(
                "nb_4",
                "The Calm After the Storm",
                "With the king's investigation thwarted, the coven gathers on the clifftop at dusk. The sea is calm for the first time in months. Agnes and Fian stand before you. 'We were storm-witches fighting a king,' Agnes says. 'You've shown us a bigger fight. Lead us — but tell us: do we keep brewing storms, or do we seek a calmer power?'",
                new ConversionChoice(
                    "Lead them to a power beyond storms",
                    "Convert the coven, −15% resources",
                    s => { return null; }),
                new ConversionChoice(
                    "Keep the storm magic — escalate",
                    "Risky: +progress if emboldened, lose ground if reckless",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.45)
                        {
                            s.Faith += 250;
                            return "The coven roars with hunger for the storm. Under your command, their weather magic will be legendary. +250 Faith.";
                        }
                        s.Followers -= 6;
                        return "Recklessness invites ruin. James doubles his witch-hunt, and six followers are caught in the purge. −6 Followers.";
                    }),
                0.25, 0.45),
        });

    // The Circle of La Cabotina — Italy, Triora, Liguria, 1587
    private static ConversionDef LaCabotina => new(
        "la_cabotina",
        "The Famine of Triora",
        new[]
        {
            new ConversionStep(
                "cab_1",
                "The Starving Village",
                "High in the Ligurian Alps, the village of Triora is gripped by famine. The local authorities blame the women who gather at La Cabotina — a desolate cavernous rock formation outside the walls. You arrive to find the accused women huddled in the cave, half-starved themselves. Their leader, a gnarled woman named Bartolomea, spits: 'They call us witches because their crops failed. We didn't curse the harvest — we tried to save it. Will you help us feed these people, or will you join the accusers?'",
                new ConversionChoice(
                    "Help them feed the village",
                    "−100 Gold for grain, +progress if grateful",
                    s => { s.Gold -= 100; if (s.Gold < 0) s.Gold = 0; s.Followers += 5; return "You buy grain from the lowland markets and distribute it. The starving villagers turn on the authorities, not the witches. Bartolomea grips your hand. +5 Followers."; }),
                new ConversionChoice(
                    "Perform a ritual to restore the harvest",
                    "−150 Faith, risky: +progress if it works",
                    s =>
                    {
                        s.Faith -= 150; if (s.Faith < 0) s.Faith = 0;
                        if (Random.Shared.NextDouble() < 0.55)
                            return "You bless the blighted fields. Within a week, green shoots push through the cracked earth. The Cabotina women watch in awe.";
                        s.Followers -= 3;
                        return "The ritual fizzles in the stubborn Ligurian soil. Three of your followers, blamed for the failure, are chased from the village. −3 Followers.";
                    }),
                0.25, 0.55),

            new ConversionStep(
                "cab_2",
                "The Shape-Shifters' Confession",
                "With the famine easing, Bartolomea reveals the Cabotina's deepest secret: the women can shape-shift — into wolves, cats, and goats — to travel unseen through the mountains. 'The authorities would burn us for this alone. But if you would lead us, you must accept the beast within. Will you share our second skin, or will you forbid it?'",
                new ConversionChoice(
                    "Accept the shape-shifting gift",
                    "−100 Faith for the bonding, embrace their ways",
                    s => { s.Faith -= 100; if (s.Faith < 0) s.Faith = 0; return "The ritual is ancient and raw. Your bones reshape — fur, claw, fang — then snap back. You stagger up, changed. The women embrace you as one of their own."; }),
                new ConversionChoice(
                    "Forbid it as too dangerous",
                    "Risky: +progress if they respect your caution",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.5)
                        {
                            s.Faith += 80;
                            return "Your caution resonates with the younger women, who fear the authorities. Bartolomea reluctantly agrees to restrict the shifting. +80 Faith.";
                        }
                        s.Followers -= 4;
                        return "Bartolomea is outraged. 'You'd cage the beast that keeps us free?' Four shape-shifters vanish into the mountains. −4 Followers.";
                    }),
                0.25, 0.5),

            new ConversionStep(
                "cab_3",
                "The Inquisitor's Arrival",
                "A Church inquisitor arrives in Triora, dispatched from Rome. He's brought instruments of torture and a list of names. Bartolomea's face is grim. 'He'll squeeze confessions from the weakest of us. We can flee to the high peaks, curse him with the evil eye, or you can face him directly — your power against his office.'",
                new ConversionChoice(
                    "Face the inquisitor directly",
                    "Risky: +progress if you unnerve him, −followers if not",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.5)
                        {
                            s.Faith += 200;
                            return "You stand before the inquisitor and let your power radiate. He stumbles, drops his instruments, and flees Triora that night. The coven is awed. +200 Faith.";
                        }
                        s.Followers -= 6;
                        return "The inquisitor is made of sterner faith. He denounces you publicly and six followers are seized for questioning. −6 Followers.";
                    }),
                new ConversionChoice(
                    "Lead the coven to the high peaks",
                    "Safe: small progress, the coven survives",
                    s => { s.Followers += 2; return "You guide the women up into the Ligurian peaks, beyond the inquisitor's reach. They scatter among the goat-herders and survive, grateful. +2 Followers."; }),
                0.25, 0.5),

            new ConversionStep(
                "cab_4",
                "The Cavern Reclaimed",
                "With the inquisitor gone, the Cabotina women return to their cavernous rock formation. Bartolomea, old and tired, sits on the stone where she first hid as a girl. 'We were never witches — we were women with knowledge, and that was enough to damn us. If you lead us now, will you protect that knowledge, or will you bury it?'",
                new ConversionChoice(
                    "Protect and spread their knowledge",
                    "Convert the coven, −15% resources",
                    s => { return null; }),
                new ConversionChoice(
                    "Bury it for safety — power in secrecy",
                    "Risky: +progress if they accept discipline",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.5)
                        {
                            s.Faith += 180;
                            return "Some women nod — secrecy has kept them alive for centuries. They share their hidden knowledge with you alone. +180 Faith.";
                        }
                        s.Followers -= 5;
                        return "Bartolomea shakes her head. 'We hid long enough.' Five women, defiant, take their knowledge elsewhere. −5 Followers.";
                    }),
                0.25, 0.5),
        });

    // The Ixchel Priestesses — Mexico, Cozumel, Maya
    private static ConversionDef IxchelPriestesses => new(
        "ixchel_priestesses",
        "The Moon over Cozumel",
        new[]
        {
            new ConversionStep(
                "ixc_1",
                "The Pilgrimage to Isla Mujeres",
                "You sail to the island of Cozumel — Isla Mujeres, the Island of Women — where for centuries women from across Mesoamerica have come to worship Ixchel, the Maya goddess of medicine, midwifery, weaving, and the moon. The priestesses meet you on the white shore. Their high priestess, a silver-haired Maya woman named Citlali, says: 'Men do not usually come to Ixchel's island. Why do you seek the goddess of the moon?'",
                new ConversionChoice(
                    "Seek her knowledge of healing",
                    "−100 Faith as offering, +progress if accepted",
                    s => { s.Faith -= 100; if (s.Faith < 0) s.Faith = 0; return "You lay your faith at the goddess's altar. Citlali nods. 'A seeker of knowledge is welcome. Come — we will teach you what the moon knows of the body.'"; }),
                new ConversionChoice(
                    "Proclaim your doctrine surpasses hers",
                    "Risky: +progress if impressed, lose ground if offended",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.4)
                        {
                            s.Faith += 120;
                            return "Citlali's eyes narrow, then soften. 'Bold. Ixchel respects boldness. But your doctrine must prove itself under the moon.' +120 Faith.";
                        }
                        s.Followers -= 4;
                        return "The priestesses turn their backs. 'Arrogance has no place on this island.' Four followers, shamed, leave your entourage. −4 Followers.";
                    }),
                0.25, 0.4),

            new ConversionStep(
                "ixc_2",
                "The Lunar Rite",
                "On the night of the full moon, Citlali leads you to a cenote — a sacred sinkhole filled with moonlit water. 'Ixchel speaks through water and moonlight. To join us, you must enter the cenote and receive her vision. But the water tests the soul — some who enter do not return the same.' The moon hangs full and low over the limestone rim.",
                new ConversionChoice(
                    "Enter the cenote and receive the vision",
                    "Risky: +progress if blessed, −faith if tested",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.6)
                        {
                            s.Faith += 200;
                            return "You sink into the moonlit water. Ixchel's presence fills you — the weave of the world, the pulse of tides, the knowledge of healing herbs. You surface transformed. +200 Faith.";
                        }
                        s.Faith -= 120; if (s.Faith < 0) s.Faith = 0;
                        return "The water is colder than death. Something vast and ancient brushes your soul and finds it wanting. You surface gasping, diminished. −120 Faith.";
                    }),
                new ConversionChoice(
                    "Observe the rite from the rim",
                    "Safe: small progress, learn by watching",
                    s => { s.Faith += 60; return "You watch from the limestone edge as the priestesses commune with the moon. The patterns of their rite etch themselves into your mind. +60 Faith."; }),
                0.25, 0.6),

            new ConversionStep(
                "ixc_3",
                "The Venom and the Remedy",
                "Citlali reveals the priestesses' most guarded secret: knowledge of botanical medicine and venom-neutralizing remedies, passed down since before the Spanish conquest. 'We can heal any snakebite, cure any fever. But the Spanish would call it brujería and burn us. After the conquest, we went underground, blending our ways with theirs. Will you protect this knowledge openly, or keep it hidden?'",
                new ConversionChoice(
                    "Protect it openly — heal the world",
                    "Risky: +progress if bold, −followers if exposed",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.5)
                        {
                            s.Followers += 7;
                            return "You declare the healing knowledge sacred and open. Word spreads — the sick come from across the Yucatán. Seven new followers join to learn. +7 Followers.";
                        }
                        s.Followers -= 5;
                        return "The colonial authorities take note of the open practice. Five priestesses are seized before you can hide them. −5 Followers.";
                    }),
                new ConversionChoice(
                    "Keep it hidden — survival first",
                    "Safe: small progress, the knowledge endures",
                    s => { s.Faith += 70; return "You agree: the knowledge must survive, even if hidden. The priestesses teach you in secret, blending Maya healing with European folk practice. +70 Faith."; }),
                0.25, 0.5),

            new ConversionStep(
                "ixc_4",
                "The Last Moon of Citlali",
                "Citlali stands at the cenote's edge on the last night of her tenure. The moon is a sliver. 'I have carried Ixchel's knowledge for fifty years. My hands are too old to weave, my eyes too dim to read the moon. The priestesses need a new voice. Will you carry the goddess's knowledge forward — and swear to honor the women who kept it alive?'",
                new ConversionChoice(
                    "Swear to honor Ixchel and her priestesses",
                    "Convert the coven, −15% resources",
                    s => { return null; }),
                new ConversionChoice(
                    "Offer a different goddess — your own doctrine",
                    "Risky: +progress if they syncretize, lose ground if not",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.4)
                        {
                            s.Faith += 300;
                            return "The priestesses see in your doctrine a mirror of Ixchel — different names, same moon. They weave the two together in a new syncretic rite. +300 Faith.";
                        }
                        s.Followers -= 6;
                        return "Citlali's face closes. 'Ixchel is not replaced.' Six priestesses, loyal to the old goddess, sail away to the mainland. −6 Followers.";
                    }),
                0.25, 0.4),
        });

    // The New Forest Coven — England, Hampshire, 1930s–40s
    private static ConversionDef NewForest => new(
        "new_forest_coven",
        "The Cone of Power",
        new[]
        {
            new ConversionStep(
                "nf_1",
                "The Clearing in the Woods",
                "Deep in the New Forest, you find a hidden clearing where a coven gathers under the old oaks. They are not medieval peasants but modern people — a schoolteacher, a gardener, a retired colonel — practicing what they claim is an unbroken lineage of ancient Pagan religion. Their high priestess, known only as 'Old Dorothy,' greets you. 'Gerald told us about you. He says you have power. We don't recruit — but we don't turn away the genuine. Show us what you can do.'",
                new ConversionChoice(
                    "Demonstrate your power in the clearing",
                    "−150 Faith, +progress if impressive",
                    s => { s.Faith -= 150; if (s.Faith < 0) s.Faith = 0; s.Followers += 3; return "You raise energy in the clearing until the oaks themselves seem to lean in. The coven exchanges glances. Old Dorothy nods slowly. 'Gerald was right.' +3 Followers."; }),
                new ConversionChoice(
                    "Share your doctrine and debate theology",
                    "Risky: +progress if convincing",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.55)
                        {
                            s.Faith += 100;
                            return "The debate lasts until dawn. Your doctrine resonates with their Pagan roots. Several members nod thoughtfully. +100 Faith.";
                        }
                        s.Faith -= 50; if (s.Faith < 0) s.Faith = 0;
                        return "The retired colonel demolishes your theology with cold logic. The coven is unimpressed. −50 Faith.";
                    }),
                0.25, 0.55),

            new ConversionStep(
                "nf_2",
                "The Initiation",
                "Old Dorothy offers you initiation into the coven — a ritual of binding to the old ways. 'It's not a toy. You swear an oath of secrecy, you receive the lineage, and you belong to the circle. Once done, there's no leaving cleanly. Gerald went through it. So must you, if you would lead us.' The coven forms a circle in the moonlit clearing.",
                new ConversionChoice(
                    "Undergo the initiation rite",
                    "−120 Faith, bind yourself to the lineage",
                    s => { s.Faith -= 120; if (s.Faith < 0) s.Faith = 0; return "The initiation is ancient and intimate — oaths whispered, energy passed hand to hand. You emerge part of the lineage, bound to the New Forest and its circle."; }),
                new ConversionChoice(
                    "Ask to observe before committing",
                    "Risky: +progress if they respect caution",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.45)
                        {
                            s.Faith += 60;
                            return "Old Dorothy appreciates your thoughtfulness. 'A wise leader doesn't rush.' She lets you observe the rite. +60 Faith.";
                        }
                        s.Followers -= 3;
                        return "The coven is disappointed by your hesitation. 'Gerald didn't hold back.' Three members lose interest. −3 Followers.";
                    }),
                0.25, 0.45),

            new ConversionStep(
                "nf_3",
                "Operation Cone of Power",
                "The year is 1940. Hitler's forces threaten to invade Britain. Old Dorothy proposes a desperate ritual: the entire coven will raise a 'Cone of Power' — a massive collective psychic projection — to stop the invasion. 'We did it once before, they say. It nearly killed us. We need every ounce of faith. Will you contribute your full power to the cone, or hold back in reserve?'",
                new ConversionChoice(
                    "Contribute your full power to the cone",
                    "−200 Faith, risky: +progress if the cone holds",
                    s =>
                    {
                        s.Faith -= 200; if (s.Faith < 0) s.Faith = 0;
                        if (Random.Shared.NextDouble() < 0.6)
                        {
                            s.Followers += 8;
                            return "The cone of power rises like a pillar of light into the forest sky. You feel it reach across the channel. Whether it stops an invasion or not, the coven is galvanized. +8 Followers.";
                        }
                        s.Followers -= 4;
                        return "The cone is too much — the energy scatters and lashes back. Four followers collapse, their faith burned out. The ritual fails. −4 Followers.";
                    }),
                new ConversionChoice(
                    "Hold back and stabilize the circle",
                    "Safe: small progress, you're the anchor",
                    s => { s.Faith += 80; return "You act as the anchor, grounding the circle as the cone rises. The coven channels safely. Old Dorothy commends your discipline. +80 Faith."; }),
                0.25, 0.6),

            new ConversionStep(
                "nf_4",
                "The New Generation",
                "The war ends. The coven has survived — and grown. Gerald Gardner has begun writing about Wicca, bringing the old ways into the open. Old Dorothy, now very old, sits in the clearing one last time. 'Gerald wants to take this public. I'm not sure. But I'm too old to decide. You've proven your power and your patience. Lead the coven — and decide: do we stay hidden, or do we change the world?'",
                new ConversionChoice(
                    "Lead them into the open — change the world",
                    "Convert the coven, −15% resources",
                    s => { return null; }),
                new ConversionChoice(
                    "Keep them hidden — the old way",
                    "Risky: +progress if they respect tradition",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.5)
                        {
                            s.Faith += 250;
                            return "The coven agrees — secrecy has preserved them for centuries. Under your leadership they remain a hidden power, deeper than the world knows. +250 Faith.";
                        }
                        s.Followers -= 5;
                        return "The younger members rebel. 'The world needs us.' Five split off to follow Gardner into the public eye. −5 Followers.";
                    }),
                0.25, 0.5),
        });

    private static ConversionDef UppsalaGothi => new(
        "uppsala_gothi",
        "The Rusting of the Old Temple",
        new[]
        {
            new ConversionStep(
                "ug_1",
                "The Pilgrim Roads",
                "The great temple at Gamla Uppsala still draws pilgrims from across the Norse world. The Gothi who guard it have grown complacent, collecting tithes and performing rituals by rote. You arrive as a pilgrim yourself, bringing fresh fervor. The head Gothi, an aging man named Sten, eyes you with suspicion. 'Another wanderer with bright eyes. The temple has seen a thousand like you. What makes you different?'",
                new ConversionChoice(
                    "Preach in the temple courtyard",
                    "−100 Faith, +progress if you draw a crowd",
                    s => { s.Faith -= 100; if (s.Faith < 0) s.Faith = 0; s.Followers += 2; return "Your words cut through the ritualized chants. Pilgrims stop to listen. Two young men pledge themselves to your cause. +2 Followers."; }),
                new ConversionChoice(
                    "Challenge Sten's authority directly",
                    "Risky: +progress if you intimidate",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.4)
                        {
                            s.Faith += 80;
                            return "Sten's composure cracks. The other Gothi murmur. Your boldness has shaken their confidence. +80 Faith.";
                        }
                        s.Followers -= 2;
                        return "Sten laughs and has you ejected from the courtyard. Two followers slip away in the shame. −2 Followers.";
                    }),
                0.25, 0.5),

            new ConversionStep(
                "ug_2",
                "The Winter Blót",
                "Winter falls hard on Uppsala. The annual Blót — the great sacrifice to the gods — approaches. Sten invites you to participate, a test of your devotion. 'We sacrifice the nine kinds. The blood must be real. If your god is stronger, prove it at the altar.' The temple mound looms against the snow, torches flickering.",
                new ConversionChoice(
                    "Perform a rival sacrifice with greater fervor",
                    "−180 Faith, +progress if the omens favor you",
                    s =>
                    {
                        s.Faith -= 180; if (s.Faith < 0) s.Faith = 0;
                        if (Random.Shared.NextDouble() < 0.55)
                        {
                            s.Followers += 5;
                            return "Your sacrifice is visceral and raw. The gathered crowd gasps as ravens circle the mound — a sign Odin himself has noticed. Five pilgrims join you. +5 Followers.";
                        }
                        s.Followers -= 3;
                        return "The omens are ambiguous. Sten declares the gods prefer the old ways. Three followers drift back to the temple. −3 Followers.";
                    }),
                new ConversionChoice(
                    "Subvert the ritual from within",
                    "−80 Faith, safe: small progress",
                    s => { s.Faith -= 80; if (s.Faith < 0) s.Faith = 0; s.Followers += 1; return "You insinuate yourself into the ritual, subtly redirecting the energy. The Gothi don't notice, but a sharp-eyed priestess does. She joins you after. +1 Follower."; }),
                0.25, 0.5),

            new ConversionStep(
                "ug_3",
                "The Seiðr Alliance",
                "A Seiðr witch named Yrsa, who dwells in a hut on the edge of the temple grounds, approaches you secretly. 'I've watched you. Sten is a fool — he worships the form, not the power. I practice the old seiðr, the magic that walks between worlds. The Gothi fear me. Join with me, and we'll take the temple from within. But I won't be discarded after.'",
                new ConversionChoice(
                    "Forge an alliance with Yrsa",
                    "−120 Faith, +progress, but you share power",
                    s => { s.Faith -= 120; if (s.Faith < 0) s.Faith = 0; s.Followers += 4; return "Yrsa's seiðr and your charisma are a potent combination. She whispers to the temple servants, and four more join your cause. +4 Followers."; }),
                new ConversionChoice(
                    "Use her, then dispose of her",
                    "Risky: +progress if she doesn't see through you",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.35)
                        {
                            s.Faith += 200;
                            return "Yrsa never suspects. Her intelligence is invaluable, and when the time comes, she vanishes into the snow. +200 Faith.";
                        }
                        s.Followers -= 5;
                        return "Yrsa sees the betrayal in your eyes before you move. She curses you and vanishes — and five followers leave with her. −5 Followers.";
                    }),
                0.25, 0.5),

            new ConversionStep(
                "ug_4",
                "The Fall of the Temple",
                "The time has come. Sten's authority is crumbling. Pilgrims whisper your name. Yrsa's seiðr has undermined the temple's protections. The Gothi are divided — some have already joined you. Sten stands before the great oak, the last holdout. 'You think you can take the temple? The gods will not allow it. Prove your power here, at the sacred tree, or leave forever.'",
                new ConversionChoice(
                    "Overwhelm Sten with a show of force",
                    "−250 Faith, convert the coven",
                    s => { s.Faith -= 250; if (s.Faith < 0) s.Faith = 0; return null; }),
                new ConversionChoice(
                    "Offer Sten a place in the new order",
                    "Risky: +progress if he accepts",
                    s =>
                    {
                        if (Random.Shared.NextDouble() < 0.45)
                        {
                            s.Faith += 300; s.Followers += 6;
                            return "Sten sees the inevitable. He kneels at the oak and pledges the temple to you. Six of his most loyal join as well. +300 Faith, +6 Followers.";
                        }
                        s.Followers -= 4;
                        return "Sten spits at your feet. 'I would rather burn the temple than serve you.' He rallies the remaining Gothi. Four followers are lost in the clash. −4 Followers.";
                    }),
                0.25, 0.5),
        });
}
