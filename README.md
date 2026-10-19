# Knyaz Barber — sayt, onlayn navbat va admin panel

Toshkentdagi Knyaz Barber (Sergeli tumani, Sergeli-V dahasi, 2) uchun to'liq tizim.
Kod tuzilishi ProfessorSyte uslubida (Controller → Service → Repository).

- **Ommaviy sayt** (o'zbek / rus / ingliz): xizmatlar va narxlar, ustalar (bosilsa — usta ishlari), sharhlar, galereya (usta bo'yicha filtr), xarita.
- **Onlayn navbat**: xizmat → usta (yoki "istalgan usta") → sana va vaqt → ism va telefon.
  Bir ustaga bir vaqtda ikki navbat tushishi **baza darajasida** taqiqlangan (PostgreSQL `EXCLUDE` cheklovi).
- **Admin panel**: statistika, navbatlar, kalendar, xizmatlar, ustalar (rasm, ish jadvali, ta'til), galereya, sharhlar, foydalanuvchilar, sozlamalar.
- **Usta kabineti**: usta faqat o'z navbatlarini ko'radi va holatini belgilaydi; "Profilim" — o'z rasmini qo'yadi va ish rasmlarini yuklaydi (30 tagacha, saytda darhol ko'rinadi, admin galereyadan o'chira oladi).
- **Telegram bot** (token qo'yilganda yoqiladi): yangi navbat xabari, Keldi / Kelmadi / Bekor tugmalari, eslatma, kechki hisobot.

| Qism | Texnologiya |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API, EF Core 10, PostgreSQL 17, JWT |
| Frontend | Vite, React 19, TypeScript, Tailwind CSS 4, TanStack Query, i18next, Framer Motion |
| Testlar | xUnit (bo'sh vaqtlarni hisoblash, telefon formati, Telegram) |

## Lokal ishga tushirish

Kerak: .NET 10 SDK, Node.js 20+, Docker Desktop.

```powershell
# 1. Baza (5434-port — kompyuterdagi PostgreSQL (5432) bilan to'qnashmaydi)
docker compose up -d

# 2. Maxfiy sozlamalar: JWT kaliti va birinchi akkauntlar paroli (bir marta)
.\scripts\dev-secrets.ps1

# 3. Backend — http://localhost:5080 (migratsiya va namunaviy ma'lumotlar avtomatik)
dotnet run --project backend/KnyazBarber.Api --launch-profile http

# 4. Frontend — http://localhost:5173
cd frontend; npm install; npm run dev
```

**Kirish:** http://localhost:5173/login — `admin` (hamma narsa) yoki `baxtiyor`, `sardor`, `bekzod` (usta, faqat o'z navbatlari).

Parolni o'rnatish / tiklash (API to'xtatilgan bo'lishi kerak):

```powershell
dotnet run --project backend/KnyazBarber.Api --launch-profile http -- reset-password admin
```

## Tekshiruvlar

```powershell
dotnet test backend/KnyazBarber.slnx     # unit testlar
cd frontend; npx tsc -b; npx eslint src   # tiplar va lint
```

## Tuzilma

```
backend/KnyazBarber.Api/
  Program.cs          faqat Configurations'dagi kengaytmalarni chaqiradi
  Configurations/     DI, baza, JWT, CORS, rate limiting, Telegram, sozlama klasslari (JwtOptions, TelegramOptions)
  Controllers/        yupqa controller'lar — so'rovni qabul qilib Service'ga uzatadi
  Services/           biznes mantiq: IXxxService + XxxService (navbat, bo'sh vaqtlar, ustalar, xizmatlar...)
    Telegram/         bot: xabarlar, tugmalar, eslatma, kechki hisobot, fon xizmati
  Repositories/       IBaseRepository<T> / BaseRepository<T> — barcha entity'lar uchun umumiy
  Entities/           har bir entity alohida faylda
  Dtos/<Mavzu>/       so'rov va javob modellari (Auth, Booking, Appointment, Barber, ...)
  Exceptions/         AppException → BadRequest / Unauthorized / Forbidden / NotFound / Conflict
  Middlewares/        ExceptionHandlingMiddleware — xatoni ProblemDetails javobiga aylantiradi
  Helpers/            sof yordamchilar: SlotCalculator, BusinessClock, PhoneNumber, Mapping, CLI buyruqlari
  Data/               AppDbContext, EntityConfigurations/, Seeder
  Migrations/
backend/KnyazBarber.Tests/   xUnit
frontend/src/
  api/                client (fetch + token), auth, public, admin, timeOff
  components/ui/      dizayn tizimi (Button, Card, Pager ...), har biri alohida faylda
  components/admin/   admin panel komponentlari (Modal, FormControls, StatusBadge ...)
  components/layout/  sayt sarlavhasi va footer
  components/routing/ ScrollManager, Preloader, TelegramBridge, RouteFallback
  components/fx/      animatsiyalar, 3D barber pole
  features/           bir nechta sahifada ishlatiladigan bo'limlar (NewAppointmentModal)
  hooks/              useAuth, useSite, useToast, usePaged, useMedia
  providers/          AuthProvider, ToastProvider, QueryProvider
  constants/          brend (brand.ts), navbat holatlari
  pages/              HomePage, BookingPage, LoginPage, NotFoundPage; pages/admin/ — panel sahifalari
  i18n/               uz / ru / en matnlari
```

## Xavfsizlik

- Parollar `PasswordHasher` (PBKDF2) bilan saqlanadi; maxfiy kalitlar kodda emas — `user-secrets` (prod: muhit o'zgaruvchilari).
- Login: IP bo'yicha daqiqasiga 10 urinish; onlayn navbat: daqiqasiga 5 ta; bitta telefondan 3 tagacha faol navbat.
- Usta faqat o'z navbatlarini ko'radi/o'zgartiradi — tekshiruv serverda (`barber_id` claim).
- Rasm yuklash: fayl turi ichidagi baytlarga qarab aniqlanadi (JPG/PNG/WEBP), 5 MB gacha, nom — tasodifiy GUID.

## Telegram bot

Token: `Telegram:BotToken` (prod: serverdagi `.env` → `TELEGRAM_BOT_TOKEN`). Bo'sh bo'lsa bot o'chiq, sayt odatdagidek ishlaydi.
Batafsil: [TELEGRAM-BOT.md](TELEGRAM-BOT.md).

## Server (prod)

https://knyazbarber.bunyodmusayev.uz — Docker Compose (`deploy/`, web konteyneri `127.0.0.1:8083`), host nginx + certbot, har kuni zaxira.
Yangilash: Git Bash'da `./deploy/deploy.sh`. Birinchi admin: `create-admin <login>` (konteyner ichida).
