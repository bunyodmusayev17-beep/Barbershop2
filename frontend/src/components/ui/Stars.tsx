import {cn} from '../../lib/cn';

export function Stars({value, className}: {value: number; className?: string}) {
  return (
    <span className={cn('inline-flex gap-0.5 text-cherry', className)} aria-label={`${value} / 5`}>
      {[1, 2, 3, 4, 5].map((i) => (
        <span key={i} aria-hidden className={i <= Math.round(value) ? '' : 'opacity-25'}>
          ★
        </span>
      ))}
    </span>
  );
}
