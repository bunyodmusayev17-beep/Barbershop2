import type {AppointmentStatus} from '../../api/admin';
import {STATUS_LABEL} from '../../constants';
import {cn} from '../../lib/cn';

const STATUS_STYLE: Record<AppointmentStatus, string> = {
  Pending: 'bg-amber-100 text-amber-800 border-amber-300',
  Confirmed: 'bg-sky-100 text-sky-800 border-sky-300',
  Completed: 'bg-emerald-100 text-emerald-800 border-emerald-300',
  Cancelled: 'bg-zinc-100 text-zinc-500 border-zinc-300 line-through',
  NoShow: 'bg-rose-100 text-rose-800 border-rose-300',
};

export function StatusBadge({status}: {status: AppointmentStatus}) {
  return <span className={cn('inline-block whitespace-nowrap rounded-full border px-2.5 py-0.5 text-xs font-semibold', STATUS_STYLE[status])}>{STATUS_LABEL[status]}</span>;
}
