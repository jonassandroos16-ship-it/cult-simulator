import { Globe, Sparkles, RotateCcw } from 'lucide-react';
import type { ShadowWarState } from '@/game/types';
import { getControlledInstitutions } from '@/game/engine';
import { TERRITORY_DEFS } from '@/game/data';

interface Props {
  state: ShadowWarState;
  onReset: () => void;
}

export default function VictoryScreen({ state, onReset }: Props) {
  const controlled = getControlledInstitutions(state);
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/90 p-4 backdrop-blur-md">
      <div className="relative max-w-lg text-center">
        <div className="absolute inset-0 -z-10 animate-pulse rounded-full bg-amber-500/10 blur-3xl" />
        <div className="mb-6 flex justify-center">
          <div className="relative">
            <Globe className="h-24 w-24 text-amber-400" />
            <Sparkles className="absolute -right-2 -top-2 h-8 w-8 text-amber-300 animate-pulse" />
          </div>
        </div>
        <h1 className="mb-3 text-4xl font-bold tracking-tight text-amber-200">The World Is Ours</h1>
        <p className="mb-6 text-stone-400">From a single coven to total global dominion. Every institution, every government, every military command — all bow to the shadow. The world doesn't even know it's been conquered.</p>
        <div className="mb-6 grid grid-cols-2 gap-3">
          <div className="rounded-lg border border-amber-800/30 bg-amber-950/20 p-4"><div className="text-2xl font-bold text-amber-300">{controlled.length}</div><div className="text-xs uppercase tracking-wider text-stone-400">Institutions</div></div>
          <div className="rounded-lg border border-amber-800/30 bg-amber-950/20 p-4"><div className="text-2xl font-bold text-amber-300">{TERRITORY_DEFS.length}</div><div className="text-xs uppercase tracking-wider text-stone-400">Territories</div></div>
          <div className="rounded-lg border border-amber-800/30 bg-amber-950/20 p-4"><div className="text-2xl font-bold text-amber-300">×{state.prestigeMultiplier}</div><div className="text-xs uppercase tracking-wider text-stone-400">Prestige Multiplier</div></div>
          <div className="rounded-lg border border-amber-800/30 bg-amber-950/20 p-4"><div className="text-2xl font-bold text-amber-300">{state.covens.length}</div><div className="text-xs uppercase tracking-wider text-stone-400">Covens</div></div>
        </div>
        <p className="mb-6 text-sm text-stone-500">A permanent prestige multiplier of ×{state.prestigeMultiplier} has been achieved. The shadow reigns supreme.</p>
        <button onClick={onReset} className="inline-flex items-center gap-2 rounded-lg border border-amber-700/40 bg-amber-900/30 px-6 py-3 text-sm font-medium text-amber-200 transition-colors hover:bg-amber-800/40">
          <RotateCcw className="h-4 w-4" /> Begin Anew
        </button>
      </div>
    </div>
  );
}
