import {
  COVEN_DEFS,
  COVEN_MAP,
  INSTITUTION_DEFS,
  INSTITUTION_MAP,
  TERRITORY_DEFS,
  TERRITORY_MAP,
} from './data';
import type {
  CovenDef,
  InstitutionDef,
  InstitutionState,
  ShadowWarState,
  TerritoryDef,
} from './types';

const TICK_MS = 1000;
const SAVE_KEY = 'shadow-war-save-v1';
const ALERT_COOLDOWN_MS = 30_000;
const INVESTIGATION_INTERVAL_MS = 60_000;
const INVESTIGATION_DEFENSE_DECAY = 5;
const DEFEND_HOLD_RATE = 3;

export function createInitialState(): ShadowWarState {
  const institutions: Record<string, InstitutionState> = {};
  for (const def of INSTITUTION_DEFS) {
    institutions[def.id] = {
      id: def.id,
      status: def.prerequisites?.length ? 'locked' : 'unlocked',
      defenseRemaining: def.defense,
      detection: 0,
      assignedAgents: 0,
      reconProgress: 0,
      controlProgress: 0,
      investigationDefense: 0,
      cooldownUntil: 0,
    };
  }
  return {
    faith: 0,
    heat: 0,
    totalAgents: 0,
    deployedAgents: 0,
    covens: [{ id: 'skanor', acolytes: 5, armyPower: 10 }],
    activeCovenId: 'skanor',
    institutions,
    lastTick: Date.now(),
    victoryAchieved: false,
    prestigeMultiplier: 1,
    totalControlled: 0,
  };
}

export function getControlledInstitutions(state: ShadowWarState): InstitutionDef[] {
  return INSTITUTION_DEFS.filter((d) => state.institutions[d.id]?.status === 'controlled');
}

export function isTerritoryControlled(state: ShadowWarState, territoryId: string): boolean {
  const t = TERRITORY_MAP[territoryId];
  return t.institutionIds.every((id) => state.institutions[id]?.status === 'controlled');
}

export function getControlledTerritories(state: ShadowWarState): TerritoryDef[] {
  return TERRITORY_DEFS.filter((t) => isTerritoryControlled(state, t.id));
}

export function isAllTerritoriesControlled(state: ShadowWarState): boolean {
  return TERRITORY_DEFS.every((t) => isTerritoryControlled(state, t.id));
}

export function getFaithPerSecond(state: ShadowWarState): number {
  let total = 0;
  for (const cs of state.covens) {
    const def = COVEN_MAP[cs.id];
    if (!def) continue;
    let rate = def.faithPerSecond * (1 + cs.acolytes * 0.02);
    if (isTerritoryControlled(state, def.territoryId)) {
      rate *= TERRITORY_MAP[def.territoryId].faithMultiplier;
    }
    const financeBonus = getControlledInstitutions(state)
      .filter((i) => i.reward.type === 'finance')
      .reduce((sum, i) => sum + i.reward.value, 0);
    rate *= 1 + financeBonus;
    total += rate;
  }
  return total;
}

export function getAgentProductionPerSecond(state: ShadowWarState): number {
  let total = 0;
  for (const cs of state.covens) {
    const def = COVEN_MAP[cs.id];
    if (!def) continue;
    total += cs.acolytes * 0.02;
  }
  const govBonus = getControlledInstitutions(state)
    .filter((i) => i.reward.type === 'government')
    .reduce((sum, i) => sum + i.reward.value, 0);
  total *= 1 + govBonus;
  const controlledTerritories = getControlledTerritories(state);
  const agentMult = controlledTerritories.reduce((m, t) => m * t.agentMultiplier, 1);
  total *= agentMult;
  return total;
}

export function getAgentStrength(state: ShadowWarState): number {
  let base = 1;
  const militaryBonus = getControlledInstitutions(state)
    .filter((i) => i.reward.type === 'military')
    .reduce((sum, i) => sum + i.reward.value, 0);
  base *= 1 + militaryBonus;
  const armyPower = state.covens.reduce((sum, cs) => sum + cs.armyPower, 0);
  base *= 1 + armyPower * 0.001;
  return base;
}

export function getDetectionMultiplier(state: ShadowWarState): number {
  const mediaReduction = getControlledInstitutions(state)
    .filter((i) => i.reward.type === 'media')
    .reduce((sum, i) => sum + i.reward.value, 0);
  return Math.max(0.3, 1 - mediaReduction);
}

export function getReconRiskMultiplier(state: ShadowWarState): number {
  const intelReduction = getControlledInstitutions(state)
    .filter((i) => i.reward.type === 'intelligence')
    .reduce((sum, i) => sum + i.reward.value, 0);
  return Math.max(0.3, 1 - intelReduction);
}

export function getSuspicionDecay(state: ShadowWarState): number {
  return getControlledInstitutions(state)
    .filter((i) => i.reward.type === 'police')
    .reduce((sum, i) => sum + i.reward.value, 0);
}

export function getAvailableAgents(state: ShadowWarState): number {
  return Math.floor(state.totalAgents - state.deployedAgents);
}

export function getCovenDef(id: string): CovenDef | undefined {
  return COVEN_MAP[id];
}

export function getInstitutionDef(id: string): InstitutionDef | undefined {
  return INSTITUTION_MAP[id];
}

export function getTerritoryDef(id: string): TerritoryDef | undefined {
  return TERRITORY_MAP[id];
}

export function getNextCovenToUnlock(state: ShadowWarState): CovenDef | undefined {
  return COVEN_DEFS.find((c) => !state.covens.some((cs) => cs.id === c.id));
}

export function getFoundCovenCost(state: ShadowWarState): number {
  const next = getNextCovenToUnlock(state);
  return next ? next.unlockCost : Infinity;
}

export interface ActionResult {
  success: boolean;
  message: string;
}

export function foundCoven(state: ShadowWarState): ActionResult {
  const next = getNextCovenToUnlock(state);
  if (!next) return { success: false, message: 'All covens have been founded.' };
  if (state.faith < next.unlockCost)
    return { success: false, message: `Need ${Math.ceil(next.unlockCost - state.faith)} more faith.` };
  state.faith -= next.unlockCost;
  state.covens.push({ id: next.id, acolytes: 3, armyPower: 5 });
  state.activeCovenId = next.id;
  return { success: true, message: `${next.name} has been founded.` };
}

export function recruitAcolytes(state: ShadowWarState, count: number): ActionResult {
  const coven = state.covens.find((c) => c.id === state.activeCovenId);
  if (!coven) return { success: false, message: 'No active coven.' };
  const def = COVEN_MAP[coven.id];
  if (!def) return { success: false, message: 'Unknown coven.' };
  if (coven.acolytes + count > def.acolyteCapacity)
    return { success: false, message: 'Not enough capacity for that many acolytes.' };
  const cost = count * 10;
  if (state.faith < cost)
    return { success: false, message: `Need ${Math.ceil(cost - state.faith)} more faith.` };
  state.faith -= cost;
  coven.acolytes += count;
  coven.armyPower += count * def.armyPowerPerAcolyte;
  return { success: true, message: `Recruited ${count} acolytes.` };
}

export function trainAgents(state: ShadowWarState, count: number): ActionResult {
  const coven = state.covens.find((c) => c.id === state.activeCovenId);
  if (!coven) return { success: false, message: 'No active coven.' };
  if (coven.acolytes < count)
    return { success: false, message: 'Not enough acolytes to train.' };
  coven.acolytes -= count;
  state.totalAgents += count;
  return { success: true, message: `Trained ${count} sleeper agents.` };
}

export function startRecon(state: ShadowWarState, institutionId: string, agentCount: number): ActionResult {
  const inst = state.institutions[institutionId];
  const def = INSTITUTION_MAP[institutionId];
  if (!inst || !def) return { success: false, message: 'Unknown institution.' };
  if (inst.status !== 'unlocked') return { success: false, message: 'Cannot recon this institution now.' };
  if (agentCount < 1 || agentCount > 3) return { success: false, message: 'Send 1-3 agents for recon.' };
  if (getAvailableAgents(state) < agentCount) return { success: false, message: 'Not enough available agents.' };
  state.totalAgents -= agentCount;
  inst.status = 'recon';
  inst.assignedAgents = agentCount;
  inst.reconProgress = 0;
  return { success: true, message: `Recon team of ${agentCount} deployed to ${def.name}.` };
}

export function sendInfiltrationWave(state: ShadowWarState, institutionId: string, waveSize: number): ActionResult {
  const inst = state.institutions[institutionId];
  const def = INSTITUTION_MAP[institutionId];
  if (!inst || !def) return { success: false, message: 'Unknown institution.' };
  if (inst.status !== 'recon' && inst.status !== 'infiltrating')
    return { success: false, message: 'Must recon before infiltrating.' };
  if (waveSize < 1) return { success: false, message: 'Wave must have at least 1 agent.' };
  if (getAvailableAgents(state) < waveSize) return { success: false, message: 'Not enough available agents.' };
  state.totalAgents -= waveSize;
  inst.assignedAgents += waveSize;
  inst.status = 'infiltrating';
  return { success: true, message: `Wave of ${waveSize} agents sent to ${def.name}.` };
}

export function withdrawAgents(state: ShadowWarState, institutionId: string): ActionResult {
  const inst = state.institutions[institutionId];
  const def = INSTITUTION_MAP[institutionId];
  if (!inst || !def) return { success: false, message: 'Unknown institution.' };
  if (inst.assignedAgents <= 0) return { success: false, message: 'No agents deployed here.' };
  if (inst.status === 'controlled') return { success: false, message: 'Institution is controlled.' };
  state.totalAgents += inst.assignedAgents;
  inst.assignedAgents = 0;
  inst.status = 'unlocked';
  inst.reconProgress = 0;
  inst.detection = Math.max(0, inst.detection - 20);
  return { success: true, message: `Agents withdrawn from ${def.name}.` };
}

export function assignDefenders(state: ShadowWarState, institutionId: string, count: number): ActionResult {
  const inst = state.institutions[institutionId];
  const def = INSTITUTION_MAP[institutionId];
  if (!inst || !def) return { success: false, message: 'Unknown institution.' };
  if (inst.status !== 'investigated') return { success: false, message: 'Not under investigation.' };
  if (getAvailableAgents(state) < count) return { success: false, message: 'Not enough available agents.' };
  state.totalAgents -= count;
  inst.assignedAgents += count;
  return { success: true, message: `${count} agents assigned to defend ${def.name}.` };
}

export function tick(state: ShadowWarState, now: number): void {
  if (state.victoryAchieved) return;
  const deltaSec = Math.min(60, (now - state.lastTick) / 1000);
  state.lastTick = now;
  state.faith += getFaithPerSecond(state) * deltaSec;
  state.totalAgents += getAgentProductionPerSecond(state) * deltaSec;
  state.heat = Math.max(0, state.heat - getSuspicionDecay(state) * deltaSec);
  for (const def of INSTITUTION_DEFS) {
    const inst = state.institutions[def.id];
    if (!inst) continue;
    processInstitution(state, inst, def, deltaSec, now);
  }
  updateLocks(state);
  maybeTriggerInvestigation(state, now);
  if (isAllTerritoriesControlled(state) && !state.victoryAchieved) {
    state.victoryAchieved = true;
    state.prestigeMultiplier = 5;
  }
  state.totalControlled = getControlledInstitutions(state).length;
}

function processInstitution(
  state: ShadowWarState,
  inst: InstitutionState,
  def: InstitutionDef,
  deltaSec: number,
  now: number,
): void {
  switch (inst.status) {
    case 'recon': {
      inst.reconProgress += 15 * deltaSec * inst.assignedAgents;
      if (inst.reconProgress >= 100) {
        inst.reconProgress = 100;
        inst.status = 'infiltrating';
      }
      break;
    }
    case 'infiltrating': {
      const strength = getAgentStrength(state);
      const damage = inst.assignedAgents * strength * 2 * deltaSec;
      inst.defenseRemaining = Math.max(0, inst.defenseRemaining - damage);
      inst.controlProgress = ((def.defense - inst.defenseRemaining) / def.defense) * 100;
      const detectionGain = def.detectionRate * inst.assignedAgents * deltaSec * getDetectionMultiplier(state);
      inst.detection = Math.min(100, inst.detection + detectionGain);
      if (inst.detection >= 100) {
        const lost = inst.assignedAgents;
        inst.assignedAgents = 0;
        inst.detection = 100;
        inst.status = 'alerted';
        inst.cooldownUntil = now + ALERT_COOLDOWN_MS;
        state.heat += 20 + lost * 5;
        break;
      }
      if (inst.defenseRemaining <= 0) {
        inst.status = 'controlled';
        inst.detection = 0;
        inst.assignedAgents = 0;
        inst.controlProgress = 100;
        inst.investigationDefense = 0;
      }
      break;
    }
    case 'alerted': {
      inst.detection = Math.max(0, inst.detection - 5 * deltaSec);
      if (now >= inst.cooldownUntil) {
        inst.status = 'unlocked';
        inst.detection = 0;
        inst.defenseRemaining = def.defense;
        inst.controlProgress = 0;
      }
      break;
    }
    case 'investigated': {
      const defend = inst.assignedAgents * DEFEND_HOLD_RATE * deltaSec;
      const decay = INVESTIGATION_DEFENSE_DECAY * deltaSec;
      inst.investigationDefense = Math.max(0, inst.investigationDefense + defend - decay);
      if (inst.investigationDefense <= 0) {
        inst.status = 'unlocked';
        inst.assignedAgents = 0;
        inst.defenseRemaining = def.defense;
        inst.detection = 0;
        inst.controlProgress = 0;
        state.heat += 15;
      } else if (inst.investigationDefense >= 100) {
        inst.status = 'controlled';
        inst.assignedAgents = 0;
        inst.investigationDefense = 0;
      }
      break;
    }
    case 'controlled':
    case 'locked':
    case 'unlocked':
      break;
  }
}

function updateLocks(state: ShadowWarState): void {
  for (const def of INSTITUTION_DEFS) {
    const inst = state.institutions[def.id];
    if (!inst || inst.status !== 'locked') continue;
    if (!def.prerequisites?.length) {
      inst.status = 'unlocked';
      continue;
    }
    const allPrereqsMet = def.prerequisites.every(
      (pid) => state.institutions[pid]?.status === 'controlled',
    );
    if (allPrereqsMet) {
      inst.status = 'unlocked';
    }
  }
}

let nextInvestigationTime = 0;

function maybeTriggerInvestigation(state: ShadowWarState, now: number): void {
  if (now < nextInvestigationTime) return;
  const controlled = getControlledInstitutions(state);
  if (controlled.length === 0) {
    nextInvestigationTime = now + INVESTIGATION_INTERVAL_MS;
    return;
  }
  const investigationChance = Math.min(0.8, 0.15 + controlled.length * 0.03);
  if (Math.random() < investigationChance) {
    const target = controlled[Math.floor(Math.random() * controlled.length)];
    const inst = state.institutions[target.id];
    if (inst && inst.status === 'controlled') {
      inst.status = 'investigated';
      inst.investigationDefense = 50;
      inst.assignedAgents = 0;
    }
  }
  const interval = Math.max(15000, INVESTIGATION_INTERVAL_MS - controlled.length * 2000);
  nextInvestigationTime = now + interval;
}

export function saveState(state: ShadowWarState): void {
  try {
    localStorage.setItem(SAVE_KEY, JSON.stringify(state));
  } catch {
    // ignore quota errors
  }
}

export function loadState(): ShadowWarState | null {
  try {
    const raw = localStorage.getItem(SAVE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as ShadowWarState;
    const initial = createInitialState();
    for (const def of INSTITUTION_DEFS) {
      if (!parsed.institutions[def.id]) {
        parsed.institutions[def.id] = initial.institutions[def.id];
      }
    }
    parsed.lastTick = Date.now();
    return parsed;
  } catch {
    return null;
  }
}

export function resetState(): void {
  try {
    localStorage.removeItem(SAVE_KEY);
  } catch {
    // ignore
  }
}

export { TICK_MS };
