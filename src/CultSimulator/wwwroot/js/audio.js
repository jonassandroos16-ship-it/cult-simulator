// ============================================================================
// CULT SIMULATOR — Procedural Audio Engine (Tone.js)
// ============================================================================
// All music and SFX are synthesized in-browser. No audio files needed.
// Requires Tone.js loaded via local <script> tag in index.html (js/tone.js).
//
// EXPOSED FUNCTIONS (called from C# via JS interop on window.cultAudio):
//   playTrack(name)                  — "menu"|"gameplay"|"map"|"combat"
//   playRegionalTrack(base, contId)  — regionally-tinted gameplay/combat
//   stopMusic()                       — fade out and stop all music
//   playUiSound(name)                 — "click"|"hover"|"error"|"success"
//   setMusicVolume(v)                 — 0.0–1.0
//   setSfxVolume(v)                   — 0.0–1.0
//   resumeAudio()                      — resume Tone context (call on first gesture)
//
// To add a new continent: add an entry to CONTINENT_CONFIG below with the
// continent ID matching your C# ContinentThemes.ByContinent key (lowercase
// string, e.g. "europe", "asia"). No other code changes needed.
// ============================================================================

// ============================================================================
// TUNABLE SOUND DESIGN PARAMETERS — edit these to adjust the mix
// ============================================================================

const CROSSFADE_MS = 1000;   // track transition crossfade duration

// --- Core track configs ---------------------------------------------------

const TRACK_CONFIG = {
  // MENU: slow, ominous drone — cold, ancient, foreboding.
  // Deep low pads + sparse ceremonial drum + occasional dissonant horn.
  menu: {
    rootNote: "C2",
    scale: [0, 3, 7, 10],
    tempo: 48,
    pad: {
      oscillator: "sine",
      envelope: { attack: 4, decay: 2, sustain: 0.8, release: 6 },
      gain: -14,
      filterFreq: 800
    },
    drone: {
      oscillator: "sawtooth",
      envelope: { attack: 6, decay: 0, sustain: 1, release: 8 },
      gain: -22,
      filterFreq: 220
    },
    horn: {
      notes: ["C3", "Eb3", "G3", "Bb3", "C4"],
      intervalSec: 12,
      oscillator: "fatsawtooth",
      envelope: { attack: 0.08, decay: 0.3, sustain: 0.4, release: 2.5 },
      gain: -18,
      filterFreq: 800
    },
    drum: {
      intervalSec: 2.5,
      pitch: "C1",
      oscillator: "sine",
      envelope: { attack: 0.005, decay: 0.4, sustain: 0, release: 0.3 },
      gain: -12
    }
  },

  // GAMEPLAY: darker ambient bed, subtle rhythmic undertone, low tension.
  gameplay: {
    rootNote: "A1",
    scale: [0, 2, 3, 5, 7, 8, 10],
    tempo: 60,
    pad: {
      oscillator: "sine",
      envelope: { attack: 3, decay: 1, sustain: 0.7, release: 5 },
      gain: -16,
      filterFreq: 600
    },
    drone: {
      oscillator: "triangle",
      envelope: { attack: 4, decay: 0, sustain: 1, release: 6 },
      gain: -24,
      filterFreq: 180
    },
    bass: {
      notes: ["A1", "A1", "C2", "E2"],
      stepSec: 2.0,
      oscillator: "sine",
      envelope: { attack: 0.05, decay: 0.3, sustain: 0.3, release: 0.8 },
      gain: -18
    },
    percussion: {
      intervalSec: 1.5,
      pitch: "A0",
      oscillator: "sine",
      envelope: { attack: 0.003, decay: 0.15, sustain: 0, release: 0.1 },
      gain: -20
    }
  },

  // MAP: spacious, atmospheric, sparse — "scheming/planning" feel.
  map: {
    rootNote: "D2",
    scale: [0, 2, 3, 5, 7, 8, 10],
    tempo: 40,
    pad: {
      oscillator: "sine",
      envelope: { attack: 5, decay: 2, sustain: 0.6, release: 8 },
      gain: -15,
      filterFreq: 800
    },
    drone: {
      oscillator: "sawtooth",
      envelope: { attack: 8, decay: 0, sustain: 1, release: 10 },
      gain: -26,
      filterFreq: 160
    },
    chime: {
      notes: ["D4", "F4", "A4", "D5"],
      intervalSec: 8,
      oscillator: "triangle",
      envelope: { attack: 0.02, decay: 1.5, sustain: 0, release: 3 },
      gain: -22,
      filterFreq: 2000
    }
  },

  // COMBAT: driving percussion, aggressive bass, faster tempo, dissonant stabs.
  combat: {
    rootNote: "E1",
    scale: [0, 1, 4, 6, 7, 10],
    tempo: 120,
    bass: {
      notes: ["E1", "E1", "E1", "F1", "E1", "E1", "Bb1", "B1"],
      stepSec: 0.25,
      oscillator: "fatsawtooth",
      envelope: { attack: 0.01, decay: 0.15, sustain: 0.2, release: 0.2 },
      gain: -10,
      filterFreq: 400,
      distortion: 0.4
    },
    stab: {
      notes: ["E3", "F3", "Bb3", "B3", "E4"],
      intervalSec: 1.0,
      oscillator: "sawtooth",
      envelope: { attack: 0.005, decay: 0.2, sustain: 0, release: 0.3 },
      gain: -14,
      filterFreq: 1200
    },
    percussion: {
      intervalSec: 0.5,
      pitch: "E0",
      oscillator: "square",
      envelope: { attack: 0.002, decay: 0.08, sustain: 0, release: 0.05 },
      gain: -8
    },
    pad: {
      oscillator: "sawtooth",
      envelope: { attack: 1, decay: 0, sustain: 0.5, release: 3 },
      gain: -20,
      filterFreq: 300
    }
  }
};

// --- Per-continent regional config ----------------------------------------
// Keys MUST match the string keys in ContinentThemes.ByContinent (C#).
// To add a new continent: copy an entry, change the key to your continent ID,
// and adjust the scale / root / timbre / percussion style.
//
// Musical rationale per entry:
//   europe        — Dorian mode on D, sine pads, slow frame-drum pulse (Norse/Viking)
//   north_america — Aeolian on C, triangle pads, softer tom-like beats (occult colonial)
//   south_america — Pentatonic minor on F#, sine+triangle, woodblock-style percussion
//   asia          — Hirajoshi scale, triangle/sine, tight muted percussion (shinobi/monk)
//   oceania       — Lydian on G, wide sine pads, sparse oceanic swell percussion
//   africa        — Dorian on G, warmer sawtooth pads, polyrhythmic djembe-style beats
//   middle_east   — Phrygian dominant on D, buzzy sawtooth, darbuka-style frame drums

const CONTINENT_CONFIG = {
  europe: {
    label: "Norse/Viking",
    rootNote: "D2",
    scale: [0, 2, 3, 5, 6, 7, 10],
    padWave: "sine",
    padGain: -15,
    drumWave: "sine",
    drumPitch: "D1",
    drumInterval: 2.0,
    bassWave: "sine",
    combatBassWave: "fatsawtooth",
    combatDistortion: 0.35,
    tempo: 52
  },
  north_america: {
    label: "Colonial Occult",
    rootNote: "C2",
    scale: [0, 2, 3, 5, 7, 8, 10],
    padWave: "triangle",
    padGain: -17,
    drumWave: "sine",
    drumPitch: "C1",
    drumInterval: 1.8,
    bassWave: "triangle",
    combatBassWave: "sawtooth",
    combatDistortion: 0.3,
    tempo: 56
  },
  south_america: {
    label: "Amazonian",
    rootNote: "F#2",
    scale: [0, 3, 5, 7, 10],
    padWave: "sine",
    padGain: -16,
    drumWave: "triangle",
    drumPitch: "F#1",
    drumInterval: 1.3,
    bassWave: "sine",
    combatBassWave: "fatsawtooth",
    combatDistortion: 0.4,
    tempo: 60
  },
  asia: {
    label: "Eastern",
    rootNote: "A2",
    scale: [0, 2, 5, 7, 9],
    padWave: "triangle",
    padGain: -18,
    drumWave: "square",
    drumPitch: "A1",
    drumInterval: 1.0,
    bassWave: "triangle",
    combatBassWave: "sawtooth",
    combatDistortion: 0.3,
    tempo: 64
  },
  oceania: {
    label: "Oceanic",
    rootNote: "G2",
    scale: [0, 2, 4, 5, 7, 9, 11],
    padWave: "sine",
    padGain: -14,
    drumWave: "sine",
    drumPitch: "G1",
    drumInterval: 3.0,
    bassWave: "sine",
    combatBassWave: "triangle",
    combatDistortion: 0.25,
    tempo: 44
  },
  africa: {
    label: "African",
    rootNote: "G2",
    scale: [0, 2, 3, 5, 6, 7, 10],
    padWave: "sawtooth",
    padGain: -18,
    drumWave: "sine",
    drumPitch: "G1",
    drumInterval: 0.8,
    bassWave: "sine",
    combatBassWave: "fatsawtooth",
    combatDistortion: 0.35,
    tempo: 68
  },
  middle_east: {
    label: "Middle Eastern",
    rootNote: "D2",
    scale: [0, 1, 4, 5, 7, 8, 11],
    padWave: "sawtooth",
    padGain: -17,
    drumWave: "square",
    drumPitch: "D1",
    drumInterval: 0.75,
    bassWave: "sawtooth",
    combatBassWave: "fatsawtooth",
    combatDistortion: 0.45,
    tempo: 72
  }
};

// --- UI sound configs -----------------------------------------------------

const UI_SOUNDS = {
  click: {
    oscillator: "sine",
    freq: 110,
    envelope: { attack: 0.003, decay: 0.12, sustain: 0, release: 0.08 },
    gain: -10,
    pitchDrop: 30
  },
  hover: {
    oscillator: "triangle",
    freq: 220,
    envelope: { attack: 0.005, decay: 0.06, sustain: 0, release: 0.04 },
    gain: -18
  },
  error: {
    oscillator: "sawtooth",
    freq: 90,
    envelope: { attack: 0.005, decay: 0.25, sustain: 0, release: 0.15 },
    gain: -12,
    pitchDrop: 40,
    filterFreq: 400
  },
  success: {
    oscillator: "sine",
    freq: 180,
    envelope: { attack: 0.01, decay: 0.3, sustain: 0, release: 0.4 },
    gain: -14,
    pitchRise: 120
  }
};

// ============================================================================
// ENGINE — no need to edit below this line for sound tuning
// ============================================================================

window.cultAudio = (function () {
  let _musicVol = parseFloat(localStorage.getItem("cult_musicVol")) || 0.5;
  let _sfxVol = parseFloat(localStorage.getItem("cult_sfxVol")) || 0.6;
  let _currentTrack = null;
  let _nodes = null;
  let _started = false;
  let _gestureListenerAttached = false;
  let _pendingTrack = null;

  // --- Audio context startup ----------------------------------------------

  async function ensureStarted() {
    if (_started) return;
    if (typeof Tone === "undefined") { console.warn("[cultAudio] Tone.js not loaded — audio disabled."); return; }
    await Tone.start();
    _started = true;
    // If a track was requested before the audio context was ready, start it now.
    if (_pendingTrack) {
      var pending = _pendingTrack;
      _pendingTrack = null;
      playTrackInternal(pending.key, pending.name, pending.continentId);
    }
  }

  // Attach a one-time listener so the audio context resumes on the first
  // user gesture (click/touch/keydown). Browsers block audio until then.
  function attachGestureListener() {
    if (_gestureListenerAttached) return;
    _gestureListenerAttached = true;
    const resume = () => { ensureStarted(); };
    document.addEventListener("pointerdown", resume, { once: false, passive: true });
    document.addEventListener("keydown", resume, { once: false, passive: true });
  }
  attachGestureListener();

  // --- Helper: note from scale degree -------------------------------------

  function noteFromScale(root, scale, degree) {
    const octave = Math.floor(degree / scale.length);
    const idx = ((degree % scale.length) + scale.length) % scale.length;
    const semis = scale[idx] + octave * 12;
    return Tone.Frequency(root).transpose(semis).toNote();
  }

  // --- Helper: create synth chains ----------------------------------------

  function makePad(cfg) {
    const synth = new Tone.PolySynth(Tone.Synth, {
      oscillator: { type: cfg.oscillator || "sine" },
      envelope: cfg.envelope || { attack: 3, decay: 1, sustain: 0.7, release: 5 }
    });
    const filter = new Tone.Filter(cfg.filterFreq || 800, "lowpass");
    const vol = new Tone.Volume(cfg.gain || -16);
    synth.connect(filter);
    filter.connect(vol);
    vol.toDestination();
    return { synth, vol, filter };
  }

  function makeDrone(cfg) {
    // Use an AMSynth for a sustained drone with envelope control
    const synth = new Tone.AMSynth({
      harmonicity: 1.5,
      oscillator: { type: cfg.oscillator || "sawtooth" },
      envelope: cfg.envelope || { attack: 4, decay: 0, sustain: 1, release: 6 },
      modulation: { type: "sine" },
      modulationEnvelope: { attack: 2, decay: 0, sustain: 1, release: 4 }
    });
    const filter = new Tone.Filter(cfg.filterFreq || 200, "lowpass");
    const vol = new Tone.Volume(cfg.gain || -24);
    synth.connect(filter);
    filter.connect(vol);
    vol.toDestination();
    return { synth, vol, filter };
  }

  function makeBass(cfg) {
    const synth = new Tone.Synth({
      oscillator: { type: cfg.oscillator || "sine" },
      envelope: cfg.envelope || { attack: 0.05, decay: 0.3, sustain: 0.3, release: 0.8 }
    });
    const filter = new Tone.Filter(cfg.filterFreq || 400, "lowpass");
    const vol = new Tone.Volume(cfg.gain || -18);
    synth.connect(filter);
    if (cfg.distortion) {
      const dist = new Tone.Distortion(cfg.distortion);
      filter.connect(dist);
      dist.connect(vol);
    } else {
      filter.connect(vol);
    }
    vol.toDestination();
    return { synth, vol, filter };
  }

  function makePercussion(cfg) {
    // Use MembraneSynth for drum-like sounds
    const synth = new Tone.MembraneSynth({
      pitchDecay: 0.05,
      octaves: 4,
      envelope: cfg.envelope || { attack: 0.003, decay: 0.15, sustain: 0, release: 0.1 }
    });
    const vol = new Tone.Volume(cfg.gain || -18);
    synth.connect(vol);
    vol.toDestination();
    return { synth, vol };
  }

  function makeChime(cfg) {
    const synth = new Tone.Synth({
      oscillator: { type: cfg.oscillator || "triangle" },
      envelope: cfg.envelope || { attack: 0.02, decay: 1.5, sustain: 0, release: 3 }
    });
    const filter = new Tone.Filter(cfg.filterFreq || 2000, "lowpass");
    const vol = new Tone.Volume(cfg.gain || -22);
    synth.connect(filter);
    filter.connect(vol);
    vol.toDestination();
    return { synth, vol, filter };
  }

  // --- Track builders ----------------------------------------------------

  function buildMenuTrack(cfg) {
    const pad = makePad(cfg.pad);
    const drone = makeDrone(cfg.drone);
    const drum = makePercussion(cfg.drum);
    const horn = makeChime(cfg.horn);

    // Start drone — sustained note
    drone.synth.triggerAttack(cfg.rootNote);

    // Pad loop — slow chord changes
    let padDegree = 0;
    const padLoop = new Tone.Loop((time) => {
      const notes = [
        noteFromScale(cfg.rootNote, cfg.scale, padDegree),
        noteFromScale(cfg.rootNote, cfg.scale, padDegree + 2),
        noteFromScale(cfg.rootNote, cfg.scale, padDegree + 4)
      ];
      pad.synth.triggerAttackRelease(notes, "2n", time);
      padDegree = (padDegree + 2) % cfg.scale.length;
    }, "1m");
    padLoop.start(0);

    // Drum pulse
    const drumLoop = new Tone.Loop((time) => {
      drum.synth.triggerAttackRelease(cfg.drum.pitch, "8n", time);
    }, cfg.drum.intervalSec);
    drumLoop.start(0);

    // Horn stabs
    let hornIdx = 0;
    const hornLoop = new Tone.Loop((time) => {
      const note = cfg.horn.notes[hornIdx % cfg.horn.notes.length];
      horn.synth.triggerAttackRelease(note, "2n", time);
      hornIdx++;
    }, cfg.horn.intervalSec);
    hornLoop.start(0);

    Tone.Transport.bpm.value = cfg.tempo;

    return {
      type: "menu",
      nodes: [pad, drone, drum, horn],
      loops: [padLoop, drumLoop, hornLoop]
    };
  }

  function buildGameplayTrack(cfg, regionCfg) {
    const root = regionCfg ? regionCfg.rootNote : cfg.rootNote;
    const scale = regionCfg ? regionCfg.scale : cfg.scale;
    const tempo = regionCfg ? (regionCfg.tempo || cfg.tempo) : cfg.tempo;
    const padWave = regionCfg ? regionCfg.padWave : cfg.pad.oscillator;
    const padGain = regionCfg ? regionCfg.padGain : cfg.pad.gain;
    const drumWave = regionCfg ? regionCfg.drumWave : cfg.percussion.oscillator;
    const drumPitch = regionCfg ? regionCfg.drumPitch : cfg.percussion.pitch;
    const drumInterval = regionCfg ? regionCfg.drumInterval : cfg.percussion.intervalSec;
    const bassWave = regionCfg ? regionCfg.bassWave : cfg.bass.oscillator;

    // Pad
    const padSynth = new Tone.PolySynth(Tone.Synth, {
      oscillator: { type: padWave },
      envelope: cfg.pad.envelope
    });
    const padFilter = new Tone.Filter(cfg.pad.filterFreq, "lowpass");
    const padVol = new Tone.Volume(padGain);
    padSynth.connect(padFilter);
    padFilter.connect(padVol);
    padVol.toDestination();

    // Drone
    const droneSynth = new Tone.AMSynth({
      harmonicity: 1.2,
      oscillator: { type: cfg.drone.oscillator },
      envelope: cfg.drone.envelope,
      modulation: { type: "sine" },
      modulationEnvelope: { attack: 2, decay: 0, sustain: 1, release: 4 }
    });
    const droneFilter = new Tone.Filter(cfg.drone.filterFreq, "lowpass");
    const droneVol = new Tone.Volume(cfg.drone.gain);
    droneSynth.connect(droneFilter);
    droneFilter.connect(droneVol);
    droneVol.toDestination();
    droneSynth.triggerAttack(root);

    // Bass
    const bassSynth = new Tone.Synth({
      oscillator: { type: bassWave },
      envelope: cfg.bass.envelope
    });
    const bassVol = new Tone.Volume(cfg.bass.gain);
    bassSynth.connect(bassVol);
    bassVol.toDestination();

    // Percussion
    const percSynth = new Tone.MembraneSynth({
      pitchDecay: 0.03,
      octaves: 3,
      envelope: cfg.percussion.envelope
    });
    const percVol = new Tone.Volume(cfg.percussion.gain);
    percSynth.connect(percVol);
    percVol.toDestination();

    // Pad loop
    let padDegree = 0;
    const padLoop = new Tone.Loop((time) => {
      const notes = [
        noteFromScale(root, scale, padDegree),
        noteFromScale(root, scale, padDegree + 2),
        noteFromScale(root, scale, padDegree + 4)
      ];
      padSynth.triggerAttackRelease(notes, "1m", time);
      padDegree = (padDegree + 2) % scale.length;
    }, "2m");
    padLoop.start(0);

    // Bass loop
    let bassStep = 0;
    const bassLoop = new Tone.Loop((time) => {
      const note = cfg.bass.notes[bassStep % cfg.bass.notes.length];
      bassSynth.triggerAttackRelease(note, "8n", time);
      bassStep++;
    }, cfg.bass.stepSec);
    bassLoop.start(0);

    // Percussion loop
    const percLoop = new Tone.Loop((time) => {
      percSynth.triggerAttackRelease(drumPitch, "8n", time);
    }, drumInterval);
    percLoop.start(0);

    Tone.Transport.bpm.value = tempo;

    return {
      type: "gameplay",
      nodes: [
        { synth: padSynth, vol: padVol },
        { synth: droneSynth, vol: droneVol },
        { synth: bassSynth, vol: bassVol },
        { synth: percSynth, vol: percVol }
      ],
      loops: [padLoop, bassLoop, percLoop]
    };
  }

  function buildMapTrack(cfg) {
    const pad = makePad(cfg.pad);
    const drone = makeDrone(cfg.drone);
    const chime = makeChime(cfg.chime);

    drone.synth.triggerAttack(cfg.rootNote);

    let padDegree = 0;
    const padLoop = new Tone.Loop((time) => {
      const notes = [
        noteFromScale(cfg.rootNote, cfg.scale, padDegree),
        noteFromScale(cfg.rootNote, cfg.scale, padDegree + 2),
        noteFromScale(cfg.rootNote, cfg.scale, padDegree + 4)
      ];
      pad.synth.triggerAttackRelease(notes, "1m", time);
      padDegree = (padDegree + 2) % cfg.scale.length;
    }, "2m");
    padLoop.start(0);

    let chimeIdx = 0;
    const chimeLoop = new Tone.Loop((time) => {
      const note = cfg.chime.notes[chimeIdx % cfg.chime.notes.length];
      chime.synth.triggerAttackRelease(note, "4n", time);
      chimeIdx++;
    }, cfg.chime.intervalSec);
    chimeLoop.start(0);

    Tone.Transport.bpm.value = cfg.tempo;

    return {
      type: "map",
      nodes: [pad, drone, chime],
      loops: [padLoop, chimeLoop]
    };
  }

  function buildCombatTrack(cfg, regionCfg) {
    const root = regionCfg ? regionCfg.rootNote : cfg.rootNote;
    const scale = regionCfg ? regionCfg.scale : cfg.scale;
    const tempo = regionCfg ? (regionCfg.tempo || cfg.tempo) : cfg.tempo;
    const bassWave = regionCfg ? regionCfg.combatBassWave : cfg.bass.oscillator;
    const distortion = regionCfg ? regionCfg.combatDistortion : cfg.bass.distortion;

    // Bass with distortion
    const bassSynth = new Tone.Synth({
      oscillator: { type: bassWave },
      envelope: cfg.bass.envelope
    });
    const bassFilter = new Tone.Filter(cfg.bass.filterFreq, "lowpass");
    const bassVol = new Tone.Volume(cfg.bass.gain);
    bassSynth.connect(bassFilter);
    if (distortion) {
      const dist = new Tone.Distortion(distortion);
      bassFilter.connect(dist);
      dist.connect(bassVol);
    } else {
      bassFilter.connect(bassVol);
    }
    bassVol.toDestination();

    // Stabs
    const stabSynth = new Tone.Synth({
      oscillator: { type: cfg.stab.oscillator },
      envelope: cfg.stab.envelope
    });
    const stabFilter = new Tone.Filter(cfg.stab.filterFreq, "lowpass");
    const stabVol = new Tone.Volume(cfg.stab.gain);
    stabSynth.connect(stabFilter);
    stabFilter.connect(stabVol);
    stabVol.toDestination();

    // Percussion
    const percSynth = new Tone.MembraneSynth({
      pitchDecay: 0.02,
      octaves: 4,
      envelope: cfg.percussion.envelope
    });
    const percVol = new Tone.Volume(cfg.percussion.gain);
    percSynth.connect(percVol);
    percVol.toDestination();

    // Pad
    const padSynth = new Tone.PolySynth(Tone.Synth, {
      oscillator: { type: cfg.pad.oscillator },
      envelope: cfg.pad.envelope
    });
    const padFilter = new Tone.Filter(cfg.pad.filterFreq, "lowpass");
    const padVol = new Tone.Volume(cfg.pad.gain);
    padSynth.connect(padFilter);
    padFilter.connect(padVol);
    padVol.toDestination();

    // Bass loop — driving
    let bassStep = 0;
    const bassLoop = new Tone.Loop((time) => {
      const note = cfg.bass.notes[bassStep % cfg.bass.notes.length];
      bassSynth.triggerAttackRelease(note, "16n", time);
      bassStep++;
    }, cfg.bass.stepSec);
    bassLoop.start(0);

    // Stab loop — dissonant tension
    let stabIdx = 0;
    const stabLoop = new Tone.Loop((time) => {
      const note = cfg.stab.notes[stabIdx % cfg.stab.notes.length];
      stabSynth.triggerAttackRelease(note, "8n", time);
      stabIdx++;
    }, cfg.stab.intervalSec);
    stabLoop.start(0);

    // Percussion loop — driving beat
    const percLoop = new Tone.Loop((time) => {
      percSynth.triggerAttackRelease(cfg.percussion.pitch, "16n", time);
    }, cfg.percussion.intervalSec);
    percLoop.start(0);

    // Pad loop — dark bed
    let padDegree = 0;
    const padLoop = new Tone.Loop((time) => {
      const notes = [
        noteFromScale(root, scale, padDegree),
        noteFromScale(root, scale, padDegree + 2),
        noteFromScale(root, scale, padDegree + 4)
      ];
      padSynth.triggerAttackRelease(notes, "2n", time);
      padDegree = (padDegree + 1) % scale.length;
    }, "1m");
    padLoop.start(0);

    Tone.Transport.bpm.value = tempo;

    return {
      type: "combat",
      nodes: [
        { synth: bassSynth, vol: bassVol },
        { synth: stabSynth, vol: stabVol },
        { synth: percSynth, vol: percVol },
        { synth: padSynth, vol: padVol }
      ],
      loops: [bassLoop, stabLoop, percLoop, padLoop]
    };
  }

  // --- Track lifecycle ----------------------------------------------------

  function teardownTrack(track) {
    if (!track) return;
    track.loops.forEach(l => { try { l.stop(); l.dispose(); } catch (e) {} });
    track.nodes.forEach(n => {
      try {
        if (n.synth) { try { n.synth.triggerRelease(); } catch(e){} n.synth.dispose(); }
        if (n.osc) { try { n.osc.stop(); } catch(e){} n.osc.dispose(); }
        if (n.env) n.env.dispose();
        if (n.vol) n.vol.dispose();
        if (n.filter) n.filter.dispose();
      } catch (e) {}
    });
  }

  function fadeOut(track, ms) {
    return new Promise(resolve => {
      if (!track) { resolve(); return; }
      track.nodes.forEach(n => {
        if (n.vol) n.vol.volume.rampTo(-60, ms / 1000);
      });
      setTimeout(() => {
        teardownTrack(track);
        resolve();
      }, ms);
    });
  }

  function buildTrack(trackName, continentId) {
    const cfg = TRACK_CONFIG[trackName];
    if (!cfg) return null;

    let regionCfg = null;
    if (continentId) {
      const key = continentId.toLowerCase();
      regionCfg = CONTINENT_CONFIG[key] || null;
    }

    switch (trackName) {
      case "menu":     return buildMenuTrack(cfg);
      case "gameplay": return buildGameplayTrack(cfg, regionCfg);
      case "map":      return buildMapTrack(cfg);
      case "combat":   return buildCombatTrack(cfg, regionCfg);
      default:         return null;
    }
  }

  async function playTrackInternal(trackKey, trackName, continentId) {
    await ensureStarted();
    if (!_started) { _pendingTrack = { key: trackKey, name: trackName, continentId }; return; }

    if (_currentTrack === trackKey) return;
    _currentTrack = trackKey;

    const old = _nodes;
    if (old) {
      await fadeOut(old, CROSSFADE_MS);
    }

    const track = buildTrack(trackName, continentId);
    if (!track) return;
    _nodes = track;

    // Start at low volume and ramp up
    track.nodes.forEach(n => {
      if (n.vol) n.vol.volume.value = -60;
    });

    const db = Tone.gainToDb(_musicVol);
    track.nodes.forEach(n => {
      if (n.vol) n.vol.volume.rampTo(db, CROSSFADE_MS / 1000);
    });

    if (Tone.Transport.state !== "started") {
      Tone.Transport.start();
    }
  }

  // --- Public API ---------------------------------------------------------

  return {
    resumeAudio: async function () {
      await ensureStarted();
    },

    playTrack: async function (name) {
      await playTrackInternal(name, name, null);
    },

    playRegionalTrack: async function (baseTrack, continentId) {
      const key = continentId ? baseTrack + ":" + continentId.toLowerCase() : baseTrack;
      await playTrackInternal(key, baseTrack, continentId);
    },

    stopMusic: async function () {
      _currentTrack = null;
      if (_nodes) {
        await fadeOut(_nodes, CROSSFADE_MS);
        _nodes = null;
      }
    },

    playUiSound: async function (name) {
      await ensureStarted();
      if (!_started) return;
      const cfg = UI_SOUNDS[name];
      if (!cfg) return;

      const synth = new Tone.Synth({
        oscillator: { type: cfg.oscillator },
        envelope: cfg.envelope
      });
      const vol = new Tone.Volume(cfg.gain + Tone.gainToDb(_sfxVol));

      if (cfg.filterFreq) {
        const filter = new Tone.Filter(cfg.filterFreq, "lowpass");
        synth.connect(filter);
        filter.connect(vol);
      } else {
        synth.connect(vol);
      }
      vol.toDestination();

      const now = Tone.now();
      synth.triggerAttack(cfg.freq, now);

      if (cfg.pitchDrop) {
        synth.frequency.rampTo(cfg.freq - cfg.pitchDrop, 0.15, now);
      }
      if (cfg.pitchRise) {
        synth.frequency.rampTo(cfg.freq + cfg.pitchRise, 0.2, now);
      }

      setTimeout(() => {
        try { synth.triggerRelease(); } catch(e){}
        setTimeout(() => {
          try { synth.dispose(); vol.dispose(); } catch (e) {}
        }, 500);
      }, 250);
    },

    setMusicVolume: function (v) {
      _musicVol = Math.max(0, Math.min(1, v));
      try { localStorage.setItem("cult_musicVol", _musicVol); } catch (e) {}
      if (_nodes) {
        const db = Tone.gainToDb(_musicVol);
        _nodes.nodes.forEach(n => {
          if (n.vol) n.vol.volume.rampTo(db, 0.2);
        });
      }
    },

    setSfxVolume: function (v) {
      _sfxVol = Math.max(0, Math.min(1, v));
      try { localStorage.setItem("cult_sfxVol", _sfxVol); } catch (e) {}
    },

    getCurrentTrack: function () {
      return _currentTrack;
    },

    getVolumes: function () {
      return { music: _musicVol, sfx: _sfxVol };
    }
  };
})();
