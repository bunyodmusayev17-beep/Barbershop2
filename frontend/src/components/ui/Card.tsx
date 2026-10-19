import type {ReactNode} from 'react';
import {cn} from '../../lib/cn';

export function Card({children, className, active}: {children: ReactNode; className?: string; active?: boolean}) {
  return (
    <div
      className={cn(
        'rounded-2xl border-2 bg-paper/60 transition',
        active ? 'border-cherry bg-cherry/5 shadow-[0_0_0_3px_color-mix(in_srgb,var(--color-cherry)_25%,transparent)]' : 'border-navy',
        className,
      )}
    >
      {children}
    </div>
  );
}
