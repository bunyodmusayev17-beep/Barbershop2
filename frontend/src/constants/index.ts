import type {AppointmentStatus} from '../api/admin';

/** Backend enum qiymatlari (raqam) */
export const STATUS_VALUE: Record<AppointmentStatus, number> = {Pending: 0, Confirmed: 1, Completed: 2, Cancelled: 3, NoShow: 4};

/** Filtr va ro'yxatlardagi tartib */
export const STATUSES: AppointmentStatus[] = ['Pending', 'Confirmed', 'Completed', 'NoShow', 'Cancelled'];

export const STATUS_LABEL: Record<AppointmentStatus, string> = {
  Pending: 'Kutilmoqda',
  Confirmed: 'Tasdiqlangan',
  Completed: 'Bajarildi',
  Cancelled: 'Bekor qilindi',
  NoShow: 'Kelmadi',
};

export const ROLE_VALUE = {Admin: 0, Barber: 1} as const;
