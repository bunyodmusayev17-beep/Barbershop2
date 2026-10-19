/**
 * Telegram Mini App: sayt Telegram ichida ochilganini aniqlash va Telegram imkoniyatlari.
 * Oddiy brauzerda hech narsa yuklanmaydi va hech narsa o'zgarmaydi.
 *
 * Telegram ishga tushirish ma'lumotini URL hash'ida beradi (#tgWebAppData=...). Uni sessiyada saqlaymiz —
 * sahifa yangilansa ham Mini App ekanligi saqlanib qoladi.
 */

type BackButton = {show(): void; hide(): void; onClick(cb: () => void): void; offClick(cb: () => void): void};

type WebApp = {
  initData: string;
  ready(): void;
  expand(): void;
  disableVerticalSwipes?(): void;
  setHeaderColor(color: string): void;
  setBackgroundColor(color: string): void;
  BackButton: BackButton;
  HapticFeedback?: {impactOccurred(style: 'light' | 'medium'): void};
};

declare global {
  interface Window {
    Telegram?: {WebApp?: WebApp};
  }
}

const KEY = 'knyaz_tg_init';
const SCRIPT = 'https://telegram.org/js/telegram-web-app.js';

function readInitData(): string | null {
  const fromHash = new URLSearchParams(window.location.hash.slice(1)).get('tgWebAppData');
  try {
    if (fromHash) sessionStorage.setItem(KEY, fromHash);
    return fromHash ?? sessionStorage.getItem(KEY);
  } catch {
    return fromHash;
  }
}

/** Telegram imzolagan initData (serverda tekshiriladi). Telegram tashqarisida — null. */
export const tgInitData = readInitData();
export const inTelegram = !!tgInitData;

export const webApp = (): WebApp | undefined => window.Telegram?.WebApp;

let ready: Promise<void> | null = null;

/**
 * Telegram ichida: rasmiy skriptni yuklaydi (u ham hash'ni o'qiydi), so'ng hash'ni tozalaydi,
 * ilovani to'liq ekranga yoyadi va rangni saytga moslaydi. Uzog'i bilan 3 soniya kutadi.
 * Sayt buni kutmasdan chiziladi; Telegram imkoniyatlari kerak joy (Orqaga tugmasi) shu va'dani kutadi.
 */
export function initTelegram(): Promise<void> {
  return (ready ??= load());
}

async function load(): Promise<void> {
  if (!inTelegram) return;
  await new Promise<void>((resolve) => {
    const s = document.createElement('script');
    s.src = SCRIPT;
    s.onload = s.onerror = () => resolve();
    document.head.appendChild(s);
    setTimeout(resolve, 3000);
  });
  if (window.location.hash.includes('tgWebAppData')) history.replaceState(null, '', window.location.pathname + window.location.search);

  const app = webApp();
  if (!app) return;
  app.ready();
  app.expand();
  // Sahifani pastga aylantirganda Mini App yopilib/kichrayib ketmasin (Telegram 7.7+)
  app.disableVerticalSwipes?.();
  app.setHeaderColor('#1b1b1a');
  app.setBackgroundColor('#1b1b1a');
}

export function haptic() {
  webApp()?.HapticFeedback?.impactOccurred('light');
}
