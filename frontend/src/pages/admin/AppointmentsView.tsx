import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query';
import {useMemo, useState} from 'react';
import {adminApi, type Appointment, type AppointmentStatus} from '../../api/admin';
import {STATUSES, STATUS_LABEL} from '../../constants';
import {useAuth} from '../../hooks/useAuth';
import {Btn} from '../../components/admin/Btn';
import {Empty} from '../../components/admin/Empty';
import {errorText, money} from '../../lib/format';
import {Input, Select} from '../../components/admin/FormControls';
import {PageHeader} from '../../components/admin/PageHeader';
import {Panel} from '../../components/admin/Panel';
import {StatusBadge} from '../../components/admin/StatusBadge';
import {useToast} from '../../hooks/useToast';
import {Spinner} from '../../components/ui/Spinner';
import {useSite} from '../../hooks/useSite';
import {cn} from '../../lib/cn';
import {addDays, todayIso} from '../../lib/dates';
import {NewAppointmentModal} from '../../features/appointments/NewAppointmentModal';

const RANGES = [
  {key: 'today', label: 'Bugun'},
  {key: 'tomorrow', label: 'Ertaga'},
  {key: 'week', label: '7 kun'},
  {key: 'month', label: '30 kun'},
  {key: 'past', label: "O'tgan 30 kun"},
] as const;
type RangeKey = (typeof RANGES)[number]['key'];

function rangeDates(key: RangeKey) {
  const t = todayIso();
  switch (key) {
    case 'today':
      return {from: t, to: t};
    case 'tomorrow':
      return {from: addDays(t, 1), to: addDays(t, 1)};
    case 'week':
      return {from: t, to: addDays(t, 6)};
    case 'month':
      return {from: t, to: addDays(t, 29)};
    case 'past':
      return {from: addDays(t, -30), to: addDays(t, -1)};
  }
}

/** Keyingi mantiqiy holatlar — tezkor tugmalar uchun */
const QUICK: Partial<Record<AppointmentStatus, AppointmentStatus[]>> = {
  Pending: ['Confirmed', 'Cancelled'],
  Confirmed: ['Completed', 'NoShow', 'Cancelled'],
};

export default function AppointmentsView() {
  const {user} = useAuth();
  const isAdmin = user?.role === 'Admin';
  const site = useSite();
  const qc = useQueryClient();
  const toast = useToast();

  const [range, setRange] = useState<RangeKey>('week');
  const [barberId, setBarberId] = useState<number | null>(null);
  const [status, setStatus] = useState<AppointmentStatus | ''>('');
  const [search, setSearch] = useState('');
  const [creating, setCreating] = useState(false);

  const {from, to} = rangeDates(range);
  const query = useQuery({
    queryKey: ['appointments', from, to, barberId, status, search],
    queryFn: () => adminApi.appointments({from, to, barberId, status, search: search.trim()}),
  });

  const mutate = useMutation({
    mutationFn: ({id, s}: {id: number; s: AppointmentStatus}) => adminApi.setStatus(id, s),
    onSuccess: (a) => {
      toast(`#${a.id}: ${STATUS_LABEL[a.status]}`);
      qc.invalidateQueries({queryKey: ['appointments']});
      qc.invalidateQueries({queryKey: ['stats']});
    },
    onError: (e) => toast(errorText(e), 'error'),
  });

  const grouped = useMemo(() => {
    const m = new Map<string, Appointment[]>();
    for (const a of query.data ?? []) m.set(a.date, [...(m.get(a.date) ?? []), a]);
    return [...m.entries()];
  }, [query.data]);

  const total = (query.data ?? []).filter((a) => a.status !== 'Cancelled' && a.status !== 'NoShow').reduce((s, a) => s + a.price, 0);

  return (
    <>
      <PageHeader
        title={isAdmin ? 'Navbatlar' : 'Mening navbatlarim'}
        sub={query.data ? `${query.data.length} ta navbat · ${money(total)}` : undefined}
        actions={<Btn onClick={() => setCreating(true)}>+ Yangi navbat</Btn>}
      />

      <Panel className="mb-5">
        <div className="flex flex-wrap gap-2">
          {RANGES.map((r) => (
            <button
              key={r.key}
              type="button"
              onClick={() => setRange(r.key)}
              className={cn('rounded-full border-2 px-3 py-1.5 text-xs font-bold tracking-wider uppercase', range === r.key ? 'border-navy bg-navy text-cream' : 'border-navy/20 hover:border-navy')}
            >
              {r.label}
            </button>
          ))}
        </div>
        <div className="mt-4 grid gap-3 sm:grid-cols-3">
          {isAdmin ? (
            <Select value={barberId ?? ''} onChange={(e) => setBarberId(e.target.value ? Number(e.target.value) : null)} aria-label="Usta">
              <option value="">Barcha ustalar</option>
              {site.data?.barbers.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.name}
                </option>
              ))}
            </Select>
          ) : null}
          <Select value={status} onChange={(e) => setStatus(e.target.value as AppointmentStatus | '')} aria-label="Holat">
            <option value="">Barcha holatlar</option>
            {STATUSES.map((s) => (
              <option key={s} value={s}>
                {STATUS_LABEL[s]}
              </option>
            ))}
          </Select>
          <Input value={search} onChange={(e) => setSearch(e.target.value)} placeholder="Ism yoki telefon bo'yicha qidirish" />
        </div>
      </Panel>

      {query.isLoading ? (
        <Spinner className="h-8 w-8" />
      ) : grouped.length === 0 ? (
        <Empty>Bu davrda navbat topilmadi</Empty>
      ) : (
        <div className="space-y-6">
          {grouped.map(([date, items]) => (
            <div key={date}>
              <h3 className="mb-2 text-sm font-bold tracking-[0.2em] uppercase text-navy/70">
                {date === todayIso() ? 'Bugun · ' : ''}
                {date}
              </h3>
              <div className="grid gap-2">
                {items.map((a) => (
                  <div key={a.id} className={cn('flex flex-wrap items-center gap-x-4 gap-y-2 rounded-2xl border-2 bg-white p-4', a.status === 'Cancelled' ? 'border-zinc-200 opacity-60' : 'border-navy/15')}>
                    <div className="w-24">
                      <p className="text-lg font-bold">{a.time}</p>
                      <p className="text-xs opacity-60">{a.endTime} gacha</p>
                    </div>
                    <div className="min-w-40 flex-1">
                      <p className="font-bold">{a.clientName}</p>
                      <a href={`tel:${a.clientPhone}`} className="text-sm text-cherry hover:underline">
                        {a.clientPhone}
                      </a>
                      {a.comment ? <p className="mt-1 text-xs italic opacity-70">“{a.comment}”</p> : null}
                    </div>
                    <div className="min-w-36">
                      <p className="text-sm font-semibold">{a.serviceName.uz}</p>
                      <p className="text-xs opacity-60">
                        {isAdmin ? `${a.barberName} · ` : ''}
                        {money(a.price)}
                      </p>
                    </div>
                    <StatusBadge status={a.status} />
                    <div className="flex flex-wrap gap-1.5">
                      {(QUICK[a.status] ?? []).map((s) => (
                        <Btn key={s} variant={s === 'Cancelled' || s === 'NoShow' ? 'danger' : 'outline'} className="!px-3 !py-1.5 !text-[10px]" disabled={mutate.isPending} onClick={() => mutate.mutate({id: a.id, s})}>
                          {STATUS_LABEL[s]}
                        </Btn>
                      ))}
                      <Select
                        aria-label="Holatni o'zgartirish"
                        className="!w-auto !py-1 !text-xs"
                        value=""
                        onChange={(e) => e.target.value && mutate.mutate({id: a.id, s: e.target.value as AppointmentStatus})}
                      >
                        <option value="">…</option>
                        {STATUSES.filter((s) => s !== a.status).map((s) => (
                          <option key={s} value={s}>
                            {STATUS_LABEL[s]}
                          </option>
                        ))}
                      </Select>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}

      <NewAppointmentModal open={creating} onClose={() => setCreating(false)} fixedBarberId={isAdmin ? null : (user?.barberId ?? null)} />
    </>
  );
}
