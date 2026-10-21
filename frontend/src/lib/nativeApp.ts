import {App} from '@capacitor/app';
import {SplashScreen} from '@capacitor/splash-screen';

/**
 * Faqat Android ilovada (main.tsx dinamik yuklaydi — saytga qo'shilmaydi):
 * ilovadek xatti-harakat — kattalashtirish yo'q, tortib cho'zish yo'q, "Orqaga" tugmasi sahifalar bo'ylab.
 * Status bar rangi va chetlar capacitor.config.ts → SystemBars da.
 */
export async function initNativeApp() {
  document.documentElement.classList.add('native-app');
  document.querySelector('meta[name="viewport"]')
    ?.setAttribute('content', 'width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no');

  // Android "Orqaga": sahifa tarixida orqaga, boshida bo'lsa — ilovadan chiqish
  void App.addListener('backButton', ({canGoBack}) => {
    if (canGoBack) window.history.back();
    else void App.exitApp();
  });

  // Sahifa chizilgach ochilish ekrani yopiladi
  requestAnimationFrame(() => void SplashScreen.hide());
}
