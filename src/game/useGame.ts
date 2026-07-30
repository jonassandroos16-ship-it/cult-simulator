import { useCallback, useEffect, useRef, useState } from 'react';
import {
  createInitialState,
  loadState,
  resetState,
  saveState,
  tick,
  TICK_MS,
} from './engine';
import type { ShadowWarState } from './types';

export interface Notification {
  id: number;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
}

export function useGame() {
  const [state, setState] = useState<ShadowWarState>(() => loadState() ?? createInitialState());
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const stateRef = useRef(state);
  const notifId = useRef(0);

  stateRef.current = state;

  const notify = useCallback((message: string, type: Notification['type'] = 'info') => {
    const id = ++notifId.current;
    setNotifications((prev) => [...prev, { id, message, type }]);
    setTimeout(() => {
      setNotifications((prev) => prev.filter((n) => n.id !== id));
    }, 4000);
  }, []);

  useEffect(() => {
    const interval = setInterval(() => {
      setState((prev) => {
        const next = structuredClone(prev);
        tick(next, Date.now());
        return next;
      });
    }, TICK_MS);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    const interval = setInterval(() => {
      saveState(stateRef.current);
    }, 10000);
    return () => clearInterval(interval);
  }, []);

  useEffect(() => {
    const handler = () => saveState(stateRef.current);
    window.addEventListener('beforeunload', handler);
    return () => window.removeEventListener('beforeunload', handler);
  }, []);

  const mutate = useCallback(
    (fn: (s: ShadowWarState) => void) => {
      setState((prev) => {
        const next = structuredClone(prev);
        fn(next);
        return next;
      });
    },
    [],
  );

  const reset = useCallback(() => {
    resetState();
    setState(createInitialState());
    notify('The cult has been abandoned. A new shadow grows.', 'warning');
  }, [notify]);

  return { state, mutate, notify, notifications, reset };
}
