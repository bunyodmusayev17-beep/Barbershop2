import type {ButtonHTMLAttributes, ReactNode} from 'react';
import {Link} from 'react-router-dom';
import {cn} from '../../lib/cn';

type Variant = 'primary' | 'outline' | 'ghost';
const variants: Record<Variant, string> = {
  primary: 'bg-cherry text-on-accent hover:bg-cherry-dark shadow-[0_4px_0_0_var(--color-navy)] active:translate-y-0.5 active:shadow-[0_2px_0_0_var(--color-navy)]',
  outline: 'border-2 border-navy text-navy hover:bg-navy hover:text-cream',
  ghost: 'text-navy hover:bg-cream-dark',
};
const base =
  'inline-flex items-center justify-center gap-2 rounded-full px-5 py-2.5 text-[13px] font-bold tracking-[0.12em] md:px-6 md:py-3 md:text-sm md:tracking-[0.15em] uppercase transition disabled:opacity-50 disabled:pointer-events-none focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-cherry/30';

export function Button({variant = 'primary', className, ...props}: ButtonHTMLAttributes<HTMLButtonElement> & {variant?: Variant}) {
  return <button className={cn(base, variants[variant], className)} {...props} />;
}

/** Tugma ko'rinishidagi havola (sahifa ichida o'tish). */
export function ButtonLink({to, variant = 'primary', className, children}: {to: string; variant?: Variant; className?: string; children: ReactNode}) {
  return (
    <Link to={to} className={cn(base, variants[variant], className)}>
      {children}
    </Link>
  );
}
