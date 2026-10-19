import {cn} from '../../lib/cn';

/** Gorizontal barber-pole chizig'i (bo'limlar ajratgichi). */
export function BarberPole({className}: {className?: string}) {
  return <div aria-hidden className={cn('barber-pole', className)} />;
}
