// Knyaz Admin — Android ilova (TWA: sayt ilova ichida, manzil satrisiz). Admin panel va usta kabineti.
// Talab: D:/Coding/android-tools ichida jdk/, sdk/ (build-tools 36.1.0, platform), bw/ (npm @bubblewrap/core).
// Ishga tushirish: node scripts/build-admin-apk.cjs [versionCode]
//   → android/knyaz-admin.apk va frontend/public/.well-known/assetlinks.json (keyin saytni deploy qilish kerak).
// Imzo kaliti loyihadan TASHQARIDA: D:/Coding/android-tools/keys — yo'qotilsa, ilovani yangilab bo'lmaydi (zaxiralang!).
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

const TOOLS = process.env.ANDROID_TOOLS ?? 'D:/Coding/android-tools';
const {Config, JdkHelper, AndroidSdkTools, TwaManifest, TwaGenerator, GradleWrapper, KeyTool, DigitalAssetLinks, ConsoleLog} =
  require(path.join(TOOLS, 'bw/node_modules/@bubblewrap/core'));

const ROOT = path.resolve(__dirname, '..');
const HOST = 'knyaz.bunyodmusayev.uz';
const PACKAGE_ID = 'uz.bunyodmusayev.knyaz.admin';
const PROJECT = path.join(ROOT, 'android');
const KEYS = path.join(TOOLS, 'keys');
const KEYSTORE = path.join(KEYS, 'knyaz-admin.keystore');
const PASSWORD_FILE = path.join(KEYS, 'knyaz-admin.password.txt');
const ALIAS = 'knyaz-admin';
const VERSION_CODE = Number(process.argv[2] ?? 1);

async function main() {
  const log = new ConsoleLog('knyaz-apk');
  const jdkDir = path.join(TOOLS, 'jdk', fs.readdirSync(path.join(TOOLS, 'jdk')).find((d) => d.startsWith('jdk')));
  // keytool/java bubblewrap ichida PATH orqali chaqiriladi — JDK'ni birinchi o'ringa qo'yamiz
  process.env.JAVA_HOME = jdkDir;
  process.env.PATH = `${path.join(jdkDir, 'bin')}${path.delimiter}${process.env.PATH}`;
  const config = new Config(jdkDir, path.join(TOOLS, 'sdk'));
  const jdkHelper = new JdkHelper(process, config);
  const sdk = new AndroidSdkTools(process, config, jdkHelper, log);
  const keyTool = new KeyTool(jdkHelper, log);

  // 1) Imzo kaliti: birinchi marta yaratiladi, parol tasodifiy va faqat keys/ papkada
  fs.mkdirSync(KEYS, {recursive: true});
  if (!fs.existsSync(PASSWORD_FILE)) fs.writeFileSync(PASSWORD_FILE, crypto.randomBytes(18).toString('base64url'), {mode: 0o600});
  const password = fs.readFileSync(PASSWORD_FILE, 'utf8').trim();
  const key = {path: KEYSTORE, alias: ALIAS, password, keypassword: password};
  if (!fs.existsSync(KEYSTORE)) {
    await keyTool.createSigningKey({...key, fullName: 'Knyaz Barber', organizationalUnit: 'Admin', organization: 'Knyaz Barber', country: 'UZ'});
  }
  const sha256 = (await keyTool.keyInfo(key)).fingerprints.get('SHA256');

  // 2) TWA loyihasi — sayt manifestidan (frontend/public/admin.webmanifest)
  const webManifest = JSON.parse(fs.readFileSync(path.join(ROOT, 'frontend/public/admin.webmanifest'), 'utf8'));
  const twa = TwaManifest.fromWebManifestJson(new URL(`https://${HOST}/admin.webmanifest`), webManifest);
  Object.assign(twa, {
    packageId: PACKAGE_ID,
    name: 'Knyaz Barber — Admin',
    launcherName: 'Knyaz Admin',
    startUrl: '/admin',
    appVersionCode: VERSION_CODE,
    appVersionName: `1.0.${VERSION_CODE}`,
    signingKey: {path: KEYSTORE, alias: ALIAS},
    fallbackType: 'customtabs',
    // androidbrowserhelper 2.7+ Android 7 (API 24) dan past versiyani qo'llamaydi
    minSdkVersion: 24,
    enableNotifications: false,
    fingerprints: [{value: sha256, name: 'release'}],
  });
  // Papkaning o'zi emas, ichi tozalanadi (Windows'da papka ochiq turgan bo'lsa EBUSY bermasin)
  fs.mkdirSync(PROJECT, {recursive: true});
  for (const f of fs.readdirSync(PROJECT)) fs.rmSync(path.join(PROJECT, f), {recursive: true, force: true});
  await new TwaGenerator().createTwaProject(PROJECT, twa, log);
  await twa.saveToFile(path.join(PROJECT, 'twa-manifest.json'));

  // 3) Yig'ish, tekislash, imzolash. Bubblewrap "gradlew.bat" ni yo'lsiz chaqiradi — Windows uni
  // joriy papkadan qidirmasligi mumkin (NoDefaultCurrentDirectoryInExePath), shuning uchun PATH'ga qo'shamiz
  process.env.PATH = `${PROJECT}${path.delimiter}${process.env.PATH}`;
  await new GradleWrapper(process, sdk, PROJECT).assembleRelease();
  const unsigned = path.join(PROJECT, 'app/build/outputs/apk/release/app-release-unsigned.apk');
  const aligned = path.join(PROJECT, 'app-release-aligned.apk');
  const signed = path.join(PROJECT, 'knyaz-admin.apk');
  await sdk.zipalign(unsigned, aligned);
  await sdk.apksigner(KEYSTORE, password, ALIAS, password, aligned, signed);
  fs.rmSync(aligned, {force: true});

  // 4) Sayt tomoni: ilova shu saytniki ekanini tasdiqlovchi fayl (manzil satri ko'rinmasligi uchun)
  const wellKnown = path.join(ROOT, 'frontend/public/.well-known');
  fs.mkdirSync(wellKnown, {recursive: true});
  fs.writeFileSync(path.join(wellKnown, 'assetlinks.json'), DigitalAssetLinks.generateAssetLinks(PACKAGE_ID, sha256) + '\n');

  console.log(`\nTayyor: ${signed}\nSHA256: ${sha256}\nEndi saytni deploy qiling (assetlinks.json uchun): ./deploy/deploy.sh`);
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});
