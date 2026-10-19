# Barbershop sayti — PLAN shabloni

> Yangi sartaroshxona uchun: faqat **1-bo'limni** to'ldiring va shu faylni yangi sessiyaga bering.
> Qolgan hammasi standart — Claude o'zgartirmaydi, faqat 1-bo'limdagi qiymatlarni qo'yadi.
> Kod asosi: **Barbershop2** (https://github.com/bunyodmusayev17-beep/Barbershop2) — to'liq nusxa olinadi va nomlar almashtiriladi.

## 1. To'ldiriladi (faqat shu qism)
```
Nomi:            ___________            (masalan: Knyaz Barber)
Kod nomi:        ___________            (PascalCase: KnyazBarber → papka, namespace, docker nomlari)
Manzil:          uz: ___  ru: ___  en: ___
Telefon:         +998 __ ___ __ __
Koordinatalar:   lat ___, lon ___       (Yandex/Google xaritadan)
Reyting:         ___ (___ ta baho)      (Yandex'dan, bo'lmasa bo'sh)
Telegram/Instagram: ___ / ___           (bo'lmasa bo'sh)
Egasi:           ism ___, qaysi tilda yozadi ___
Dizayn:          ___                    (rang, shrift, uslub yoki "variantlar ko'rsat")
Logo:            ___                    (bor rasm yoki "yasab ber")
Domen:           ___.bunyodmusayev.uz
Server porti:    80__                   (band emasligini tekshirish kerak)
Narx taklifi:    obuna ___ so'm/oy · to'liq ___ so'm
Qo'shimcha talab: ___                   (standartdan farqi bo'lsa)
```

## 2. Ishlash qoidalari (Claude uchun)
- Foydalanuvchi bilan **o'zbekcha, norasmiy**. Muhim qarorni o'zi qilmaydi — so'raydi.
- Matnlarda **"navbat"** ("zapis"/"yozilish" emas).
- Loyiha **to'liq mustaqil**: boshqa barbershop loyihasi bilan umumiy kod, sozlama, baza, `UserSecretsId`, port, domen yo'q.
- Serverdagi boshqa saytlarga **tegilmaydi** (faqat o'z papkasi, konteynerlari, porti).
- Token va parollar chatga yozilmaydi — foydalanuvchiga yashirin `read -s` buyrug'i beriladi.
- Lokal kompyuterda Docker yo'q: frontend `scripts/mock-api.mjs` (soxta API) bilan tekshiriladi.
- Har o'zgarishdan keyin: testlar, `tsc`, `eslint`, telefon o'lchamida (375px) tekshirish, commit, push, deploy.

## 3. Standart yechim (o'zgarmaydi)
**Stek:** .NET 10 Web API (Controller → Service → `IBaseRepository<T>`), EF Core + PostgreSQL 17, JWT (Admin/Barber) ·
Vite + React 19 + TS + Tailwind 4 + TanStack Query + i18next (uz/ru/en) + Framer Motion · xUnit · Docker Compose.

**Sayt (3 til):** hero, xizmatlar va narxlar, ustalar (bosilsa — ishlari), sharhlar, galereya (usta bo'yicha filtr), xarita ·
onlayn navbat: xizmat → usta ("istalgan usta") → sana/vaqt → ism/telefon → tasdiq.

**Admin panel (o'zbekcha):** Boshqaruv · Navbatlar · Kalendar (Kecha/Bugun/Ertaga) · Xizmatlar · Ustalar (jadval, rasm, ta'til) ·
Ishlamaydigan kunlar · Galereya · Sharhlar · Foydalanuvchilar (+Telegram) · Sozlamalar.

**Usta kabineti:** o'z navbatlari va holati · Kalendar · "Ishlamayman" (qizil → yashil) · Profilim: o'z rasmi + 30 tagacha ish rasmi.

**Navbat qoidalari:** bo'sh vaqt = jadval − ta'til − navbatlar − o'tgan vaqt; ustma-ust navbat PostgreSQL `EXCLUDE` bilan taqiqlangan;
+998 telefon formati; spamga qarshi limitlar; navbat avtomatik tasdiqlanadi; narx navbatda saqlanadi.

**Telegram bot + Mini App:** mijozga /start — manzil, ish vaqti, reyting, telefon + Navbat olish / Narxlar / Manzil / Qo'ng'iroq;
xodimlarga yangi navbat, Keldi/Kelmadi/Bekor tugmalari, 2 soat oldin eslatma, kechki hisobot; Mini App'dan parolsiz kirish.

**Telefon va tezlik:** blur yo'q, 3D va parallaks faqat kompyuterda, animatsiyalar GPU'da, telefonda ixcham o'lchamlar.

**Server:** 31.220.81.174, `/opt/<kodnomi kichik harfda>`, web `127.0.0.1:<port>`, host nginx + Let's Encrypt, kunlik zaxira 03:30.

## 4. Qadamlar (tartib bilan)
1. **Nusxa va nom almashtirish** — Barbershop2'dan yangi papkaga (`git` tarixisiz), so'ng:
   - `KnyazBarber` / `knyazbarber` / `knyaz` → yangi nom (papkalar, `.slnx`, namespace, `deploy/*`, `docker-compose.yml`, localStorage kalitlari);
   - **yangi `UserSecretsId`** GUID; dev baza porti boshqasi (5435, 5436…);
   - `frontend/src/constants/brand.ts`, `index.html` sarlavhasi, `Seeder.cs` (nom, manzil, telefon, koordinata, reyting);
   - `deploy/server-setup.sh` (DOMAIN), `deploy/nginx-site.conf`, `deploy/docker-compose.yml` (port, MiniAppUrl), README/HANDOFF;
   - eski brend izlari qolmasligini `grep` bilan tekshirish (logo, rang, bot nomi, matnlar).
2. **Dizayn** — kerak bo'lsa variantlar (rasm bilan) ko'rsatib tanlatish; mavzu `index.css` dagi ranglar va `.theme-*` klassida.
3. **Logo, favicon, bot avatari** — variantlar → tanlov → `public/favicon.svg`, PNG'lar, `scripts/make-bot-avatar.mjs`.
4. **Tekshiruv** — `dotnet test`, `tsc`, `eslint`, mock-api bilan brauzerda (kompyuter + 375px).
5. **GitHub** — foydalanuvchi repo ochadi (nomini aytadi), `git init`, commit, push.
6. **Serverga chiqarish** — foydalanuvchi DNS A yozuvi qo'shadi → `./deploy/deploy.sh` → `server-setup.sh` (nginx + HTTPS) →
   foydalanuvchi `create-admin` bilan o'z akkauntini yaratadi.
7. **Namunaviy ma'lumot** (ixtiyoriy) — `seed-demo`; haqiqiy ma'lumot kelganda `clear-demo`.
8. **Telegram bot** — foydalanuvchi BotFather'da ochadi (nom, avatar, about, description) → tokenni serverga yashirin qo'yadi →
   Foydalanuvchilar'da Telegram username → botga /start.
9. **Haqiqiy ma'lumot** egasidan: xizmatlar va narxlar, ustalar (ism, rasm, jadval), ish vaqti, rasmlar → admin paneldan.
10. **Topshirish** — ustalarga akkaunt, egasiga panelni o'rgatish, narx bo'yicha kelishuv.

## 5. Tayyor buyruqlar
| Nima | Buyruq |
|---|---|
| Deploy | Git Bash: `./deploy/deploy.sh` |
| Admin yaratish / parol | `ssh -t root@31.220.81.174 "cd /opt/<nom> && docker compose exec api dotnet <Nom>.Api.dll create-admin <login>"` (`reset-password`) |
| Telegram ulash | `... dotnet <Nom>.Api.dll set-telegram <login> @username` |
| Namunaviy / tozalash | `... seed-demo` · `... clear-demo` ("ha" bilan tasdiq) |
| Bot tokeni | `ssh -t root@31.220.81.174 'read -s -p "Token: " T && echo && echo "TELEGRAM_BOT_TOKEN=$T" >> /opt/<nom>/.env && cd /opt/<nom> && docker compose up -d'` |
| Port bandmi | `ssh root@31.220.81.174 'ss -ltn \| grep 80'` |
