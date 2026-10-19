import type {Appointment} from './admin';
import {api, json} from './client';

export type StaffTimeOff = {id: number; barberId: number; barberName: string; from: string; to: string; reason: string | null};
export type StaffTimeOffResult = {timeOff: StaffTimeOff; conflicts: Appointment[]};

/** Ishlamaydigan vaqtlar: usta — o'zi uchun, admin — istalgan usta uchun */
export const timeOffApi = {
  list: (q: {from?: string; to?: string; barberId?: number | null} = {}) => {
    const p = new URLSearchParams();
    if (q.from) p.set('from', q.from);
    if (q.to) p.set('to', q.to);
    if (q.barberId) p.set('barberId', String(q.barberId));
    return api<StaffTimeOff[]>(`/api/staff/timeoffs?${p}`);
  },
  create: (t: {barberId?: number | null; from: string; to: string; reason?: string}) => api<StaffTimeOffResult>('/api/staff/timeoffs', json('POST', t)),
  remove: (id: number) => api<void>(`/api/staff/timeoffs/${id}`, json('DELETE')),
};
