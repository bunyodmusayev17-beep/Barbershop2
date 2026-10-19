import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query';
import {useState} from 'react';
import {adminApi} from '../../api/admin';
import {publicApi} from '../../api/public';
import {Btn} from '../../components/admin/Btn';
import {Field, Input, Select, Textarea} from '../../components/admin/FormControls';
import {Modal} from '../../components/admin/Modal';
import {Spinner} from '../../components/ui/Spinner';
import {useSite} from '../../hooks/useSite';
import {useToast} from '../../hooks/useToast';
import {cn} from '../../lib/cn';
import {todayIso} from '../../lib/dates';
import {errorText, money} from '../../lib/format';
import {formatLocalPhone, isValidLocalPhone} from '../../lib/phone';

/** Xodim qo'lda navbat qo'shadi (telefon orqali yoki to'g'ridan-to'g'ri kelgan mijoz). Navbatlar va kalendar sahifalarida ishlatiladi. */
export function NewAppointmentModal({open, onClose, fixedBarberId, initial}: {open: boolean; onClose: () => void; fixedBarberId: number | null; initial?: {date?: string; time?: string; barberId?: number}}) {
  const site = useSite();
  const qc = useQueryClient();
  const toast = useToast();
  const services = site.data?.services ?? [];
  const barbers = site.data?.barbers ?? [];

  const [form, setForm] = useState({
    serviceId: services[0]?.id ?? 0,
    barberId: fixedBarberId ?? initial?.barberId ?? barbers[0]?.id ?? 0,
    date: initial?.date ?? todayIso(),
    time: initial?.time ?? '',
    name: '',
    phone: '',
    comment: '',
  });
  const set = (p: Partial<typeof form>) => setForm((f) => ({...f, ...p}));
  const serviceId = form.serviceId || services[0]?.id || 0;
  const barberId = form.barberId || barbers[0]?.id || 0;

  const slots = useQuery({
    queryKey: ['slots', serviceId, barberId, form.date],
    queryFn: () => publicApi.slots(serviceId, form.date, barberId),
    enabled: open && !!serviceId && !!barberId,
  });

  const create = useMutation({
    mutationFn: () =>
      adminApi.createAppointment({
        serviceId,
        barberId,
        date: form.date,
        time: form.time,
        clientName: form.name.trim(),
        clientPhone: `+998${form.phone.replace(/\D/g, '')}`,
        comment: form.comment.trim() || undefined,
      }),
    onSuccess: (a) => {
      toast(`Navbat #${a.id} qo'shildi`);
      qc.invalidateQueries({queryKey: ['appointments']});
      qc.invalidateQueries({queryKey: ['stats']});
      qc.invalidateQueries({queryKey: ['slots']});
      set({name: '', phone: '', comment: '', time: ''});
      onClose();
    },
    onError: (e) => toast(errorText(e), 'error'),
  });

  const valid = form.name.trim().length >= 2 && isValidLocalPhone(form.phone) && /^\d{2}:\d{2}$/.test(form.time);
  const service = services.find((s) => s.id === serviceId);

  return (
    <Modal open={open} onClose={onClose} title="Yangi navbat">
      <form
        className="grid gap-4"
        onSubmit={(e) => {
          e.preventDefault();
          if (valid) create.mutate();
        }}
      >
        <Field label="Xizmat">
          <Select value={serviceId} onChange={(e) => set({serviceId: Number(e.target.value), time: ''})}>
            {services.map((s) => (
              <option key={s.id} value={s.id}>
                {s.name.uz} — {money(s.price)} · {s.durationMinutes} daq
              </option>
            ))}
          </Select>
        </Field>
        {fixedBarberId ? null : (
          <Field label="Usta">
            <Select value={barberId} onChange={(e) => set({barberId: Number(e.target.value), time: ''})}>
              {barbers
                .filter((b) => !service || service.barberIds.includes(b.id))
                .map((b) => (
                  <option key={b.id} value={b.id}>
                    {b.name}
                  </option>
                ))}
            </Select>
          </Field>
        )}
        <div className="grid grid-cols-2 gap-3">
          <Field label="Sana">
            <Input type="date" value={form.date} onChange={(e) => set({date: e.target.value, time: ''})} required />
          </Field>
          <Field label="Vaqt" hint="Bo'sh vaqtni tanlang yoki o'zingiz yozing">
            <Input type="time" value={form.time} onChange={(e) => set({time: e.target.value})} step={300} required />
          </Field>
        </div>
        <div className="flex max-h-32 flex-wrap gap-1.5 overflow-y-auto">
          {slots.isLoading ? <Spinner /> : null}
          {slots.data?.slots.map((s) => (
            <button
              key={s.time}
              type="button"
              onClick={() => set({time: s.time})}
              className={cn('rounded-lg border-2 px-2 py-1 text-xs font-bold', form.time === s.time ? 'border-cherry bg-cherry text-white' : 'border-navy/20 hover:border-navy')}
            >
              {s.time}
            </button>
          ))}
          {slots.data && !slots.data.slots.length ? <p className="text-xs opacity-60">Bu kunda onlayn bo'sh vaqt yo'q — vaqtni qo'lda kiriting</p> : null}
        </div>
        <Field label="Mijoz ismi">
          <Input value={form.name} onChange={(e) => set({name: e.target.value})} maxLength={100} required />
        </Field>
        <Field label="Telefon">
          <div className="flex">
            <span className="flex items-center rounded-l-xl border-2 border-r-0 border-navy/25 bg-cream-dark px-3 text-sm font-bold">+998</span>
            <Input className="rounded-l-none" value={form.phone} onChange={(e) => set({phone: formatLocalPhone(e.target.value)})} placeholder="90 123 45 67" inputMode="tel" required />
          </div>
        </Field>
        <Field label="Izoh">
          <Textarea value={form.comment} onChange={(e) => set({comment: e.target.value})} maxLength={500} />
        </Field>
        <div className="flex justify-end gap-2 pt-2">
          <Btn type="button" variant="ghost" onClick={onClose}>
            Bekor
          </Btn>
          <Btn type="submit" disabled={!valid || create.isPending}>
            {create.isPending ? <Spinner /> : 'Saqlash'}
          </Btn>
        </div>
      </form>
    </Modal>
  );
}
