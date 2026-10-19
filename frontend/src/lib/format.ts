/** Server xatosini o'zbekcha matnga aylantirish (admin panel) */
export function errorText(e: unknown): string {
  const err = e as {status?: number; code?: string; message?: string};
  if (err.code === 'slot_taken' || err.code === 'slot_unavailable') return 'Bu vaqtda ustada boshqa navbat bor';
  if (err.code === 'phone_invalid') return "Telefon raqam noto'g'ri";
  if (err.status === 403) return "Bu amal uchun ruxsat yo'q";
  if (err.status === 409) return err.message || 'Ziddiyat: maʼlumot band';
  return err.message || 'Xatolik yuz berdi';
}

/** 120000 → "120 000 so'm" */
export function money(n: number) {
  return `${n.toLocaleString('ru-RU').replace(/,/g, ' ')} so'm`;
}
