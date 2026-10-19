import {api, json} from './client';
import type {BookingConfirmation, CreateBookingRequest, SiteResponse, SlotsResponse} from './types';

/** Saytning ommaviy qismi: kontent, bo'sh vaqtlar va navbatga yozilish */
export const publicApi = {
  site: () => api<SiteResponse>('/api/public/site'),
  slots: (serviceId: number, date: string, barberId: number | null) =>
    api<SlotsResponse>(`/api/booking/slots?serviceId=${serviceId}&date=${date}${barberId ? `&barberId=${barberId}` : ''}`),
  book: (req: CreateBookingRequest) => api<BookingConfirmation>('/api/booking', json('POST', req)),
};
