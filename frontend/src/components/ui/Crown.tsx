import {cn} from '../../lib/cn';

/** Brend belgisi — toj ("knyaz" = shahzoda). Rangi currentColor'dan; shakli public/favicon.svg dagi toj bilan bir xil. */
export function Crown({className}: {className?: string}) {
  return (
    <svg viewBox="26 6 48 28" aria-hidden className={cn('h-auto', className)} fill="currentColor">
      <polygon points="26,34 26,12 38,22 50,6 62,22 74,12 74,34" />
    </svg>
  );
}
