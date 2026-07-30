import { useState } from 'react';
import { Users, Plus, Swords, ChevronRight, Home } from 'lucide-react';
import type { ShadowWarState } from '@/game/types';
import { COVEN_DEFS, COVEN_MAP, TERRITORY_MAP } from '@/game/data';
import { foundCoven, getFoundCovenCost, getNextCovenToUnlock, recruitAcolytes, trainAgents } from '@/game/engine';

interface Props {
  state: ShadowWarState;
  mutate: (fn: (s: ShadowWarState) => void) => void;
  notify: (msg: string, type?: 'info' | 'success' | 'warning' | 'error') => void;
}

export default function CovenPanel({ state, mutate, notify }: Props) {
  const [recruitCount, setRecruitCount] = useState(1);
  const [trainCount, setTrainCount] = useState(1);
  const activeCoven = state.covens.find((c) => c.id === state.activeCovenId);
  const activeDef = activeCoven ? COVEN_MAP[activeCoven.id] : undefined;
  const nextCoven = getNextCovenToUnlock(state);
  const foundCost = getFoundCovenCost(state);

  const handleFound = () => {
    let result: { success: boolean; message: string } = { success: false, message: '' };
    mutate((s) => { result = foundCoven(s); });
    notify(result.message, result.success ? 'success' : 'error');
  };
  const handleRecruit = () => {
    let result: { success: boolean; message: string } = { success: false, message: '' };
    mutate((s) => { result = recruitAcolytes(s, recruitCount); });
    notify(result.message, result.success ? 'success' : 'error');
  };
  const handleTrain = () => {
    let result: { success: boolean; message: string } = { success: false, message: '' };
    mutate((s) => { result = trainAgents(s, trainCount); });
    notify(result.message, result.success ? 'success' : 'error');
  };
  const setActive = (id: string) => { mutate((s) => { s.activeCovenId = id; }); };

  return (
    <div className="rounded-xl border border-amber-900/30 bg-stone-950/70 p-5 backdrop-blur-sm">
      <h2 className="mb-4 flex items-center gap-2 text-lg font-semibold text-amber-200"><Home className="h-5 w-5" /> Covens</h2>
      <div className="mb-4 space-y-2">
        {state.covens.map((cs) => {
          const def = COVEN_MAP[cs.id];
          if (!def) return null;
          const territory = TERRITORY_MAP[def.territoryId];
          const isActive = cs.id === state.activeCovenId;
          return (
            <button key={cs.id} onClick={() => setActive(cs.id)} className={`w-full rounded-lg border p-3 text-left transition-all ${isActive ? 'border-amber-600/50 bg-amber-950/30' : 'border-stone-700/40 bg-stone-900/40 hover:border-stone-600/50'}`}>
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-sm font-medium text-stone-100">{def.name}</div>
                  <div className="text-xs text-stone-400">{territory?.icon} {territory?.name} · {cs.acolytes} acolytes · {cs.armyPower} army power</div>
                </div>
                {isActive && <ChevronRight className="h-4 w-4 text-amber-400" />}
              </div>
            </button>
          );
        })}
      </div>
      {nextCoven && (
        <button onClick={handleFound} disabled={state.faith < foundCost} className="mb-4 flex w-full items-center justify-center gap-2 rounded-lg border border-stone-700/50 bg-stone-900/50 p-3 text-sm text-stone-300 transition-all hover:border-amber-600/40 hover:text-amber-200 disabled:cursor-not-allowed disabled:opacity-40">
          <Plus className="h-4 w-4" /> Found {nextCoven.name} — {Math.ceil(foundCost).toLocaleString()} faith
        </button>
      )}
      {activeCoven && activeDef && (
        <div className="space-y-3 border-t border-stone-700/40 pt-4">
          <div className="text-xs uppercase tracking-wider text-stone-500">{activeDef.name} · {activeDef.acolyteCapacity} max acolytes</div>
          <div className="flex items-center gap-2">
            <input type="range" min={1} max={Math.min(20, activeDef.acolyteCapacity - activeCoven.acolytes)} value={recruitCount} onChange={(e) => setRecruitCount(Number(e.target.value))} className="flex-1 accent-amber-500" />
            <span className="w-20 text-right text-sm text-stone-300">{recruitCount} acolytes</span>
            <button onClick={handleRecruit} className="flex items-center gap-1.5 rounded-lg bg-amber-800/40 px-3 py-2 text-sm text-amber-100 transition-colors hover:bg-amber-700/50"><Users className="h-4 w-4" /> Recruit</button>
          </div>
          <div className="text-right text-xs text-stone-500">Cost: {recruitCount * 10} faith</div>
          <div className="flex items-center gap-2">
            <input type="range" min={1} max={Math.min(10, activeCoven.acolytes)} value={trainCount} onChange={(e) => setTrainCount(Number(e.target.value))} className="flex-1 accent-cyan-500" />
            <span className="w-20 text-right text-sm text-stone-300">{trainCount} acolytes</span>
            <button onClick={handleTrain} className="flex items-center gap-1.5 rounded-lg bg-cyan-800/40 px-3 py-2 text-sm text-cyan-100 transition-colors hover:bg-cyan-700/50"><Swords className="h-4 w-4" /> Train</button>
          </div>
          <div className="text-right text-xs text-stone-500">Converts acolytes into sleeper agents</div>
        </div>
      )}
      {COVEN_DEFS.filter((c) => !state.covens.some((cs) => cs.id === c.id)).length > 0 && !nextCoven && (
        <div className="text-center text-xs text-stone-500">All covens founded.</div>
      )}
    </div>
  );
}
