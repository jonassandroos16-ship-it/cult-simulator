import { useState, useEffect } from 'react';
import { X, Eye, Swords, Shield, AlertTriangle, CheckCircle2, ShieldAlert, ArrowLeftRight } from 'lucide-react';
import type { ShadowWarState } from '@/game/types';
import { INSTITUTION_MAP } from '@/game/data';
import { assignDefenders, getAgentStrength, getAvailableAgents, getDetectionMultiplier, getReconRiskMultiplier, sendInfiltrationWave, startRecon, withdrawAgents } from '@/game/engine';

interface Props {
  state: ShadowWarState;
  institutionId: string | null;
  onClose: () => void;
  mutate: (fn: (s: ShadowWarState) => void) => void;
  notify: (msg: string, type?: 'info' | 'success' | 'warning' | 'error') => void;
}

const TYPE_ICONS: Record<string, string> = { police: '🚔', media: '📡', government: '🏛️', military: '⚔️', finance: '💰', intelligence: '🕵️' };

export default function InstitutionModal({ state, institutionId, onClose, mutate, notify }: Props) {
  const [reconCount, setReconCount] = useState(2);
  const [waveSize, setWaveSize] = useState(3);
  const [defenderCount, setDefenderCount] = useState(3);

  useEffect(() => { setReconCount(2); setWaveSize(3); setDefenderCount(3); }, [institutionId]);

  if (!institutionId) return null;
  const def = INSTITUTION_MAP[institutionId];
  const inst = state.institutions[institutionId];
  if (!def || !inst) return null;

  const available = getAvailableAgents(state);
  const strength = getAgentStrength(state);
  const detMult = getDetectionMultiplier(state);
  const reconRiskMult = getReconRiskMultiplier(state);

  const handleRecon = () => { let r: { success: boolean; message: string } = { success: false, message: '' }; mutate((s) => { r = startRecon(s, institutionId, reconCount); }); notify(r.message, r.success ? 'success' : 'error'); };
  const handleWave = () => { let r: { success: boolean; message: string } = { success: false, message: '' }; mutate((s) => { r = sendInfiltrationWave(s, institutionId, waveSize); }); notify(r.message, r.success ? 'success' : 'error'); };
  const handleWithdraw = () => { let r: { success: boolean; message: string } = { success: false, message: '' }; mutate((s) => { r = withdrawAgents(s, institutionId); }); notify(r.message, r.success ? 'success' : 'error'); };
  const handleDefend = () => { let r: { success: boolean; message: string } = { success: false, message: '' }; mutate((s) => { r = assignDefenders(s, institutionId, defenderCount); }); notify(r.message, r.success ? 'success' : 'error'); };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 p-4 backdrop-blur-sm" onClick={onClose}>
      <div className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-xl border border-amber-900/40 bg-stone-950/95 p-6 shadow-2xl" onClick={(e) => e.stopPropagation()}>
        <div className="mb-4 flex items-start justify-between">
          <div className="flex items-center gap-3">
            <span className="text-3xl">{TYPE_ICONS[def.type]}</span>
            <div><h2 className="text-lg font-semibold text-stone-100">{def.name}</h2><div className="text-xs uppercase tracking-wider text-stone-400">Tier {def.tier} · {def.type}</div></div>
          </div>
          <button onClick={onClose} className="rounded-md p-1 text-stone-400 hover:bg-stone-800 hover:text-stone-200"><X className="h-5 w-5" /></button>
        </div>
        <p className="mb-4 text-sm text-stone-400">{def.description}</p>
        <div className="mb-4 grid grid-cols-2 gap-2 text-xs">
          <StatBox label="Defense" value={Math.ceil(inst.defenseRemaining).toString()} icon={<Shield className="h-3.5 w-3.5" />} />
          <StatBox label="Detection" value={`${Math.floor(inst.detection)}%`} icon={<AlertTriangle className="h-3.5 w-3.5" />} />
          <StatBox label="Agents Deployed" value={inst.assignedAgents.toString()} icon={<Swords className="h-3.5 w-3.5" />} />
          <StatBox label="Agent Strength" value={`×${strength.toFixed(2)}`} icon={<Swords className="h-3.5 w-3.5" />} />
        </div>
        <div className="mb-4 rounded-lg border border-emerald-800/30 bg-emerald-950/20 p-3"><div className="flex items-center gap-2 text-xs text-emerald-300"><CheckCircle2 className="h-4 w-4" /><span className="font-medium">Reward on Control:</span><span className="text-emerald-200">{def.reward.label}</span></div></div>
        {inst.status === 'unlocked' && (
          <div className="space-y-3"><div className="rounded-lg border border-blue-800/30 bg-blue-950/20 p-3"><div className="mb-2 flex items-center gap-2 text-sm text-blue-200"><Eye className="h-4 w-4" /> Recon Phase</div><p className="mb-3 text-xs text-stone-400">Send 1-3 agents to scout. Reveals defense and detection. Risk of losing agents: {(def.reconRisk * reconRiskMult * 100).toFixed(0)}%.</p><div className="flex items-center gap-2"><input type="range" min={1} max={3} value={reconCount} onChange={(e) => setReconCount(Number(e.target.value))} className="flex-1 accent-blue-500" /><span className="w-16 text-sm text-stone-300">{reconCount} agents</span><button onClick={handleRecon} disabled={available < reconCount} className="rounded-lg bg-blue-800/50 px-4 py-2 text-sm text-blue-100 transition-colors hover:bg-blue-700/50 disabled:cursor-not-allowed disabled:opacity-40">Start Recon</button></div></div></div>
        )}
        {(inst.status === 'recon' || inst.status === 'infiltrating') && (
          <div className="space-y-3">
            {inst.status === 'recon' && <div><div className="mb-1 flex justify-between text-xs text-stone-400"><span>Recon Progress</span><span>{Math.floor(inst.reconProgress)}%</span></div><ProgressBar value={inst.reconProgress} max={100} color="bg-blue-500" /></div>}
            <div><div className="mb-1 flex justify-between text-xs text-stone-400"><span>Defense Remaining</span><span>{Math.ceil(inst.defenseRemaining)} / {def.defense}</span></div><ProgressBar value={def.defense - inst.defenseRemaining} max={def.defense} color="bg-amber-500" /></div>
            <div><div className="mb-1 flex justify-between text-xs text-stone-400"><span>Detection Level</span><span>{Math.floor(inst.detection)}%</span></div><ProgressBar value={inst.detection} max={100} color="bg-red-500" /></div>
            <div className="rounded-lg border border-amber-800/30 bg-amber-950/20 p-3"><div className="mb-2 flex items-center gap-2 text-sm text-amber-200"><Swords className="h-4 w-4" /> Infiltration Phase</div><p className="mb-3 text-xs text-stone-400">Send waves of agents. Each wave reduces defense but raises detection. Bigger waves hit harder but spike detection. Estimated detection per agent: {(def.detectionRate * detMult).toFixed(1)}%/s</p><div className="flex items-center gap-2"><input type="range" min={1} max={Math.min(20, Math.max(1, available))} value={waveSize} onChange={(e) => setWaveSize(Number(e.target.value))} className="flex-1 accent-amber-500" /><span className="w-16 text-sm text-stone-300">{waveSize} agents</span><button onClick={handleWave} disabled={available < waveSize} className="rounded-lg bg-amber-800/50 px-4 py-2 text-sm text-amber-100 transition-colors hover:bg-amber-700/50 disabled:cursor-not-allowed disabled:opacity-40">Send Wave</button></div></div>
            <button onClick={handleWithdraw} className="flex w-full items-center justify-center gap-2 rounded-lg border border-stone-700/50 bg-stone-900/50 p-2.5 text-sm text-stone-400 transition-colors hover:border-red-700/40 hover:text-red-300"><ArrowLeftRight className="h-4 w-4" /> Withdraw Agents</button>
          </div>
        )}
        {inst.status === 'controlled' && (
          <div className="rounded-lg border border-emerald-800/30 bg-emerald-950/20 p-4 text-center"><CheckCircle2 className="mx-auto mb-2 h-8 w-8 text-emerald-400" /><div className="text-sm font-medium text-emerald-200">This institution is under your control.</div><div className="mt-1 text-xs text-emerald-400">{def.reward.label}</div></div>
        )}
        {inst.status === 'alerted' && (
          <div className="rounded-lg border border-red-800/30 bg-red-950/20 p-4 text-center"><AlertTriangle className="mx-auto mb-2 h-8 w-8 text-red-400" /><div className="text-sm font-medium text-red-200">The institution has been alerted!</div><div className="mt-1 text-xs text-red-400">Agents lost. Cooldown active — will become available again shortly.</div></div>
        )}
        {inst.status === 'investigated' && (
          <div className="space-y-3"><div className="rounded-lg border border-orange-800/30 bg-orange-950/20 p-3"><div className="mb-2 flex items-center gap-2 text-sm text-orange-200"><ShieldAlert className="h-4 w-4" /> Under Investigation!</div><p className="mb-3 text-xs text-stone-400">This institution is being investigated. Assign defenders to maintain control — if defense drops to 0, you lose it.</p><div className="mb-3"><div className="mb-1 flex justify-between text-xs text-stone-400"><span>Investigation Defense</span><span>{Math.floor(inst.investigationDefense)}%</span></div><ProgressBar value={inst.investigationDefense} max={100} color="bg-orange-500" /></div><div className="flex items-center gap-2"><input type="range" min={1} max={Math.min(20, Math.max(1, available))} value={defenderCount} onChange={(e) => setDefenderCount(Number(e.target.value))} className="flex-1 accent-orange-500" /><span className="w-16 text-sm text-stone-300">{defenderCount} agents</span><button onClick={handleDefend} disabled={available < defenderCount} className="rounded-lg bg-orange-800/50 px-4 py-2 text-sm text-orange-100 transition-colors hover:bg-orange-700/50 disabled:cursor-not-allowed disabled:opacity-40">Defend</button></div></div></div>
        )}
        {inst.status === 'locked' && (
          <div className="rounded-lg border border-stone-700/30 bg-stone-900/30 p-4 text-center"><div className="text-sm text-stone-400">Locked. Control prerequisite institutions to unlock.</div></div>
        )}
      </div>
    </div>
  );
}

function StatBox({ label, value, icon }: { label: string; value: string; icon: React.ReactNode }) {
  return <div className="flex items-center gap-2 rounded-md border border-stone-700/40 bg-stone-900/50 p-2"><span className="text-stone-400">{icon}</span><div><div className="text-[10px] uppercase tracking-wider text-stone-500">{label}</div><div className="text-sm font-medium text-stone-200">{value}</div></div></div>;
}

function ProgressBar({ value, max, color }: { value: number; max: number; color: string }) {
  const pct = Math.min(100, (value / max) * 100);
  return <div className="h-2 overflow-hidden rounded-full bg-stone-800"><div className={`h-full rounded-full transition-all duration-300 ${color}`} style={{ width: `${pct}%` }} /></div>;
}
