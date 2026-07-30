import type { CovenDef, InstitutionDef, TerritoryDef } from './types';

export const COVEN_DEFS: CovenDef[] = [
  { id: 'skanor', name: 'Coven of Skanör', territoryId: 'europe', faithPerSecond: 1.0, acolyteCapacity: 20, armyPowerPerAcolyte: 2, unlockCost: 0 },
  { id: 'ashen-veil', name: 'Ashen Veil', territoryId: 'europe', faithPerSecond: 1.8, acolyteCapacity: 30, armyPowerPerAcolyte: 3, unlockCost: 500 },
  { id: 'crimson-pact', name: 'Crimson Pact', territoryId: 'north-america', faithPerSecond: 2.6, acolyteCapacity: 40, armyPowerPerAcolyte: 4, unlockCost: 2000 },
  { id: 'hollow-eye', name: 'Hollow Eye', territoryId: 'south-america', faithPerSecond: 3.5, acolyteCapacity: 50, armyPowerPerAcolyte: 5, unlockCost: 6000 },
  { id: 'void-whisper', name: 'Void Whisper', territoryId: 'asia', faithPerSecond: 4.5, acolyteCapacity: 60, armyPowerPerAcolyte: 6, unlockCost: 15000 },
  { id: 'drowned-star', name: 'Drowned Star', territoryId: 'oceania', faithPerSecond: 5.5, acolyteCapacity: 70, armyPowerPerAcolyte: 7, unlockCost: 35000 },
  { id: 'pale-moon', name: 'Pale Moon', territoryId: 'africa', faithPerSecond: 6.5, acolyteCapacity: 80, armyPowerPerAcolyte: 8, unlockCost: 75000 },
  { id: 'final-temple', name: 'Final Temple', territoryId: 'middle-east', faithPerSecond: 8.0, acolyteCapacity: 100, armyPowerPerAcolyte: 10, unlockCost: 200000 },
];

export const INSTITUTION_DEFS: InstitutionDef[] = [
  { id: 'eu-local-police', name: 'Lund Police Station', territoryId: 'europe', type: 'police', tier: 1, defense: 100, detectionRate: 2.5, reconRisk: 0.05, reward: { type: 'police', label: 'Suspicion decay +0.5/s', value: 0.5 }, description: 'A small-town precinct. Easy to infiltrate, reduces global suspicion over time.' },
  { id: 'eu-regional-media', name: 'Nordic Broadcasting Corp', territoryId: 'europe', type: 'media', tier: 2, defense: 280, detectionRate: 3, reconRisk: 0.08, reward: { type: 'media', label: 'Global detection rate -10%', value: 0.1 }, prerequisites: ['eu-local-police'], description: 'Regional news network. Controlling it suppresses detection worldwide.' },
  { id: 'eu-eu-government', name: 'European Parliament', territoryId: 'europe', type: 'government', tier: 3, defense: 600, detectionRate: 4, reconRisk: 0.12, reward: { type: 'government', label: 'Agent recruitment rate +25%', value: 0.25 }, prerequisites: ['eu-regional-media'], description: 'The heart of European governance. Massively boosts agent recruitment.' },
  { id: 'na-city-police', name: 'NYPD Intelligence Bureau', territoryId: 'north-america', type: 'police', tier: 1, defense: 140, detectionRate: 2.8, reconRisk: 0.06, reward: { type: 'police', label: 'Suspicion decay +0.8/s', value: 0.8 }, description: 'A major-city police force with deep surveillance resources.' },
  { id: 'na-cable-news', name: 'Continental News Network', territoryId: 'north-america', type: 'media', tier: 2, defense: 350, detectionRate: 3.2, reconRisk: 0.09, reward: { type: 'media', label: 'Global detection rate -12%', value: 0.12 }, prerequisites: ['na-city-police'], description: 'A coast-to-coast cable news empire. Shapes what the public believes.' },
  { id: 'na-federal-gov', name: 'Federal Government', territoryId: 'north-america', type: 'government', tier: 3, defense: 800, detectionRate: 4.5, reconRisk: 0.15, reward: { type: 'government', label: 'Agent recruitment rate +30%', value: 0.3 }, prerequisites: ['na-cable-news'], description: 'The executive branch of a superpower. The ultimate prize.' },
  { id: 'na-military-command', name: 'Northern Command', territoryId: 'north-america', type: 'military', tier: 3, defense: 900, detectionRate: 5, reconRisk: 0.18, reward: { type: 'military', label: 'Agent combat strength +40%', value: 0.4 }, prerequisites: ['na-federal-gov'], description: 'A unified military command. Turns your agents into a lethal force.' },
  { id: 'sa-border-police', name: 'Border Constabulary', territoryId: 'south-america', type: 'police', tier: 1, defense: 110, detectionRate: 2.3, reconRisk: 0.05, reward: { type: 'police', label: 'Suspicion decay +0.6/s', value: 0.6 }, description: 'A border patrol force. Corruptible and lightly defended.' },
  { id: 'sa-state-media', name: 'National Broadcaster', territoryId: 'south-america', type: 'media', tier: 2, defense: 260, detectionRate: 2.8, reconRisk: 0.07, reward: { type: 'media', label: 'Global detection rate -8%', value: 0.08 }, prerequisites: ['sa-border-police'], description: 'State-run media. A mouthpiece waiting for a new master.' },
  { id: 'sa-intelligence', name: 'National Intelligence Service', territoryId: 'south-america', type: 'intelligence', tier: 3, defense: 550, detectionRate: 4.2, reconRisk: 0.14, reward: { type: 'intelligence', label: 'Recon risk -30% globally', value: 0.3 }, prerequisites: ['sa-state-media'], description: 'A shadowy intelligence apparatus. Reduces recon losses everywhere.' },
  { id: 'as-metro-police', name: 'Metropolitan Police Bureau', territoryId: 'asia', type: 'police', tier: 1, defense: 160, detectionRate: 2.6, reconRisk: 0.06, reward: { type: 'police', label: 'Suspicion decay +0.7/s', value: 0.7 }, description: 'A massive metropolitan police force in a dense urban sprawl.' },
  { id: 'as-tech-media', name: 'Digital Media Conglomerate', territoryId: 'asia', type: 'media', tier: 2, defense: 400, detectionRate: 3.5, reconRisk: 0.1, reward: { type: 'media', label: 'Global detection rate -15%', value: 0.15 }, prerequisites: ['as-metro-police'], description: 'A tech-driven media empire reaching billions. Powerful detection suppression.' },
  { id: 'as-central-gov', name: 'Central Committee', territoryId: 'asia', type: 'government', tier: 3, defense: 850, detectionRate: 4.8, reconRisk: 0.16, reward: { type: 'government', label: 'Agent recruitment rate +35%', value: 0.35 }, prerequisites: ['as-tech-media'], description: 'A centralized single-party government. Enormous recruitment boost.' },
  { id: 'as-military-region', name: 'Eastern Military Region', territoryId: 'asia', type: 'military', tier: 3, defense: 1000, detectionRate: 5.5, reconRisk: 0.2, reward: { type: 'military', label: 'Agent combat strength +50%', value: 0.5 }, prerequisites: ['as-central-gov'], description: 'The largest standing military force on earth. Unmatched combat power.' },
  { id: 'oc-federal-police', name: 'Federal Police Service', territoryId: 'oceania', type: 'police', tier: 1, defense: 120, detectionRate: 2.4, reconRisk: 0.05, reward: { type: 'police', label: 'Suspicion decay +0.5/s', value: 0.5 }, description: 'A continent-wide federal police agency.' },
  { id: 'oc-media-network', name: 'Southern Media Group', territoryId: 'oceania', type: 'media', tier: 2, defense: 300, detectionRate: 3, reconRisk: 0.08, reward: { type: 'media', label: 'Global detection rate -10%', value: 0.1 }, prerequisites: ['oc-federal-police'], description: 'A media network spanning the southern hemisphere.' },
  { id: 'oc-finance-hub', name: 'Pacific Financial Hub', territoryId: 'oceania', type: 'finance', tier: 3, defense: 650, detectionRate: 4, reconRisk: 0.12, reward: { type: 'finance', label: 'Faith production +20% globally', value: 0.2 }, prerequisites: ['oc-media-network'], description: 'A global financial center. Channels wealth into your cult.' },
  { id: 'af-constabulary', name: 'Colonial Constabulary', territoryId: 'africa', type: 'police', tier: 1, defense: 100, detectionRate: 2.2, reconRisk: 0.04, reward: { type: 'police', label: 'Suspicion decay +0.5/s', value: 0.5 }, description: 'A legacy colonial-era police force. Lightly defended.' },
  { id: 'af-regional-media', name: 'Continental Radio Network', territoryId: 'africa', type: 'media', tier: 2, defense: 240, detectionRate: 2.7, reconRisk: 0.07, reward: { type: 'media', label: 'Global detection rate -8%', value: 0.08 }, prerequisites: ['af-constabulary'], description: 'A radio network reaching the entire continent.' },
  { id: 'af-union-gov', name: 'Continental Union Assembly', territoryId: 'africa', type: 'government', tier: 3, defense: 580, detectionRate: 4, reconRisk: 0.13, reward: { type: 'government', label: 'Agent recruitment rate +25%', value: 0.25 }, prerequisites: ['af-regional-media'], description: 'A pan-continental political body. Massive recruitment potential.' },
  { id: 'me-secret-police', name: 'Secret Police Directorate', territoryId: 'middle-east', type: 'police', tier: 1, defense: 150, detectionRate: 2.7, reconRisk: 0.07, reward: { type: 'police', label: 'Suspicion decay +0.9/s', value: 0.9 }, description: 'A feared secret police apparatus. High reward, moderate risk.' },
  { id: 'me-state-media', name: 'State Media Authority', territoryId: 'middle-east', type: 'media', tier: 2, defense: 320, detectionRate: 3.2, reconRisk: 0.09, reward: { type: 'media', label: 'Global detection rate -12%', value: 0.12 }, prerequisites: ['me-secret-police'], description: 'A state-controlled media authority. Powerful narrative control.' },
  { id: 'me-central-command', name: 'Central Military Command', territoryId: 'middle-east', type: 'military', tier: 3, defense: 750, detectionRate: 5, reconRisk: 0.17, reward: { type: 'military', label: 'Agent combat strength +35%', value: 0.35 }, prerequisites: ['me-state-media'], description: 'A strategically critical military command. Elite agent training.' },
  { id: 'me-intelligence-bureau', name: 'Global Intelligence Bureau', territoryId: 'middle-east', type: 'intelligence', tier: 3, defense: 700, detectionRate: 4.5, reconRisk: 0.15, reward: { type: 'intelligence', label: 'Recon risk -25% globally', value: 0.25 }, prerequisites: ['me-central-command'], description: 'A cross-border intelligence network. Reduces recon losses everywhere.' },
];

export const TERRITORY_DEFS: TerritoryDef[] = [
  { id: 'europe', name: 'Europe', icon: '🏰', bonusLabel: 'Doubles faith from European covens', faithMultiplier: 2, agentMultiplier: 1.2, institutionIds: ['eu-local-police', 'eu-regional-media', 'eu-eu-government'] },
  { id: 'north-america', name: 'North America', icon: '🗽', bonusLabel: 'Doubles faith from North American covens', faithMultiplier: 2, agentMultiplier: 1.3, institutionIds: ['na-city-police', 'na-cable-news', 'na-federal-gov', 'na-military-command'] },
  { id: 'south-america', name: 'South America', icon: '🌴', bonusLabel: 'Doubles faith from South American covens', faithMultiplier: 2, agentMultiplier: 1.15, institutionIds: ['sa-border-police', 'sa-state-media', 'sa-intelligence'] },
  { id: 'asia', name: 'Asia', icon: '🏯', bonusLabel: 'Doubles faith from Asian covens', faithMultiplier: 2, agentMultiplier: 1.4, institutionIds: ['as-metro-police', 'as-tech-media', 'as-central-gov', 'as-military-region'] },
  { id: 'oceania', name: 'Oceania', icon: '🦘', bonusLabel: 'Doubles faith from Oceanian covens', faithMultiplier: 2, agentMultiplier: 1.1, institutionIds: ['oc-federal-police', 'oc-media-network', 'oc-finance-hub'] },
  { id: 'africa', name: 'Africa', icon: '🌍', bonusLabel: 'Doubles faith from African covens', faithMultiplier: 2, agentMultiplier: 1.15, institutionIds: ['af-constabulary', 'af-regional-media', 'af-union-gov'] },
  { id: 'middle-east', name: 'Middle East', icon: '🕌', bonusLabel: 'Doubles faith from Middle Eastern covens', faithMultiplier: 2, agentMultiplier: 1.2, institutionIds: ['me-secret-police', 'me-state-media', 'me-central-command', 'me-intelligence-bureau'] },
];

export const INSTITUTION_MAP: Record<string, InstitutionDef> = Object.fromEntries(
  INSTITUTION_DEFS.map((i) => [i.id, i]),
);

export const TERRITORY_MAP: Record<string, TerritoryDef> = Object.fromEntries(
  TERRITORY_DEFS.map((t) => [t.id, t]),
);

export const COVEN_MAP: Record<string, CovenDef> = Object.fromEntries(
  COVEN_DEFS.map((c) => [c.id, c]),
);

export function institutionsByTerritory(territoryId: string): InstitutionDef[] {
  return INSTITUTION_DEFS.filter((i) => i.territoryId === territoryId);
}

export function covensByTerritory(territoryId: string): CovenDef[] {
  return COVEN_DEFS.filter((c) => c.territoryId === territoryId);
}
