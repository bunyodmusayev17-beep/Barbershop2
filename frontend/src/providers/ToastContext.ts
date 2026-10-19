import {createContext} from 'react';

export type ToastKind = 'ok' | 'error';

/** Bildirishnoma chiqarish: push("Saqlandi") yoki push("Xato", "error") */
export const ToastContext = createContext<(text: string, kind?: ToastKind) => void>(() => {});
