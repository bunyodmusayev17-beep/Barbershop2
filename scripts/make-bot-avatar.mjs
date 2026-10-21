// Telegram bot avatari (640×640 PNG): qora kigiz, oltin halqa, toj + K — favicon.svg bilan bir xil belgi.
// Telegram avatarni doira qilib kesadi, shuning uchun belgi doira ichida joylashgan.
// Ishga tushirish: node scripts/make-bot-avatar.mjs  →  branding/telegram-bot-avatar.png
// Boshqa o'lcham: node scripts/make-bot-avatar.mjs 512 frontend/public/icons/admin-512.png  (ilova ikonkasi)
// Rejim (4-argument): full (standart) · foreground — shaffof fonda faqat belgi (Android moslashuvchan ikonka) ·
//   splash — kigiz fonda kichik belgi, halqasiz (ilova ochilish ekrani)
import {writeFileSync} from 'node:fs';
import {deflateSync} from 'node:zlib';

const N = Number(process.argv[2] ?? 640);
const K = N / 640; // 640 px dagi chizma o'lchamlari shu koeffitsient bilan
const MODE = process.argv[4] ?? 'full';
const ALPHA = MODE === 'foreground';
const NONE = [0, 0, 0, 0];
const SS = 4; // har piksel uchun 4×4 namuna — silliq qirralar
const GOLD = [242, 193, 78], WHITE = [242, 240, 234], FELT = [27, 27, 26];

const inPoly = (x, y, pts) => {
  let inside = false;
  for (let i = 0, j = pts.length - 1; i < pts.length; j = i++) {
    const [xi, yi] = pts[i], [xj, yj] = pts[j];
    if (yi > y !== yj > y && x < ((xj - xi) * (y - yi)) / (yj - yi) + xi) inside = !inside;
  }
  return inside;
};
const crown = [[34.6, 25.8], [34.6, 11.7], [42.3, 18.1], [50, 7.8], [57.7, 18.1], [65.4, 11.7], [65.4, 25.8]];
const kArm = [[40, 56], [58, 20], [74, 20], [52, 58]];
const kLeg = [[47, 46], [75, 80], [58, 80], [40, 60]];
// Belgi o'lchami: moslashuvchan ikonkada xavfsiz zona 66/108, ochilish ekranida — kichik
const scale = (N / 100) * (MODE === 'foreground' ? 0.5 : MODE === 'splash' ? 0.3 : 0.74);

/** Bitta namuna nuqtasining rangi */
function sample(px, py) {
  const u = (px - N / 2) / scale + 50, v = (py - N / 2) / scale + 46;
  if (inPoly(u, v, crown)) return GOLD;
  const kx = (u - 10) / 0.8, ky = (v - 20) / 0.8;
  if ((kx >= 27 && kx <= 42 && ky >= 20 && ky <= 80) || inPoly(kx, ky, kArm) || inPoly(kx, ky, kLeg)) return WHITE;
  if (ALPHA) return NONE;
  const r = Math.hypot(px - N / 2, py - N / 2);
  if (MODE === 'full' && Math.abs(r - (N / 2 - 34 * K)) <= 3 * K) return GOLD.map((c, i) => Math.round(c * 0.55 + FELT[i] * 0.45));
  const row = Math.floor(py / K) % 14;
  if (row >= 10 && row < 13) return FELT.map((c) => Math.round(c * 0.55)); // ariqcha
  if (row === 13) return FELT.map((c) => c + 8);
  return FELT;
}

const CH = ALPHA ? 4 : 3; // RGBA (shaffof) yoki RGB
const raw = Buffer.alloc(N * (N * CH + 1));
for (let y = 0; y < N; y++) {
  raw[y * (N * CH + 1)] = 0;
  for (let x = 0; x < N; x++) {
    const acc = [0, 0, 0, 0];
    for (let sy = 0; sy < SS; sy++)
      for (let sx = 0; sx < SS; sx++) {
        const c = sample(x + (sx + 0.5) / SS, y + (sy + 0.5) / SS);
        const a = c[3] ?? 255;
        // Shaffof chetlarda rang alfa bilan o'rtachalanadi (qora hoshiya chiqmasin)
        acc[0] += c[0] * a; acc[1] += c[1] * a; acc[2] += c[2] * a; acc[3] += a;
      }
    const o = y * (N * CH + 1) + 1 + x * CH;
    for (let i = 0; i < 3; i++) raw[o + i] = acc[3] ? Math.round(acc[i] / acc[3]) : 0;
    if (ALPHA) raw[o + 3] = Math.round(acc[3] / (SS * SS));
  }
}

const crcTable = Array.from({length: 256}, (_, n) => {
  let c = n;
  for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
  return c >>> 0;
});
const crc = (buf) => {
  let c = 0xffffffff;
  for (const b of buf) c = crcTable[(c ^ b) & 0xff] ^ (c >>> 8);
  return (c ^ 0xffffffff) >>> 0;
};
const chunk = (type, data) => {
  const len = Buffer.alloc(4);
  len.writeUInt32BE(data.length);
  const body = Buffer.concat([Buffer.from(type), data]);
  const sum = Buffer.alloc(4);
  sum.writeUInt32BE(crc(body));
  return Buffer.concat([len, body, sum]);
};
const ihdr = Buffer.alloc(13);
ihdr.writeUInt32BE(N, 0);
ihdr.writeUInt32BE(N, 4);
ihdr[8] = 8; // 8 bit
ihdr[9] = ALPHA ? 6 : 2; // RGBA yoki RGB
const png = Buffer.concat([
  Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
  chunk('IHDR', ihdr),
  chunk('IDAT', deflateSync(raw, {level: 9})),
  chunk('IEND', Buffer.alloc(0)),
]);
const out = process.argv[3] ?? new URL('../branding/telegram-bot-avatar.png', import.meta.url);
writeFileSync(out, png);
console.log(`Yozildi: ${out} (${N}×${N}, ${Math.round(png.length / 1024)} KB)`);
