// ─── Core type system for the Shadow War ──────────────────────────────────

export type InstitutionType =
  | 'police'
  | 'media'
  | 'government'
  | 'military'
  | 'finance'
  | 'intelligence';

export type InstitutionTier = 1 | 2 | 3;

export type InstitutionStatus =
  | 'locked'
  | 'unlocked'
  | 'recon'
  | 'infiltrating'
  | 'controlled'
  | 'alerted'
  | 'investigated';

export interface InstitutionDef {
  id: string;
  name: string;
  territoryId: string;
  type: InstitutionType;
  tier: InstitutionTier;
  defense: number;
  detectionRate: number;
  reconRisk: number;
  reward: InstitutionReward;
  description: string;
  prerequisites?: string[];
}

export interface InstitutionReward {
  type: InstitutionType;
  label: string;
  value: number;
}

export interface TerritoryDef {
  id: string;
  name: string;
  icon: string;
  bonusLabel: string;
  faithMultiplier: number;
  agentMultiplier: number;
  institutionIds: string[];
}

export type CovenDef = {
  id: string;
  name: string;
  territoryId: string;
  faithPerSecond: number;
  acolyteCapacity: number;
  armyPowerPerAcolyte: number;
  unlockCost: number;
};

export interface CovenState {
  id: string;
  acolytes: number;
  armyPower: number;
}

export interface InstitutionState {
  id: string;
  status: InstitutionStatus;
  defenseRemaining: number;
  detection: number;
  assignedAgents: number;
  reconProgress: number;
  controlProgress: number;
  investigationDefense: number;
  cooldownUntil: number;
}

export interface ShadowWarState {
  faith: number;
  heat: number;
  totalAgents: number;
  deployedAgents: number;
  covens: CovenState[];
  activeCovenId: string;
  institutions: Record<string, InstitutionState>;
  lastTick: number;
  victoryAchieved: boolean;
  prestigeMultiplier: number;
  totalControlled: number;
}
