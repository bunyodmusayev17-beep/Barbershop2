import {cn} from '../../lib/cn';

/** Rasm bo'lmaganda: monogramma doira */
export function Monogram({name, className}: {name: string; className?: string}) {
  return (
    <div className={cn('flex items-center justify-center rounded-full border-2 border-cherry bg-cream font-deco text-navy', className)}>
      {name.slice(0, 1)}
    </div>
  );
}
