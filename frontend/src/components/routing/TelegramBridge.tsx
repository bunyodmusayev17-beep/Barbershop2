import {useEffect, useRef, useState} from 'react';
import {useLocation, useNavigate} from 'react-router-dom';
import {authApi} from '../../api/auth';
import {useAuth} from '../../hooks/useAuth';
import {initTelegram, inTelegram, tgInitData, webApp} from '../../lib/telegram';

/**
 * Telegram Mini App: xodim bo'lsa parolsiz kirib o'z paneliga o'tadi (mijoz uchun sayt o'zgarmaydi);
 * Telegram'ning "Orqaga" tugmasi sayt ichidagi orqaga o'tish bilan ishlaydi.
 */
export function TelegramBridge() {
  const {user, loading, setSession} = useAuth();
  const navigate = useNavigate();
  const {pathname} = useLocation();
  const tried = useRef(false);

  useEffect(() => {
    if (!inTelegram || !tgInitData || loading || user || tried.current) return;
    tried.current = true;
    authApi
      .telegram(tgInitData)
      .then((res) => {
        if (!res) return;
        setSession(res);
        if (pathname === '/') navigate(res.user.role === 'Admin' ? '/admin' : '/barber', {replace: true});
      })
      .catch(() => undefined);
  }, [loading, user, setSession, navigate, pathname]);

  // Telegram skripti sayt chizilgandan keyin yuklanishi mumkin — Orqaga tugmasi shundan keyin ulanadi
  const [tgReady, setTgReady] = useState(() => !!webApp());
  useEffect(() => {
    if (inTelegram && !tgReady) void initTelegram().then(() => setTgReady(true));
  }, [tgReady]);

  useEffect(() => {
    const back = tgReady ? webApp()?.BackButton : undefined;
    if (!back) return;
    const isRoot = pathname === '/' || pathname === '/admin' || pathname === '/barber';
    if (isRoot) {
      back.hide();
      return;
    }
    const onBack = () => (window.history.length > 1 ? navigate(-1) : navigate('/'));
    back.show();
    back.onClick(onBack);
    return () => back.offClick(onBack);
  }, [pathname, navigate, tgReady]);

  return null;
}
