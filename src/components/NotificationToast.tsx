import { CheckCircle2, AlertTriangle, Info, XCircle } from 'lucide-react';
import type { Notification } from '@/game/useGame';

interface Props {
  notifications: Notification[];
}

const ICONS = {
  info: <Info className="h-4 w-4 text-blue-400" />,
  success: <CheckCircle2 className="h-4 w-4 text-emerald-400" />,
  warning: <AlertTriangle className="h-4 w-4 text-amber-400" />,
  error: <XCircle className="h-4 w-4 text-red-400" />,
};

const COLORS = {
  info: 'border-blue-700/40 bg-blue-950/40',
  success: 'border-emerald-700/40 bg-emerald-950/40',
  warning: 'border-amber-700/40 bg-amber-950/40',
  error: 'border-red-700/40 bg-red-950/40',
};

export default function NotificationToast({ notifications }: Props) {
  return (
    <div className="pointer-events-none fixed bottom-4 right-4 z-50 flex flex-col gap-2">
      {notifications.map((n) => (
        <div key={n.id} className={`flex items-center gap-2 rounded-lg border px-4 py-2.5 text-sm text-stone-200 shadow-lg backdrop-blur-sm ${COLORS[n.type]}`}>
          {ICONS[n.type]}
          {n.message}
        </div>
      ))}
    </div>
  );
}
