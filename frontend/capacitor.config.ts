import type {CapacitorConfig} from '@capacitor/cli';

/**
 * Android ilova "Knyaz Barber": sayt (dist/) ilova ichiga joylanadi, API va rasmlar — https://knyaz.bunyodmusayev.uz.
 * Yig'ish (Git Bash): ./scripts/build-android.sh <versionCode> → android-release/*.apk va *.aab.
 */
const config: CapacitorConfig = {
  appId: 'uz.bunyodmusayev.knyazbarber',
  appName: 'Knyaz Barber',
  webDir: 'dist',
  backgroundColor: '#1b1b1a',
  android: {
    // Saytdagi https://knyaz... bilan bir xil sxema — localStorage, fetch va CORS uchun (origin: https://localhost)
    allowMixedContent: false,
  },
  plugins: {
    SplashScreen: {
      launchAutoHide: false, // ilova chizilgach nativeApp.ts yopadi
      backgroundColor: '#1b1b1a',
      showSpinner: false,
    },
    SystemBars: {
      insetsHandling: 'native', // kontent status bar va pastki panel ostiga tushmaydi
      style: 'DARK', // qora fonda oq belgilar
    },
  },
};

export default config;
