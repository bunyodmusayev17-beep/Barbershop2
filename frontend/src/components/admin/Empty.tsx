import type {ReactNode} from 'react';

/** Bo'sh ro'yxat o'rnida ko'rsatiladigan blok. */
export function Empty({children}: {children: ReactNode}) {
  return <div className="rounded-2xl border-2 border-dashed border-navy/20 p-10 text-center text-sm opacity-70">{children}</div>;
}
