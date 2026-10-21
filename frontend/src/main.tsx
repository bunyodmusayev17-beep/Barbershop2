import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import './i18n'
import App from './App.tsx'
import {installPressFx} from './lib/pressFx'
import {initTelegram} from './lib/telegram'
import {isNativeApp} from './lib/native'

installPressFx()

// Telegram Mini App bo'lsa — Telegram skripti parallel yuklanadi, sayt uni kutmaydi
void initTelegram()

// Android ilova (Capacitor): ilovaga xos sozlamalar alohida bo'lakda — saytga yuklanmaydi
if (isNativeApp) void import('./lib/nativeApp').then((m) => m.initNativeApp())

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
