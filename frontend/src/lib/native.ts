import {Capacitor} from '@capacitor/core';

/** Android ilova (Capacitor) ichidami. Oddiy brauzer va Telegram Mini App'da — false. */
export const isNativeApp = Capacitor.isNativePlatform();

/**
 * API va yuklangan rasmlar manzili. Saytda — shu domen (bo'sh prefiks); ilovada sayt fayllari telefonda turadi,
 * shuning uchun so'rovlar serverga to'liq manzil bilan boradi.
 */
export const API_BASE = isNativeApp ? (import.meta.env.VITE_API_BASE ?? 'https://knyaz.bunyodmusayev.uz') : '';
