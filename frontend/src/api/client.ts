import {API_BASE} from '../lib/native';

const TOKEN_KEY = 'knyaz_token';

/** Ilovada server bergan "/uploads/..." manzillari to'liq manzilga aylanadi (saytda o'zgarmaydi) */
const absoluteUploads = (_key: string, value: unknown) =>
  typeof value === 'string' && value.startsWith('/uploads/') ? API_BASE + value : value;

/** Kirish tokeni (localStorage). Har qanday so'rov 401 qaytarsa o'chiriladi va LOGOUT_EVENT yuboriladi. */
export const auth = {
  get token(): string | null {
    try {
      return localStorage.getItem(TOKEN_KEY);
    } catch {
      return null;
    }
  },
  set token(value: string | null) {
    try {
      if (value) localStorage.setItem(TOKEN_KEY, value);
      else localStorage.removeItem(TOKEN_KEY);
    } catch {
      /* private rejim — token faqat shu sessiyada yo'qoladi */
    }
  },
};

export const LOGOUT_EVENT = 'knyaz:logout';

/** Server xatosi: `code` — 3 tilda xabar ko'rsatish uchun (navbat xatolari). */
export class ApiError extends Error {
  status: number;
  code: string | undefined;

  constructor(status: number, code: string | undefined, message: string) {
    super(message);
    this.status = status;
    this.code = code;
  }
}

export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  if (init.body && !(init.body instanceof FormData)) headers.set('Content-Type', 'application/json');
  const token = auth.token;
  if (token) headers.set('Authorization', `Bearer ${token}`);

  const res = await fetch(API_BASE + path, {...init, headers});
  if (res.status === 401 && token) {
    auth.token = null;
    window.dispatchEvent(new Event(LOGOUT_EVENT));
  }
  if (!res.ok) {
    let body: {title?: string; detail?: string; code?: string} = {};
    try {
      body = await res.json();
    } catch {
      /* bo'sh javob */
    }
    // Server xabari detail da keladi; validatsiya xatosida esa faqat umumiy title bo'ladi
    throw new ApiError(res.status, body.code, body.detail ?? body.title ?? res.statusText);
  }
  if (res.status === 204) return undefined as T;
  if (!API_BASE) return res.json() as Promise<T>;
  return JSON.parse(await res.text(), absoluteUploads) as T;
}

/** JSON so'rov: json('POST', body) */
export const json = (method: string, body?: unknown): RequestInit => ({method, body: body === undefined ? undefined : JSON.stringify(body)});
