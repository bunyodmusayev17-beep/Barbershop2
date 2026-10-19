# Telegram bot + Mini App — reja

## Holat (2026-10-19): kod tayyor, bot hali yaratilmagan, sayt hali serverga chiqmagan

Kod tayyor, testlar (53 ta) o'tadi: /start bog'lash (username va ID), yangi navbat xabari (admin + usta), 4 tugma va ruxsatlar,
bekor qilish → ustaga xabar, 2 soatlik eslatma, kechki hisobot, Mini App'dan parolsiz kirish → /admin.
Mijoz uchun /start — saytdagi ma'lumot (manzil, ish vaqti, reyting, telefon; uz/ru/en — Telegram tiliga qarab) va tugmalar:
✂️ Navbat olish (sayt → /book), 💈 Xizmatlar va narxlar (sayt → /#services), 📍 Manzil (xarita), 📞 Qo'ng'iroq.
Buyruqlar: /start, /manzil, /aloqa.

**Qotib qolmasligi uchun:** har bir xabar/tugma 30 soniya bilan cheklangan; tugma xato bersa ham javob beriladi
(tugmada "yuklanmoqda" osilib qolmaydi); "message is not modified" xato hisoblanmaydi; Telegram 429 qaytarsa — 3 martagacha
qayta yuboriladi; server internet yo'q paytda ishga tushsa, bot sozlamalari har daqiqada qayta uriniladi;
Mini App'da og'ir 3D o'chirilgan (Telegram brauzerida sekinlashadi).

### 1. BotFather'da bot yaratish (Sanjar yoki biz — bir marta)
1. @BotFather → `/newbot` → nomi: `Knyaz Barber` → username: masalan `knyazbarber_bot` (band bo'lsa `knyaz_barber_bot`).
2. `/setuserpic` → `branding/telegram-bot-avatar.png` (qayta yasash: `node scripts/make-bot-avatar.mjs`).
3. `/setabouttext` (120 belgigacha, bot profilida):
   `Sergeli-V, 2 dagi sartaroshxona. Onlayn navbat · +998 90 965 18 77`
4. `/setdescription` (Start bosilishidan oldin ko'rinadi):
   ```
   💈 Knyaz Barber — Sergeli-V, 2.
   Onlayn navbat oling, narxlar va ustalarni ko'ring. Start tugmasini bosing 👇

   💈 Knyaz Barber — Сергели-V, 2.
   Онлайн-запись, цены и мастера. Нажмите «Старт» 👇
   ```
5. Token — **chatga yozilmaydi**, faqat serverga (pastda).

Buyruqlar va "Ochish" (Mini App) menyu tugmasini bot ishga tushganda o'zi sozlaydi — BotFather'da `/setcommands` shart emas.

### 2. Serverga ulash (sayt serverga chiqqandan keyin — Mini App uchun HTTPS kerak)
1. `ssh -t root@31.220.81.174 'read -s -p "Token: " T && echo && echo "TELEGRAM_BOT_TOKEN=$T" >> /opt/knyazbarber/.env && cd /opt/knyazbarber && docker compose up -d'`
2. Admin: `... docker compose exec api dotnet KnyazBarber.Api.dll create-admin <login>`,
   so'ng `... docker compose exec api dotnet KnyazBarber.Api.dll set-telegram <login> @telegram_username`.
3. Telegram'da botga /start → "admin sifatida ulangansiz". Keyin ustalarni panel → Foydalanuvchilar → Telegram maydoni.

Logda tekshirish: `docker compose logs api | grep Telegram` → "Telegram bot ishga tushdi: @<username>".

**Muhim:** bitta token bilan faqat bitta joyda polling ishlaydi. Lokalda sinash uchun alohida test bot oching
(`dotnet user-secrets set Telegram:BotToken ...` — foydalanuvchi o'z terminalida).
`Telegram:ApiBaseUrl` — soxta/o'z Bot API serveri uchun (test).

## Umumiy
- **Bot** — faqat xodimlar uchun (admin va ustalar): xabarlar, tugmalar, eslatma, hisobot.
- **Mini App** — mavjud React sayt Telegram ichida 1ga 1 ochiladi:
  - **mijoz** → ommaviy sayt va onlayn navbat (saytdagidek);
  - **xodim** → parolsiz kirib, admin panel / usta kabineti.
- Zapislar endi **avtomatik tasdiqlanadi** (`Confirmed`).

## Xodimlarni Telegramga bog'lash (parolsiz)
1. **Birinchi admin** — biz o'zimiz belgilaymiz, CLI buyrug'i bilan (`reset-password` kabi):
   `dotnet run ... -- set-telegram admin @username` (yoki raqamli Telegram ID).
2. **Keyin admin o'zi**: "Foydalanuvchilar" sahifasida har bir usta/admin uchun Telegram username (yoki ID) kiritadi.
   Username bazada unique.
3. Xodim botda **Start** bosadi yoki Mini App'ni ochadi → server uning Telegram username'ini tekshiradi →
   mos kelsa **Telegram ID** doimiy bog'lanadi. Keyin faqat ID ishlatiladi (username o'zgarsa ham ishlayveradi).

Nega Start shart: Telegram boti birinchi bo'lib yoza olmaydi va username'dan ID topa olmaydi —
odam botga bir marta kirishi kerak. Username'i yo'q xodim uchun admin raqamli ID kiritadi.

Xavfsizlik: Mini App ma'lumoti (`initData`) serverda bot tokeni bilan HMAC orqali tekshiriladi —
username/ID ni soxtalashtirib bo'lmaydi. Saytdagi login/parol bilan kirish ham saqlanadi.

## Bot funksiyalari

| # | Nima | Kimga |
|---|---|---|
| 1 | **Yangi navbat xabari** (saytdan, Mini App'dan yoki paneldan qo'lda): mijoz, telefon, xizmat, usta, vaqt, narx, izoh | Adminlar + o'sha usta |
| 2 | **Panelda bekor qilinsa** — xabar | O'sha usta |
| 3 | Xabar ostida **inline tugmalar**: ✅ Keldi · 🚫 Kelmadi · ❌ Bekor qilish · 📞 Qo'ng'iroq | Admin — hamma navbat, usta — o'ziniki |
| 4 | **2 soat oldin eslatma** (tugmalar bilan) | Usta |
| 5 | **Kechki hisobot**: keldi / kelmadi / bekor, kunlik tushum, ustalar kesimida | Adminlar |
| 6 | **Menyu tugmasi** "Ochish" → Mini App | Hamma |

### Tugmalar qoidasi
- **Keldi / Kelmadi** — faqat navbat vaqti boshlangandan keyin.
- **Bekor qilish** — ikki bosqichli ("Ha, bekor qilinsin / Yo'q").
- **📞** — Telegram inline tugmada `tel:` ga ruxsat bermaydi → bot mijoz kontaktini yuboradi (bir bosishda qo'ng'iroq).
- Bosilgach xabar tahrirlanadi ("✅ Keldi — Sardor, 14:32"), tugmalar olib tashlanadi.
- Har bosishda server tekshiradi: Telegram ID bog'langan faol xodimmi, usta bo'lsa — navbat o'zinikimi.

### Kechki hisobot vaqti
Har kuni avtomatik: o'sha kuni ishlagan ustalar jadvalidagi **eng kech tugash vaqti + 30 daqiqa**.
Hech kim ishlamagan kuni hisobot yuborilmaydi.

### 2 soatlik eslatma
`Appointment.ReminderSentAt` bilan bir martalik. 2 soatlik oyna ichida yaratilgan navbatga yuborilmaydi.

## Mini App
- Bot menyu tugmasi va BotFather'da Mini App manzili — **HTTPS kerak**:
  demo — Cloudflare tunnel, prod — domen + SSL.
- Frontend'ga `telegram-web-app.js` qo'shiladi; Telegram tashqarisida hech narsa o'zgarmaydi.
- Ochilganda: `initData` → `POST /api/auth/telegram` → xodim bo'lsa JWT qaytadi va panel/kabinet ochiladi,
  aks holda — oddiy ommaviy sayt.
- Telegram ichida: tepadagi **Orqaga** tugmasi brauzer orqaga o'tishi bilan bog'lanadi (navbat qadamlari),
  xavfsiz hudud (safe area) hisobga olinadi. Dizayn o'zgarmaydi.

## Texnik yechim
- `Telegram.Bot` paketi, **long polling** (`BackgroundService`) — public URL kerak emas.
  Prod'da webhook'ga o'tish mumkin (handler o'zgarmaydi).
- `Telegram:BotToken` bo'sh bo'lsa bot o'chiq — sayt va API odatdagidek ishlaydi.
  Token `user-secrets` da (`scripts/dev-secrets.ps1`), prod'da muhit o'zgaruvchisi.
- Bot xatosi navbat yaratilishini buzmaydi — xabarlar navbat (`Channel`) orqali fon'da yuboriladi.

### Fayllar (`backend/KnyazBarber.Api/Services/Telegram/`)
```
../../Configurations/TelegramOptions.cs  BotToken, MiniAppUrl, ReminderLead, ApiBaseUrl (test)
TelegramMessages.cs        matnlar, tugmalar, hisobot, hisobot vaqti — sof funksiyalar (testlangan)
TelegramInitData.cs        Mini App initData HMAC tekshiruvi — sof funksiya (testlangan)
TelegramAccountService.cs  username/ID bog'lash, admin kiritgan qiymatni tekshirish, oluvchilar
TelegramNotifier.cs        Channel navbat (navbat yaratish/bekor qilish kutmaydi)
TelegramNotificationService.cs yangi/bekor xabarlari, eslatma, kechki hisobot
TelegramUpdateHandler.cs   /start va callback tugmalar (ruxsat tekshiruvi)
TelegramWorker.cs          BackgroundService: polling + navbat + har daqiqalik rejalashtiruvchi
```
Auth: `POST /api/auth/telegram` (xodim → token, mijoz → 204). CLI: `set-telegram <login> <@username|ID>`, `create-admin <login>`.
Testlar: `backend/KnyazBarber.Tests/TelegramTests.cs`.
### Baza (1 migratsiya `AddTelegram`)
- `User.TelegramUsername` (string?, unique, kichik harflarda), `User.TelegramUserId` (long?, unique).
- `Appointment.ReminderSentAt`.
- `SiteSettings.LastReportDate`.

### Frontend
- "Foydalanuvchilar" sahifasi: Telegram username/ID maydoni + "Ulangan ✓ / Kutilmoqda" holati.
- `src/lib/telegram.ts`: Mini App aniqlash, avtomatik kirish, Orqaga tugmasi.

## Bosqichlar (hammasi bajarildi — faqat haqiqiy bot bilan qo'lda tekshiruv qoldi)
1. Migratsiya, `TelegramOptions`, token; BotFather'da bot yaratish (foydalanuvchi).
2. Bog'lash: username/ID (panel + CLI) + `/start` → Telegram ID.
3. Yangi navbat / bekor qilish xabarlari + avtomatik tasdiq.
4. Inline tugmalar + ruxsat tekshiruvi.
5. Eslatma + kechki hisobot.
6. Mini App: initData auth, frontend moslash (server HTTPS bilan — tunnel kerak bo'lmadi).
7. Testlar (matnlar, initData HMAC, ruxsat, eslatma tanlash, hisobot vaqti) + haqiqiy bot bilan qo'lda tekshiruv.
