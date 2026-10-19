import {api, json} from './client';
import type {LoginResponse, UserDto} from './types';

export const authApi = {
  login: (login: string, password: string) => api<LoginResponse>('/api/auth/login', json('POST', {login, password})),
  me: () => api<UserDto>('/api/auth/me'),
  /** Mini App: Telegram imzolagan initData bilan parolsiz kirish. Xodim bo'lmasa — undefined (204). */
  telegram: (initData: string) => api<LoginResponse | undefined>('/api/auth/telegram', json('POST', {initData})),
};
