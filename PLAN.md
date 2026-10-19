# Knyaz Barber — veb-tizim rejasi

Sayt: https://knyaz.bunyodmusayev.uz · Kod: https://github.com/bunyodmusayev17-beep/Barbershop2
Joriy holat va keyingi qadamlar qisqacha: [HANDOFF.md](HANDOFF.md) · Texnik tafsilotlar: [README.md](README.md) · Bot: [TELEGRAM-BOT.md](TELEGRAM-BOT.md)

## Maqsad
Toshkentdagi **Knyaz Barber** (Sergeli tumani, Sergeli-V dahasi, 2; tel. +998 90 965 18 77; Yandex: 5.0, 46 baho) uchun
haqiqiy ishlaydigan tizim: mijoz saytda yoki Telegram'da o'zi navbat oladi, admin va ustalar hammasini paneldan boshqaradi,
yangi navbatlar va hisobotlar Telegram'ga keladi.

- Egasi: Sanjar (Telegram'da, kirillcha o'zbek tilida yozadi).
- Narx taklifi: obuna **500 000 so'm/oy** yoki to'liq sotib olish **4 000 000 so'm** (domen/server alohida, 3 oy bepul xizmat).
- **Mustaqil loyiha**: boshqa hech bir loyiha bilan umumiy kod, sozlama, baza yoki ma'lumot yo'q.

## Qarorlar
| Mavzu | Tanlov |
|---|---|
| Backend | C#, **.NET 10**, ASP.NET Core Web API — Controller → Service (interfeys) → `IBaseRepository<T>` → DbContext |
| Baza | **PostgreSQL 17** (Docker), EF Core 10 + Npgsql, migratsiyalar |
| Frontend | **Vite + React 19 + TypeScript + Tailwind 4**, Framer Motion, i18next, TanStack Query, React Router |
| Kod tuzilishi | ProfessorSyte uslubida (`D:\Coding\DotNet\Project\ProfessorSyte`) |
| Auth | Login + parol, **JWT**, rollar: `Admin`, `Barber`; Telegram Mini App'dan parolsiz kirish |
| Tillar | O'zbek, rus, ingliz (UI matnlari + bazadagi kontent 3 tilda) |
| Atama | Hamma joyda **"navbat"** ("zapis" emas) |
| Dizayn (sayt) | 28-variant **"narxlar taxtasi"**: qora kigiz ariqchalari, eman ramka, Oswald, oltin harflar `#f2c14e` — `.theme-letterboard` |
| Dizayn (admin) | Knyaz'ning och varianti: bo'r rang fon, qora matn, to'q oltin `#966a0e`, kigiz yon menyu, eman/oltin kartalar |
| Logo | Toj + K (`frontend/public/favicon.svg`, `components/ui/Crown.tsx`), bot avatari `branding/telegram-bot-avatar.png` |
| Hosting | Server 31.220.81.174, Docker Compose `/opt/knyazbarber`, web `127.0.0.1:8083`, host nginx + Let's Encrypt |
| Domen | `knyaz.bunyodmusayev.uz` (DNS: airnet.uz, A → 31.220.81.174) |
| Vaqt zonasi | Asia/Tashkent (UTC+5), bazada UTC |

## Papkalar
```
KnyazBarber/
├─ backend/KnyazBarber.Api/
│  ├─ Program.cs          faqat Configurations'dagi kengaytmalarni chaqiradi
│  ├─ Configurations/     DI, baza, JWT, CORS, rate limiting, Telegram, sozlama klasslari
│  ├─ Controllers/        yupqa controller'lar (so'rov → Service)
│  ├─ Services/           biznes mantiq: IXxxService + XxxService; Telegram/ — bot
│  ├─ Repositories/       IBaseRepository<T> / BaseRepository<T>
│  ├─ Entities/           har bir entity alohida faylda
│  ├─ Dtos/<Mavzu>/       so'rov va javob modellari
│  ├─ Exceptions/ + Middlewares/   AppException → ProblemDetails
│  ├─ Helpers/            SlotCalculator, BusinessClock, PhoneNumber, CliCommands
│  ├─ Data/               AppDbContext, EntityConfigurations/, Seeder
│  └─ Migrations/
├─ backend/KnyazBarber.Tests/     xUnit (53 test)
├─ frontend/src/
│  ├─ api/  components/{ui,admin,layout,routing,fx}/  features/  hooks/  providers/
│  ├─ constants/ (brand.ts)  pages/ (+ pages/admin/)  i18n/ (uz, ru, en)  lib/
├─ deploy/                docker-compose, nginx, server-setup.sh, deploy.sh, backup.sh
├─ scripts/               dev-secrets.ps1, mock-api.mjs (soxta API), make-bot-avatar.mjs
└─ branding/              30 ta dizayn varianti, bot avatari
```

## Ma'lumotlar modeli
- **User**: Login, Email, PasswordHash, Role (Admin | Barber), BarberId?, IsActive, TelegramUsername?, TelegramUserId?
- **Barber**: Name, Title{uz,ru,en}, Bio{uz,ru,en}, PhotoUrl, SortOrder, IsActive; Works — o'zi yuklagan ish rasmlari
- **Service**: Name{3 til}, Description{3 til}, Price, DurationMinutes, SortOrder, IsActive
- **BarberService**: qaysi usta qaysi xizmatni qiladi (ko'p-ko'p)
- **WorkingHours**: BarberId, DayOfWeek, Start, End, IsDayOff
- **TimeOff**: BarberId, FromUtc, ToUtc, Reason (kasal, ta'til, shaxsiy ish)
- **Appointment**: BarberId, ServiceId, ClientName, ClientPhone, StartUtc, EndUtc,
  Status (Pending | Confirmed | Completed | Cancelled | NoShow), Price (o'sha paytdagi), Comment, Language, ReminderSentAt, CreatedAt
- **GalleryImage**: Url, Caption{3 til}, SortOrder, **BarberId?** (usta ishi bo'lsa — o'sha usta; admin rasmi — null)
- **Review**: AuthorName, Rating, Text, IsPublished
- **SiteSettings**: nom, manzil{3 til}, telefon, Telegram/Instagram, xarita koordinatalari, reyting, slot qadami,
  navbat oldidan minimal vaqt, necha kun oldinga navbat, LastReportDate

## Navbat qoidalari (eng muhim qism)
1. Bo'sh vaqtlar: ustaning ish jadvali − ishlamaydigan vaqtlari − mavjud navbatlar − o'tgan vaqt.
   Qadam sozlamadan (15 daqiqa), davomiylik = xizmat davomiyligi.
2. **Bir ustaga bir vaqtda ikki navbat tushmaydi**: PostgreSQL `EXCLUDE USING gist (barber_id WITH =, tstzrange(start,end) WITH &&)`
   (bekor qilinmaganlar uchun). Poyga holatida ham bazaning o'zi to'xtatadi.
3. Telefon formati: +998 XX XXX XX XX.
4. "Istalgan usta": eng yaqin bo'sh usta avtomatik tanlanadi.
5. Spamga qarshi: IP bo'yicha daqiqasiga 5 ta navbat, bitta telefondan 3 tagacha faol navbat; login — daqiqasiga 10 urinish.
6. Navbatlar avtomatik tasdiqlanadi (`Confirmed`).

## Sahifalar
**Ommaviy sayt** (3 til): bosh sahifa — hero, xizmatlar va narxlar, ustalar (bosilsa — usta haqida va ishlari), sharhlar,
galereya (usta bo'yicha filtr, kattalashtirish), manzil va xarita · onlayn navbat (xizmat → usta → sana/vaqt → ism/telefon → tasdiq).

**Admin panel** (`Admin`): Boshqaruv (bugun, kutilayotgan, 7 kunlik daromad, kelmaganlar, grafik, ustalar) · Navbatlar
(filtrlar, holat, qo'lda navbat) · Kalendar (ustalar bo'yicha kun; Kecha/Bugun/Ertaga yoki sana) · Xizmatlar · Ustalar
(profil, xizmatlari, haftalik jadval, rasm, ta'til) · Ishlamaydigan kunlar · Galereya (usta ishlari ism bilan belgilangan) ·
Sharhlar · Foydalanuvchilar (admin/usta akkauntlari, Telegram) · Sozlamalar.

**Usta kabineti** (`Barber`): o'z navbatlari va holatini belgilash · Kalendar · "Ishlamayman" (qizil → bosilsa yashil
"Ishlayman", kun yopiladi/qayta ochiladi) · **Profilim va ishlarim**: o'z rasmi va 30 tagacha ish rasmi (saytda darhol ko'rinadi).

**Telegram bot + Mini App**:
- Mijoz: /start — saytdagi ma'lumot (manzil, ish vaqti, reyting, telefon; Telegram tiliga qarab uz/ru/en) va tugmalar:
  Navbat olish, Xizmatlar va narxlar (sayt Telegram ichida ochiladi), Manzil (xarita), Qo'ng'iroq; /manzil, /aloqa.
- Xodim: yangi navbat xabari (admin + usta), Keldi / Kelmadi / Bekor (ikki bosqichli) / Qo'ng'iroq tugmalari,
  ustaga 2 soat oldin eslatma, adminlarga kechki hisobot; Mini App'dan parolsiz kirib panel/kabinet.
- Qotmaslik: har xabarga 30 soniya chegara, tugmaga har doim javob, 429 da qayta urinish, ishga tushishda internet bo'lmasa qayta sozlash.

## Ish tartibi (bosqichlar) — holat
1. ✅ Loyiha NY nusxasidan alohida mustaqil kodga aylantirildi, ProfessorSyte uslubiga keltirildi.
2. ✅ Dizayn: 30 variant → 28 "narxlar taxtasi" tanlandi; admin panel Knyaz ranglarida.
3. ✅ Logo va favicon (toj + K), bot avatari.
4. ✅ Usta o'z rasmi va ish rasmlarini yuklaydi; saytda usta oynasi va galereya filtri.
5. ✅ Telegram bot: mijoz uchun saytdagidek /start, qotishlarga qarshi himoya (kod tayyor, token kutilmoqda).
6. ✅ GitHub (Barbershop2, public), serverga chiqarish, HTTPS, kunlik zaxira.
7. ✅ Telefon va Telegram uchun tezlik va ixcham o'lchamlar.
8. ✅ Vaqtinchalik namunaviy ma'lumot (`seed-demo`): 5 xizmat, 3 usta, 3 sharh, navbatlar.
9. ⏳ **Bot tokeni**: BotFather'da bot ochish → tokenni serverga → adminni ulash (qadamlar: TELEGRAM-BOT.md).
10. ⏳ **Haqiqiy ma'lumot** Sanjardan: xizmatlar va narxlar, ustalar (ism, rasm, jadval), ish vaqti, rasmlar, sharhlar.
    Kiritishdan oldin: `clear-demo` (namunaviylarni tozalaydi, akkaunt va sozlamalar qoladi).
11. ⏳ Ustalarga akkaunt berish va Telegram'ga ulash; Sanjarga panelni o'rgatish.
12. ⏳ Haqiqiy telefonda va Telegram ichida yakuniy sinov.
13. ⏳ Topshirish va narx bo'yicha kelishuv.

## Kelajakda (ixtiyoriy)
- Meta teglar (`description`, `og:image`) — Telegram/ijtimoiy tarmoqda havola chiroyli ko'rinishi uchun.
- Mijozga navbat eslatmasi (SMS yoki bot orqali).
- Instagram (topilsa) va Yandex'dagi haqiqiy sharhlar.

## Buyruqlar
| Nima | Buyruq |
|---|---|
| Serverga yangilash | Git Bash: `./deploy/deploy.sh` |
| Birinchi admin / parol | `ssh -t root@31.220.81.174 "cd /opt/knyazbarber && docker compose exec api dotnet KnyazBarber.Api.dll create-admin <login>"` (`reset-password` ham shunday) |
| Xodim Telegram'i | `... docker compose exec api dotnet KnyazBarber.Api.dll set-telegram <login> @username` |
| Namunaviy ma'lumot | `... docker compose exec -T api dotnet KnyazBarber.Api.dll seed-demo` |
| Namunaviylarni tozalash | `ssh -t ... "... docker compose exec api dotnet KnyazBarber.Api.dll clear-demo"` ("ha" deb tasdiqlanadi) |
| Lokal ko'rish (Docker'siz) | `.claude/launch.json` → `mock-api` + `web-mock-api` |
| Testlar | `dotnet test backend/KnyazBarber.slnx`; `cd frontend; npx tsc -b; npx eslint src` |
