import { Globe, Shield, Users, Zap, Skull, AlertTriangle } from 'lucide-react';
import type { ShadowWarState } from '@/game/types';
import { getAgentProductionPerSecond, getAgentStrength, getAvailableAgents, getControlledInstitutions, getControlledTerritories, getDetectionMultiplier, getFaithPerSecond, getSuspicionDecay } from '@/game/engine';

interface Props {
  state: ShadowWarState;
}

function StatChip({ icon, label, value, sub, color }: { icon: React.ReactNode; label: string; value: string; sub?: string; color: string }) {
  return (
    <div className="flex items-center gap-2.5 rounded-lg border border-amber-900/30 bg-stone-950/60 px-3 py-2 backdrop-blur-sm">
      <div className={`flex h-9 w-9 items-center justify-center rounded-md ${color}`}>{icon}</div>
      <div className="leading-tight">
        <div className="text-[10px] uppercase tracking-wider text-stone-400">{label}</div>
        <div className="text-sm font-semibold text-stone-100">{value}</div>
        {sub && <div className="text-[10px] text-stone-500">{sub}</div>}
      </div>
    </div>
  );
}

export default function ResourceBar({ state }: Props) {
  const fps = getFaithPerSecond(state);
  const aps = getAgentProductionPerSecond(state);
  const available = getAvailableAgents(state);
  const strength = getAgentStrength(state);
  const detMult = getDetectionMultiplier(state);
  const suspicionDecay = getSuspicionDecay(state);
  const controlled = getControlledInstitutions(state);
  const territories = getControlledTerritories(state);

  return (
    <div className="mb-4 flex flex-wrap gap-2">
      <StatChip icon={<Zap className="h-5 w-5 text-amber-300" />} label="Faith" value={Math.floor(state.faith).toLocaleString()} sub={`+${fps.toFixed(1)}/s`} color="bg-amber-950/50" />
      <StatChip icon={<Users className="h-5 w-5 text-cyan-300" />} label="Agents" value={`${available} / ${Math.floor(state.totalAgents)}`} sub={`+${aps.toFixed(2)}/s`} color="bg-cyan-950/50" />
      <StatChip icon={<Shield className="h-5 w-5 text-emerald-300" />} label="Agent Strength" value={`×${strength.toFixed(2)}`} color="bg-emerald-950/50" />
      <StatChip icon={<AlertTriangle className="h-5 w-5 text-red-300" />} label="Heat" value={Math.floor(state.heat).toLocaleString()} sub={suspicionDecay > 0 ? `-${suspicionDecay.toFixed(1)}/s` : 'no decay'} color="bg-red-950/50" />
      <StatChip icon={<Skull className="h-5 w-5 text-purple-300" />} label="Detection" value={`×${detMult.toFixed(2)}`} sub="global modifier" color="bg-purple-950/50" />
      <StatChip icon={<Globe className="h-5 w-5 text-stone-300" />} label="Controlled" value={`${controlled.length} / ${Object.keys(state.institutions).length}`} sub={`${territories.length} territories`} color="bg-stone-800/50" />
    </div>
  );
}
