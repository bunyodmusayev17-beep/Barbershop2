import {useEffect} from 'react';
import {useLocation} from 'react-router-dom';
import {useAuth} from '../../hooks/useAuth';
import {preloadPublic, preloadStaff} from '../../lib/pages';

/**
 * Sahifalar kodini oldindan yuklash: mijozlar uchun navbat/login darhol,
 * xodimlar uchun admin bo'limlari — login sahifasiga kirganda yoki tizimga kirgan bo'lsa.
 */
export function Preloader() {
  const {pathname} = useLocation();
  const {user} = useAuth();
  useEffect(() => preloadPublic(), []);
  useEffect(() => {
    if (user || pathname.startsWith('/login') || pathname.startsWith('/admin') || pathname.startsWith('/barber')) preloadStaff();
  }, [user, pathname]);
  return null;
}
