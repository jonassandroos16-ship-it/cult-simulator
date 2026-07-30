import { useState } from 'react';
import { Home, Map, Swords, Trophy, RotateCcw } from 'lucide-react';
import { useGame } from '@/game/useGame';
import ResourceBar from '@/components/ResourceBar';
import CovenPanel from '@/components/CovenPanel';
import InfiltrationMap from '@/components/InfiltrationMap';
import InstitutionModal from '@/components/InstitutionModal';
import VictoryScreen from '@/components/VictoryScreen';
import NotificationToast from '@/components/NotificationToast';

type Tab = 'coven' | 'shadow-war';

function App() {
  const { state, mutate, notify, notifications, reset } = useGame();
  const [activeTab, setActiveTab] = useState<Tab>('coven');
  const [selectedInstitution, setSelectedInstitution] = useState<string | null>(null);

  return (
    <div className="min-h-screen bg-gradient-to-b from-stone-950 via-stone-900 to-black text-stone-200">
      <header className="sticky top-0 z-30 border-b border-amber-900/20 bg-stone-950/80 backdrop-blur-md">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
          <div className="flex items-center gap-2">
            <span className="text-2xl">🕯️</span>
            <div>
              <h1 className="text-base font-bold tracking-tight text-amber-200">The Shadow War</h1>
              <p className="text-[10px] uppercase tracking-widest text-stone-500">Cult Simulator · End Game</p>
            </div>
          </div>
          <button
            onClick={reset}
            className="flex items-center gap-1.5 rounded-md border border-stone-700/40 px-3 py-1.5 text-xs text-stone-400 transition-colors hover:border-red-700/40 hover:text-red-300"
          >
            <RotateCcw className="h-3.5 w-3.5" /> Abandon
          </button>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-4 py-6">
        <ResourceBar state={state} />

        <div className="mb-4 flex gap-2">
          <TabButton active={activeTab === 'coven'} onClick={() => setActiveTab('coven')} icon={<Home className="h-4 w-4" />} label="Covens" />
          <TabButton active={activeTab === 'shadow-war'} onClick={() => setActiveTab('shadow-war')} icon={<Map className="h-4 w-4" />} label="Shadow War" />
        </div>

        {activeTab === 'coven' && (
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
            <CovenPanel state={state} mutate={mutate} notify={notify} />
            <div className="rounded-xl border border-amber-900/30 bg-stone-950/70 p-5 backdrop-blur-sm">
              <h2 className="mb-3 flex items-center gap-2 text-lg font-semibold text-amber-200">
                <Swords className="h-5 w-5" /> How the Shadow War Works
              </h2>
              <div className="space-y-3 text-sm text-stone-400">
                <p>Your cult doesn't fight openly — it <span className="text-amber-300">seeps</span> into society. Train acolytes into <span className="text-cyan-300">sleeper agents</span>, then deploy them to infiltrate real-world institutions.</p>
                <p><span className="text-stone-200">Recon</span> an institution first to scout its defenses. Then send <span className="text-stone-200">waves of agents</span> to reduce its defense — but watch the <span className="text-red-300">detection meter</span>. If it hits 100% before you take control, you lose your agents and gain heat.</p>
                <p>Each institution type grants a different permanent bonus:</p>
                <ul className="ml-4 space-y-1 text-xs">
                  <li>🚔 <span className="text-stone-300">Police</span> — reduces heat over time</li>
                  <li>📡 <span className="text-stone-300">Media</span> — lowers global detection rate</li>
                  <li>🏛️ <span className="text-stone-300">Government</span> — boosts agent recruitment</li>
                  <li>⚔️ <span className="text-stone-300">Military</span> — increases agent combat strength</li>
                  <li>💰 <span className="text-stone-300">Finance</span> — boosts faith production</li>
                  <li>🕵️ <span className="text-stone-300">Intelligence</span> — reduces recon losses</li>
                </ul>
                <p>Control all institutions in a territory for a <span className="text-emerald-300">territory bonus</span>. Control all territories to trigger the endgame: <span className="text-amber-300">"The World Is Ours"</span>.</p>
                <p className="text-xs text-stone-500">Start by founding covens and recruiting acolytes here. Then switch to the Shadow War tab to deploy agents.</p>
              </div>
            </div>
          </div>
        )}

        {activeTab === 'shadow-war' && (
          <InfiltrationMap state={state} onSelectInstitution={setSelectedInstitution} />
        )}

        <div className="mt-6 rounded-xl border border-amber-900/20 bg-stone-950/50 p-4">
          <div className="mb-2 flex items-center gap-2 text-xs uppercase tracking-wider text-stone-400">
            <Trophy className="h-4 w-4 text-amber-400" /> Path to Global Domination
          </div>
          <div className="flex flex-wrap gap-2">
            {Object.entries(state.institutions).map(([id, inst]) => {
              const isControlled = inst.status === 'controlled';
              const isInvestigated = inst.status === 'investigated';
              return (
                <div
                  key={id}
                  className={`h-3 w-3 rounded-full transition-colors ${
                    isControlled ? 'bg-emerald-500' : isInvestigated ? 'bg-orange-500' : 'bg-stone-700'
                  }`
                  title={id}
                />
              );
            })}
          </div>
          <div className="mt-2 text-xs text-stone-500">
            {state.totalControlled} / {Object.keys(state.institutions).length} institutions controlled
          </div>
        </div>
      </main>

      <InstitutionModal state={state} institutionId={selectedInstitution} onClose={() => setSelectedInstitution(null)} mutate={mutate} notify={notify} />
      <NotificationToast notifications={notifications} />
      {state.victoryAchieved && <VictoryScreen state={state} onReset={reset} />}
    </div>
  );
}

function TabButton({ active, onClick, icon, label }: { active: boolean; onClick: () => void; icon: React.ReactNode; label: string }) {
  return (
    <button
      onClick={onClick}
      className={`flex items-center gap-2 rounded-lg border px-4 py-2 text-sm font-medium transition-all ${
        active ? 'border-amber-600/50 bg-amber-950/30 text-amber-200' : 'border-stone-700/40 bg-stone-900/40 text-stone-400 hover:border-stone-600/50 hover:text-stone-200'
      }`}
    >
      {icon}
      {label}
    </button>
  );
}

export default App;
