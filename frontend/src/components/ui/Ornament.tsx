import type {ReactNode} from 'react';
import {cn} from '../../lib/cn';

/** Art Deco bezagi: ◆ ── matn ── ◆ */
export function Ornament({children, className}: {children?: ReactNode; className?: string}) {
  return (
    <div className={cn('flex items-center justify-center gap-3 text-xs font-semibold tracking-[0.35em] uppercase', className)}>
      <span aria-hidden>◆</span>
      {children ? <span>{children}</span> : <span aria-hidden className="h-px w-16 bg-current" />}
      <span aria-hidden>◆</span>
    </div>
  );
}
