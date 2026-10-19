// Soxta API — Docker/baza yo'q kompyuterda frontendni Knyaz ma'lumotlari bilan ko'rish uchun.
// Ma'lumotlar backend/KnyazBarber.Api/Data/Seeder.cs dagi namunalar bilan bir xil.
// Ishga tushirish: node scripts/mock-api.mjs  (port 5090), keyin frontend: API_TARGET=http://localhost:5090 npm run dev
// Kirish: istalgan parol bilan admin yoki baxtiyor / sardor / bekzod (usta). Yuklangan rasmlar faqat xotirada turadi.
import {createServer} from 'node:http';

const PORT = Number(process.env.PORT ?? 5090);
const t = (uz, ru, en) => ({uz, ru, en});

const services = [
  [1, t('Soch olish', 'Стрижка', 'Haircut'), t("Klassik yoki zamonaviy soch turmagi, yuvish va ukladka bilan.", 'Классическая или современная стрижка с мытьём и укладкой.', 'Classic or modern haircut with wash and styling.'), 80_000, 45, [1, 2, 3]],
  [2, t('Soqol dizayni', 'Оформление бороды', 'Beard trim'), t("Soqolga shakl berish, konturlash va parvarish.", 'Моделирование, контур и уход за бородой.', 'Shaping, line-up and beard care.'), 50_000, 30, [1, 2, 3]],
  [3, t('Soch + soqol', 'Стрижка + борода', 'Haircut + beard'), t("To'liq obraz: soch olish va soqol dizayni.", 'Полный образ: стрижка и оформление бороды.', 'The full look: haircut and beard trim.'), 120_000, 75, [1, 2, 3]],
  [4, t('Ustara bilan soqol olish', 'Бритьё опасной бритвой', 'Hot towel shave'), t("Issiq sochiq va ustara bilan klassik soqol olish.", 'Классическое бритьё с горячим полотенцем.', 'Classic straight-razor shave with a hot towel.'), 70_000, 40, [1, 2]],
  [5, t('Bolalar soch olishi', 'Детская стрижка', 'Kids haircut'), t("12 yoshgacha bo'lgan bolalar uchun.", 'Для детей до 12 лет.', 'For kids under 12.'), 60_000, 30, [3]],
].map(([id, name, description, price, durationMinutes, barberIds]) => ({id, name, description, price, durationMinutes, barberIds}));

const barbers = [
  [1, 'Baxtiyor', t('Top-barber', 'Топ-барбер', 'Top barber'), t('8 yillik tajriba. Fade va klassik soch turmaklari ustasi.', '8 лет опыта. Мастер фейдов и классических стрижек.', '8 years of experience. Master of fades and classic cuts.'), 1],
  [2, 'Sardor', t('Barber', 'Барбер', 'Barber'), t("Soqol dizayni va ustara bilan ishlash bo'yicha mutaxassis.", 'Специалист по бороде и бритью опасной бритвой.', 'Beard and straight-razor specialist.'), 3],
  [3, 'Bekzod', t('Barber', 'Барбер', 'Barber'), t("Zamonaviy uslublar va bolalar soch olishi.", 'Современные стрижки и детские стрижки.', 'Modern styles and kids haircuts.'), 5],
].map(([id, name, title, bio, dayOff]) => ({
  id, name, title, bio, photoUrl: null, daysOff: [dayOff],
  serviceIds: services.filter((s) => s.barberIds.includes(id)).map((s) => s.id),
}));

const site = {
  settings: {
    businessName: 'Knyaz Barber',
    address: t('Sergeli tumani, Sergeli-V dahasi, 2-uy', 'Сергелийский район, массив Сергели-V, дом 2', 'Sergeli district, Sergeli-V, 2'),
    phone: '+998 90 965 18 77',
    telegram: null,
    instagram: null,
    latitude: 41.223602,
    longitude: 69.209534,
    rating: 5.0,
    ratingCount: 46,
    bookingHorizonDays: 14,
    hours: [0, 1, 2, 3, 4, 5, 6].map((day) => ({day, open: '11:00', close: '23:00'})),
  },
  services,
  barbers,
  reviews: [
    {id: 1, authorName: 'Aziz', rating: 5, text: "Juda zo'r usta, soch aynan men xohlagandek chiqdi. Albatta yana kelaman!", createdAt: '2026-10-01T10:00:00Z'},
    {id: 2, authorName: 'Тимур', rating: 5, text: 'Отличный сервис, приятная атмосфера и мастера своего дела.', createdAt: '2026-10-05T10:00:00Z'},
    {id: 3, authorName: 'Bobur', rating: 5, text: 'Onlayn navbat juda qulay ekan, kutib o\'tirmadim.', createdAt: '2026-10-10T10:00:00Z'},
  ],
  gallery: [],
};

// Rasmlar xotirada: /uploads/<nom> → {type, body}
const files = new Map();
let nextImageId = 1;
const demoWork = (barberId, n, hue) => {
  const name = `demo-${barberId}-${n}.svg`;
  files.set(name, {type: 'image/svg+xml', body: Buffer.from(
    `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 300 300"><rect width="300" height="300" fill="hsl(${hue} 25% 22%)"/><circle cx="150" cy="120" r="60" fill="hsl(${hue} 30% 45%)"/><rect x="70" y="190" width="160" height="80" rx="40" fill="hsl(${hue} 30% 45%)"/><text x="150" y="290" font-family="sans-serif" font-size="18" fill="#f2c14e" text-anchor="middle">Ish ${n}</text></svg>`)});
  site.gallery.push({id: nextImageId++, url: `/uploads/${name}`, caption: t('', '', ''), barberId});
};
[1, 2, 3, 4].forEach((n) => demoWork(1, n, 30 + n * 20));
[1, 2].forEach((n) => demoWork(2, n, 200 + n * 30));

const users = {admin: {id: 1, login: 'admin', role: 'Admin', barberId: null, barberName: null}};
barbers.forEach((b, i) => (users[b.name.toLowerCase()] = {id: i + 2, login: b.name.toLowerCase(), role: 'Barber', barberId: b.id, barberName: b.name}));
const userOf = (req) => users[(req.headers.authorization ?? '').replace('Bearer mock-', '')] ?? null;

const MAX_WORKS = 30;
const profileOf = (barberId) => {
  const b = barbers.find((x) => x.id === barberId);
  const works = site.gallery.filter((g) => g.barberId === barberId).sort((a, c) => c.id - a.id).map((g) => ({id: g.id, url: g.url, createdAt: new Date().toISOString()}));
  return {barberId, name: b.name, photoUrl: b.photoUrl, works, maxWorks: MAX_WORKS};
};

/** multipart/form-data dan fayllarni ajratadi (faqat soxta API uchun yetarli darajada) */
function parseFiles(req, raw) {
  const boundary = /boundary=(.+)$/.exec(req.headers['content-type'] ?? '')?.[1];
  if (!boundary) return [];
  const out = [];
  const sep = Buffer.from(`--${boundary}`);
  let pos = raw.indexOf(sep);
  while (pos !== -1) {
    const next = raw.indexOf(sep, pos + sep.length);
    if (next === -1) break;
    const part = raw.subarray(pos + sep.length + 2, next - 2);
    const headEnd = part.indexOf('\r\n\r\n');
    const head = part.subarray(0, headEnd).toString();
    const type = /Content-Type: (.+)/i.exec(head)?.[1]?.trim();
    if (/filename=/.test(head) && type) out.push({type, body: part.subarray(headEnd + 4)});
    pos = next;
  }
  return out;
}
const store = (f) => {
  const ext = {'image/jpeg': '.jpg', 'image/png': '.png', 'image/webp': '.webp'}[f.type];
  if (!ext) return null;
  const name = `${Date.now()}-${Math.random().toString(36).slice(2)}${ext}`;
  files.set(name, f);
  return `/uploads/${name}`;
};

function slots(url) {
  const serviceId = Number(url.searchParams.get('serviceId'));
  const date = url.searchParams.get('date');
  const barberId = Number(url.searchParams.get('barberId')) || null;
  const day = new Date(`${date}T00:00:00`).getDay();
  const ids = barbers.filter((b) => b.serviceIds.includes(serviceId) && !b.daysOff.includes(day) && (!barberId || b.id === barberId)).map((b) => b.id);
  const list = [];
  for (let m = 11 * 60; m < 22 * 60; m += 30) list.push({time: `${String(m / 60 | 0).padStart(2, '0')}:${String(m % 60).padStart(2, '0')}`, barberIds: ids});
  return {date, slots: ids.length ? list : []};
}

createServer((req, res) => {
  const url = new URL(req.url, 'http://x');
  const send = (code, body) => {
    res.writeHead(code, {'Content-Type': 'application/json'});
    res.end(body === undefined ? '' : JSON.stringify(body));
  };
  if (req.method === 'GET' && url.pathname.startsWith('/uploads/')) {
    const f = files.get(url.pathname.slice('/uploads/'.length));
    if (!f) return send(404, {title: 'Rasm topilmadi'});
    res.writeHead(200, {'Content-Type': f.type});
    return res.end(f.body);
  }
  if (req.method === 'GET' && url.pathname === '/api/public/site') return send(200, site);
  if (req.method === 'POST' && url.pathname === '/api/auth/login') {
    let raw = '';
    req.on('data', (c) => (raw += c));
    req.on('end', () => {
      const u = users[String(JSON.parse(raw || '{}').login ?? '').toLowerCase()];
      if (!u) return send(401, {title: "Login yoki parol noto'g'ri"});
      send(200, {token: `mock-${u.login}`, expiresAt: new Date(Date.now() + 864e5).toISOString(), user: u});
    });
    return;
  }
  if (url.pathname.startsWith('/api/auth/me')) return userOf(req) ? send(200, userOf(req)) : send(401, {title: 'Kirilmagan'});
  if (url.pathname.startsWith('/api/staff/profile')) {
    const u = userOf(req);
    if (!u) return send(401, {title: 'Kirilmagan'});
    if (!u.barberId) return send(403, {title: "Bu bo'lim faqat ustalar uchun"});
    const b = barbers.find((x) => x.id === u.barberId);
    const work = /^\/api\/staff\/profile\/works\/(\d+)$/.exec(url.pathname);
    if (req.method === 'GET') return send(200, profileOf(b.id));
    if (req.method === 'DELETE' && url.pathname.endsWith('/photo')) {
      b.photoUrl = null;
      return send(200, profileOf(b.id));
    }
    if (req.method === 'DELETE' && work) {
      const i = site.gallery.findIndex((g) => g.id === Number(work[1]) && g.barberId === b.id);
      if (i === -1) return send(404, {title: 'Rasm topilmadi'});
      site.gallery.splice(i, 1);
      return send(200, profileOf(b.id));
    }
    const chunks = [];
    req.on('data', (c) => chunks.push(c));
    req.on('end', () => {
      const uploaded = parseFiles(req, Buffer.concat(chunks));
      if (!uploaded.length) return send(400, {title: '1 dan 10 tagacha rasm tanlang'});
      if (url.pathname.endsWith('/photo')) {
        const u2 = store(uploaded[0]);
        if (!u2) return send(400, {title: 'Faqat JPG, PNG yoki WEBP rasm yuklash mumkin'});
        b.photoUrl = u2;
        return send(200, profileOf(b.id));
      }
      const count = site.gallery.filter((g) => g.barberId === b.id).length;
      if (count + uploaded.length > MAX_WORKS) return send(400, {title: `Ko'pi bilan ${MAX_WORKS} ta ish rasmi. Yana ${MAX_WORKS - count} ta yuklash mumkin`});
      for (const f of uploaded) {
        const u3 = store(f);
        if (!u3) return send(400, {title: 'Faqat JPG, PNG yoki WEBP rasm yuklash mumkin'});
        site.gallery.push({id: nextImageId++, url: u3, caption: t('', '', ''), barberId: b.id});
      }
      send(200, profileOf(b.id));
    });
    return;
  }
  if (req.method === 'GET' && url.pathname === '/api/admin/gallery')
    return send(200, site.gallery.map((g, i) => ({...g, sortOrder: i, barberName: barbers.find((b) => b.id === g.barberId)?.name ?? null})));
  // Kabinetning boshqa ro'yxatlari soxta API'da bo'sh
  if (req.method === 'GET' && (url.pathname.startsWith('/api/admin/') || url.pathname.startsWith('/api/staff/'))) return send(200, []);
  if (req.method === 'GET' && url.pathname === '/api/booking/slots') return send(200, slots(url));
  if (req.method === 'POST' && url.pathname === '/api/booking') {
    let raw = '';
    req.on('data', (c) => (raw += c));
    req.on('end', () => {
      const b = JSON.parse(raw || '{}');
      const s = services.find((x) => x.id === b.serviceId) ?? services[0];
      const barber = barbers.find((x) => x.id === b.barberId) ?? barbers[0];
      const [h, m] = String(b.time ?? '11:00').split(':').map(Number);
      const end = h * 60 + m + s.durationMinutes;
      send(200, {id: Date.now() % 100000, barberId: barber.id, barberName: barber.name, serviceId: s.id, date: b.date, time: b.time,
        endTime: `${String(end / 60 | 0).padStart(2, '0')}:${String(end % 60).padStart(2, '0')}`, price: s.price, clientName: b.clientName, clientPhone: b.clientPhone});
    });
    return;
  }
  send(404, {title: 'Soxta API: bu yo\'l yo\'q', path: url.pathname});
}).listen(PORT, () => console.log(`Soxta Knyaz API: http://localhost:${PORT}`));
