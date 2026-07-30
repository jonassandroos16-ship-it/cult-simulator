import { Eye, Shield, AlertTriangle, Lock, CheckCircle2, Swords, ShieldAlert } from 'lucide-react';
import type { InstitutionDef, InstitutionState, InstitutionStatus, ShadowWarState } from '@/game/types';

interface Props {
  def: InstitutionDef;
  inst: InstitutionState;
  state: ShadowWarState;
  onClick: () => void;
}

const STATUS_CONFIG: Record<InstitutionStatus, { label: string; color: string; icon: React.ReactNode }> = {
  locked: { label: 'Locked', color: 'text-stone-500 border-stone-700/30 bg-stone-900/30', icon: <Lock className="h-3.5 w-3.5" /> },
  unlocked: { label: 'Available', color: 'text-stone-300 border-stone-600/40 bg-stone-900/50', icon: <Eye className="h-3.5 w-3.5" /> },
  recon: { label: 'Recon', color: 'text-blue-300 border-blue-700/40 bg-blue-950/30', icon: <Eye className="h-3.5 w-3.5" /> },
  infiltrating: { label: 'Infiltrating', color: 'text-amber-300 border-amber-700/40 bg-amber-950/30', icon: <Swords className="h-3.5 w-3.5" /> },
  controlled: { label: 'Controlled', color: 'text-emerald-300 border-emerald-700/40 bg-emerald-950/30', icon: <CheckCircle2 className="h-3.5 w-3.5" /> },
  alerted: { label: 'Alerted', color: 'text-red-300 border-red-700/40 bg-red-950/30', icon: <AlertTriangle className="h-3.5 w-3.5" /> },
  investigated: { label: 'Under Investigation', color: 'text-orange-300 border-orange-700/40 bg-orange-950/30', icon: <ShieldAlert className="h-3.5 w-3.5" /> },
};

const TYPE_ICONS: Record<string, string> = { police: '🚔', media: '📡', government: '🏛️', military: '⚔️', finance: '💰', intelligence: '🕵️' };

export default function InstitutionCard({ def, inst, state, onClick }: Props) {
  const config = STATUS_CONFIG[inst.status];
  const isLocked = inst.status === 'locked';
  return (
    <button onClick={onClick} disabled={isLocked} className={`group relative w-full overflow-hidden rounded-lg border p-3 text-left transition-all ${config.color} ${isLocked ? 'cursor-not-allowed opacity-50' : 'hover:scale-[1.02] hover:shadow-lg'}`}>
      <div className="mb-2 flex items-start justify-between">
        <div className="flex items-center gap-2">
          <span className="text-lg">{TYPE_ICONS[def.type]}</span>
          <div>
            <div className="text-sm font-medium text-stone-100">{def.name}</div>
            <div className="flex items-center gap-1 text-[10px] uppercase tracking-wider opacity-70">{config.icon}{config.label}</div>
          </div>
        </div>
        <div className="text-[10px] font-medium uppercase tracking-wider text-stone-400">Tier {def.tier}</div>
      </div>
      {inst.status === 'recon' && (
        <div className="mb-1.5"><div className="mb-0.5 flex justify-between text-[10px] text-stone-400"><span>Recon Progress</span><span>{Math.floor(inst.reconProgress)}%</span></div><ProgressBar value={inst.reconProgress} max={100} color="bg-blue-500" /></div>
      )}
      {(inst.status === 'infiltrating' || inst.status === 'recon') && (
        <>
          <div className="mb-1.5"><div className="mb-0.5 flex justify-between text-[10px] text-stone-400"><span>Defense</span><span>{Math.ceil(inst.defenseRemaining)}</span></div><ProgressBar value={def.defense - inst.defenseRemaining} max={def.defense} color="bg-amber-500" /></div>
          <div className="mb-1.5"><div className="mb-0.5 flex justify-between text-[10px] text-stone-400"><span>Detection</span><span>{Math.floor(inst.detection)}%</span></div><ProgressBar value={inst.detection} max={100} color="bg-red-500" /></div>
        </>
      )}
      {inst.status === 'investigated' && (
        <div className="mb-1.5"><div className="mb-0.5 flex justify-between text-[10px] text-stone-400"><span>Investigation Defense</span><span>{Math.floor(inst.investigationDefense)}%</span></div><ProgressBar value={inst.investigationDefense} max={100} color="bg-orange-500" /></div>
      )}
      <div className="mt-2 flex items-center justify-between text-[10px] text-stone-400">
        <span className="flex items-center gap-1"><Swords className="h-3 w-3" />{inst.assignedAgents} agents</span>
        {inst.status === 'controlled' && <span className="font-medium text-emerald-300">{def.reward.label}</span>}
        {inst.status === 'unlocked' && <span className="text-stone-500">Click to infiltrate</span>}
      </div>
    </button>
  );
}

function ProgressBar({ value, max, color }: { value: number; max: number; color: string }) {
  const pct = Math.min(100, (value / max) * 100);
  return <div className="h-1.5 overflow-hidden rounded-full bg-stone-800"><div className={`h-full rounded-full transition-all duration-300 ${color}`} style={{ width: `${pct}%` }} /></div>;
}
