import {api, json} from './client';

export type BarberWork = {id: number; url: string; createdAt: string};
export type BarberProfile = {barberId: number; name: string; photoUrl: string | null; works: BarberWork[]; maxWorks: number};

/** Usta kabineti → "Profilim": o'z rasmi va ish rasmlari */
export const profileApi = {
  get: () => api<BarberProfile>('/api/staff/profile'),
  uploadPhoto: (file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return api<BarberProfile>('/api/staff/profile/photo', {method: 'POST', body: fd});
  },
  deletePhoto: () => api<BarberProfile>('/api/staff/profile/photo', json('DELETE')),
  uploadWorks: (files: File[]) => {
    const fd = new FormData();
    files.forEach((f) => fd.append('files', f));
    return api<BarberProfile>('/api/staff/profile/works', {method: 'POST', body: fd});
  },
  deleteWork: (id: number) => api<BarberProfile>(`/api/staff/profile/works/${id}`, json('DELETE')),
};
