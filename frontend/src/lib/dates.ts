/** Barbershop vaqt zonasi bo'yicha sanalar (foydalanuvchi qaysi zonada bo'lishidan qat'i nazar). */
export const BUSINESS_TZ = 'Asia/Tashkent';

/** Toshkent vaqti bo'yicha bugungi sana, YYYY-MM-DD */
export function todayIso(): string {
  return new Intl.DateTimeFormat('en-CA', {timeZone: BUSINESS_TZ, year: 'numeric', month: '2-digit', day: '2-digit'}).format(new Date());
}

export function addDays(iso: string, days: number): string {
  const [y, m, d] = iso.split('-').map(Number);
  const dt = new Date(Date.UTC(y, m - 1, d + days));
  return dt.toISOString().slice(0, 10);
}

const MONTHS_UZ = ['yanvar', 'fevral', 'mart', 'aprel', 'may', 'iyun', 'iyul', 'avgust', 'sentabr', 'oktabr', 'noyabr', 'dekabr'];

/** "20-oktabr" */
export function dayMonthUz(iso: string): string {
  const p = parts(iso);
  return `${p.day}-${MONTHS_UZ[p.month - 1]}`;
}

/** Bugunga nisbatan: "Kecha" / "Bugun" / "Ertaga", qolgan kunlar — "22-oktabr" */
export function relativeDayUz(iso: string, today = todayIso()): string {
  if (iso === today) return 'Bugun';
  if (iso === addDays(today, 1)) return 'Ertaga';
  if (iso === addDays(today, -1)) return 'Kecha';
  return dayMonthUz(iso);
}

export function parts(iso: string) {
  const [y, m, d] = iso.split('-').map(Number);
  const weekday = new Date(Date.UTC(y, m - 1, d)).getUTCDay();
  return {year: y, month: m, day: d, weekday};
}

/** "14 oktabr, Seshanba" */
export function formatDate(iso: string, months: string[], days: string[]): string {
  const p = parts(iso);
  return `${p.day} ${months[p.month - 1]}, ${days[p.weekday]}`;
}
