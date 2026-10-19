import {useEffect, type ReactNode} from 'react';
import {cn} from '../../lib/cn';

/** Telefonda pastdan chiqadigan, kompyuterda markazda turadigan oyna. Esc yoki fon bosilsa yopiladi. */
export function Modal({open, title, onClose, children, wide}: {open: boolean; title: string; onClose: () => void; children: ReactNode; wide?: boolean}) {
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => e.key === 'Escape' && onClose();
    window.addEventListener('keydown', onKey);
    document.body.style.overflow = 'hidden';
    return () => {
      window.removeEventListener('keydown', onKey);
      document.body.style.overflow = '';
    };
  }, [open, onClose]);
  if (!open) return null;
  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-ink/50 p-0 sm:items-center sm:p-4" onMouseDown={(e) => e.target === e.currentTarget && onClose()}>
      <div role="dialog" aria-modal aria-label={title} className={cn('max-h-[92vh] w-full overflow-y-auto rounded-t-3xl bg-cream shadow-2xl sm:rounded-3xl', wide ? 'sm:max-w-3xl' : 'sm:max-w-lg')}>
        <div className="barber-pole !h-2 rounded-t-3xl" />
        <div className="flex items-center justify-between gap-4 px-6 pt-5">
          <h2 className="font-deco text-2xl text-navy md:text-3xl">{title}</h2>
          <button type="button" onClick={onClose} aria-label="Yopish" className="flex h-9 w-9 items-center justify-center rounded-full border-2 border-navy/30 hover:border-navy">
            ✕
          </button>
        </div>
        <div className="px-6 pt-4 pb-6">{children}</div>
      </div>
    </div>
  );
}
