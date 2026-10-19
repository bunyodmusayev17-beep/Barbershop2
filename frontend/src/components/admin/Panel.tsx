import type {ReactNode} from 'react';
import {cn} from '../../lib/cn';

export function Panel({children, className, title}: {children: ReactNode; className?: string; title?: string}) {
  return (
    <section className={cn('rounded-2xl border-2 border-navy/15 bg-white p-5 shadow-sm', className)}>
      {title ? <h2 className="mb-4 text-sm font-bold tracking-[0.2em] uppercase text-navy/70">{title}</h2> : null}
      {children}
    </section>
  );
}
