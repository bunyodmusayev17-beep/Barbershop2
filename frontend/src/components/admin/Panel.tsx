import type {ReactNode} from 'react';
import {cn} from '../../lib/cn';

export function Panel({children, className, title}: {children: ReactNode; className?: string; title?: string}) {
  return (
    <section className={cn('rounded-2xl border-2 border-navy/15 bg-white p-4 shadow-sm md:p-5', className)}>
      {title ? (
        <h2 className="mb-4 flex items-center gap-2 text-sm font-bold tracking-[0.2em] uppercase text-navy/70">
          <span aria-hidden className="h-4 w-1.5 rounded-full bg-gold" />
          {title}
        </h2>
      ) : null}
      {children}
    </section>
  );
}
