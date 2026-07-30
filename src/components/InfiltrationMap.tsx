import { Map as MapIcon } from 'lucide-react';
import type { ShadowWarState } from '@/game/types';
import { TERRITORY_DEFS, institutionsByTerritory } from '@/game/data';
import { isTerritoryControlled } from '@/game/engine';
import InstitutionCard from './InstitutionCard';

interface Props {
  state: ShadowWarState;
  onSelectInstitution: (id: string) => void;
}

export default function InfiltrationMap({ state, onSelectInstitution }: Props) {
  return (
    <div className="rounded-xl border border-amber-900/30 bg-stone-950/70 p-5 backdrop-blur-sm">
      <h2 className="mb-4 flex items-center gap-2 text-lg font-semibold text-amber-200"><MapIcon className="h-5 w-5" /> Global Infiltration Map</h2>
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
        {TERRITORY_DEFS.map((territory) => {
          const controlled = isTerritoryControlled(state, territory.id);
          const institutions = institutionsByTerritory(territory.id);
          const controlledCount = institutions.filter((i) => state.institutions[i.id]?.status === 'controlled').length;
          return (
            <div key={territory.id} className={`rounded-lg border p-3 ${controlled ? 'border-emerald-700/40 bg-emerald-950/20' : 'border-stone-700/40 bg-stone-900/40'}`}>
              <div className="mb-3 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <span className="text-xl">{territory.icon}</span>
                  <div>
                    <div className="text-sm font-semibold text-stone-100">{territory.name}</div>
                    <div className="text-[10px] text-stone-400">{controlledCount} / {institutions.length} controlled</div>
                  </div>
                </div>
                {controlled && <div className="rounded-md bg-emerald-900/40 px-2 py-1 text-[10px] font-medium text-emerald-300">BONUS ACTIVE</div>}
              </div>
              <div className="mb-3 rounded-md bg-stone-950/50 px-2 py-1.5 text-[10px] text-stone-400">Bonus: {territory.bonusLabel}</div>
              <div className="space-y-2">
                {institutions.map((def) => {
                  const inst = state.institutions[def.id];
                  if (!inst) return null;
                  return <InstitutionCard key={def.id} def={def} inst={inst} state={state} onClick={() => onSelectInstitution(def.id)} />;
                })}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
