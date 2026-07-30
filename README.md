# Cult Simulator - The Shadow War

A dark, atmospheric idle/management game where your cult seeps into society and conquers the world from the shadows. Built with React + TypeScript + Tailwind CSS.

## The Shadow War

Your cult does not fight openly - it infiltrates. Acolytes across all covens are trained into Sleeper Agents and deployed to infiltrate real-world institutions on a global infiltration map.

### Core Loop

1. Found covens - each coven produces faith and houses acolytes
2. Recruit acolytes - convert faith into followers
3. Train sleeper agents - convert acolytes into deployable agents
4. Recon institutions - send 1-3 agents to scout defenses and detection rates
5. Infiltrate - send waves of agents to reduce defense while managing detection
6. Control - once defense hits zero, the institution is yours with a permanent bonus
7. Defend - controlled institutions get investigated; assign agents to hold them

### Institution Types

- Police: Reduces heat over time
- Media: Lowers global detection rate
- Government: Boosts agent recruitment
- Military: Increases agent combat strength
- Finance: Boosts faith production
- Intelligence: Reduces recon losses

### Territories

The world is divided into 7 territories. Controlling all institutions in a territory grants a territory bonus (doubled faith from that territory covens). Controlling all territories triggers the endgame: The World Is Ours - a victory screen with a permanent prestige multiplier.

## Architecture

The game is modular and data-driven:

- src/game/types.ts - type system (institutions, territories, covens, state)
- src/game/data.ts - all game data definitions (7 territories, 22 institutions, 8 covens)
- src/game/engine.ts - game logic (tick, infiltration, detection, heat, counter-attacks, bonuses)
- src/game/useGame.ts - React hook for state management, tick loop, and localStorage persistence
- src/components/ - UI components (resource bar, coven panel, infiltration map, modals, victory screen)
