import type {ReactNode} from 'react';

export function PageHeader({title, sub, actions}: {title: string; sub?: string; actions?: ReactNode}) {
  return (
    <div className="mb-5 flex flex-wrap items-end justify-between gap-3 md:mb-6 md:gap-4">
      <div>
        <h1 className="font-deco text-3xl text-navy md:text-5xl">{title}</h1>
        <span aria-hidden className="oak mt-2 block h-1.5 w-16 rounded-full" />
        {sub ? <p className="mt-1 text-sm opacity-70">{sub}</p> : null}
      </div>
      {actions ? <div className="flex flex-wrap gap-2">{actions}</div> : null}
    </div>
  );
}
