import {useCallback, useState, type ReactNode} from 'react';
import {cn} from '../lib/cn';
import {ToastContext, type ToastKind} from './ToastContext';

type Toast = {id: number; text: string; kind: ToastKind};

/** Pastki o'ng burchakda 3.5 soniya turadigan bildirishnomalar (admin panel). */
export function ToastProvider({children}: {children: ReactNode}) {
  const [items, setItems] = useState<Toast[]>([]);
  const push = useCallback((text: string, kind: ToastKind = 'ok') => {
    const id = Date.now() + Math.random();
    setItems((x) => [...x, {id, text, kind}]);
    setTimeout(() => setItems((x) => x.filter((t) => t.id !== id)), 3500);
  }, []);
  return (
    <ToastContext.Provider value={push}>
      {children}
      <div className="pointer-events-none fixed right-4 bottom-20 z-[60] lg:bottom-4 flex flex-col gap-2" aria-live="polite">
        {items.map((t) => (
          <div key={t.id} className={cn('rounded-xl px-4 py-3 text-sm font-semibold shadow-lg', t.kind === 'ok' ? 'bg-navy text-cream' : 'bg-cherry text-white')}>
            {t.text}
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}
