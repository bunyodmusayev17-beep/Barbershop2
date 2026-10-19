import type {ButtonHTMLAttributes} from 'react';
import {cn} from '../../lib/cn';

type Variant = 'primary' | 'outline' | 'danger' | 'ghost';
const variants: Record<Variant, string> = {
  primary: 'bg-cherry text-white hover:bg-cherry-dark',
  outline: 'border-2 border-navy text-navy hover:bg-navy hover:text-cream',
  danger: 'border-2 border-rose-300 text-rose-700 hover:bg-rose-50',
  ghost: 'text-navy hover:bg-cream-dark',
};

/** Admin paneldagi ixcham tugma. */
export function Btn({variant = 'primary', className, ...props}: ButtonHTMLAttributes<HTMLButtonElement> & {variant?: Variant}) {
  return (
    <button
      {...props}
      className={cn(
        'inline-flex items-center justify-center gap-2 rounded-full px-4 py-2 text-xs font-bold tracking-widest uppercase transition disabled:pointer-events-none disabled:opacity-50',
        variants[variant],
        className,
      )}
    />
  );
}
