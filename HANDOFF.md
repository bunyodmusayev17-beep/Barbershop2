# KnyazBarber — holat va rejalar (yangi sessiya uchun)

Batafsil texnik ma'lumot: [README.md](README.md).

## Loyiha
Knyaz Barber (Toshkent, Sergeli tumani, Sergeli-V dahasi, 2; tel. +998 90 965 18 77; Yandex: 5.0, 46 baho;
Yandex Maps: https://yandex.uz/maps/org/112879150656/) uchun sayt + onlayn navbat + admin panel + usta kabineti.
Egasi bilan Telegram'da yozishilmoqda (Sanjar, kirillcha o'zbek tilida yozadi). Instagram'i topilmadi.
Narx taklifi: obuna 500 000 so'm/oy yoki to'liq sotib olish 4 000 000 so'm (domen/server alohida, 3 oy bepul xizmat).

Mustaqil loyiha — boshqa hech bir loyiha bilan umumiy kod, sozlama yoki ma'lumot yo'q (foydalanuvchi talabi).
Kod tuzilishi ProfessorSyte (`D:\Coding\DotNet\Project\ProfessorSyte`) uslubiga keltirilgan.

## Foydalanuvchi qarorlari (2026-10-18)
- Backend: Controller → Service (interfeys bilan) → `IBaseRepository<T>` → DbContext; papka `backend/KnyazBarber.Api`.
- Frontend: React + TypeScript (.tsx), papkalar ProfessorSyte'dek.
- Tillar: o'zbek, rus, ingliz. Domen: `knyaz.bunyodmusayev.uz`. Telegram bot — keyinroq (token yo'q, kod tayyor).
- **Dizayn: 28-variant "narxlar taxtasi"** (qora kigiz ariqchalari, eman ramka, Oswald, oltin harflar) — `src/index.css` → `.theme-letterboard`, faqat ommaviy saytda (Layout). Admin panel och rangda qoladi. 30 ta variant rasmi: `branding/dizaynlar/`.
  Brend nomi bitta joyda: `frontend/src/constants/brand.ts`.
- Matnlarda "zapis" emas, "navbat" (sayt, admin panel, bot).

## Diqqat
- Lokal kompyuterda Docker yo'q — backend'ni bazasiz ishga tushirib bo'lmaydi.
  Frontendni ko'rish uchun: `mock-api` (`scripts/mock-api.mjs`, port 5090, Seeder namunalari) + `web-mock-api`.
- Serverda boshqa saytlar ham ishlaydi — Knyaz faqat o'z papkasi (`/opt/knyazbarber`), konteynerlari va 8083-portini ishlatadi.
- Prod (2026-10-19): https://knyaz.bunyodmusayev.uz — DNS `knyaz` A → 31.220.81.174, nginx + Let's Encrypt (avto yangilanadi),
  kunlik zaxira 03:30 (`/etc/cron.d/knyazbarber-backup`). Prod baza toza: xizmat va ustalar admin paneldan kiritiladi.

## Navbatdagi ishlar
1. ~~Logo va favicon~~ — tayyor (2026-10-18): belgi "toj + K" (`frontend/public/favicon.svg`, `favicon-32.png`,
   `apple-touch-icon.png`), sarlavha/footer/admin'da toj + KNYAZ / BARBER (`components/ui/Crown.tsx`).2. Egasidan haqiqiy ma'lumotlar: xizmatlar va narxlar, ustalar (ism, rasm), ish vaqti, rasmlar.
3. ~~Serverga chiqarish~~ — tayyor (2026-10-19). Yangilash: Git Bash'da `./deploy/deploy.sh`.
4. Bot tokeni (BotFather) — egasi rozi bo'lsa.
