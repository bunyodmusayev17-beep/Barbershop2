import {useCallback, useEffect, useMemo, useState, type ReactNode} from 'react';
import {authApi} from '../api/auth';
import {auth, LOGOUT_EVENT} from '../api/client';
import type {LoginResponse, UserDto} from '../api/types';
import {AuthContext} from './AuthContext';

export function AuthProvider({children}: {children: ReactNode}) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState(() => !!auth.token);

  // Saqlangan token bo'lsa — foydalanuvchini tiklash
  useEffect(() => {
    if (!auth.token) return;
    authApi
      .me()
      .then(setUser)
      .catch(() => (auth.token = null))
      .finally(() => setLoading(false));
  }, []);

  // Har qanday so'rov 401 qaytarsa — chiqish
  useEffect(() => {
    const onLogout = () => setUser(null);
    window.addEventListener(LOGOUT_EVENT, onLogout);
    return () => window.removeEventListener(LOGOUT_EVENT, onLogout);
  }, []);

  const login = useCallback(async (login: string, password: string) => {
    const res = await authApi.login(login, password);
    auth.token = res.token;
    setUser(res.user);
    return res.user;
  }, []);

  const setSession = useCallback((res: LoginResponse) => {
    auth.token = res.token;
    setUser(res.user);
  }, []);

  const logout = useCallback(() => {
    auth.token = null;
    setUser(null);
  }, []);

  const value = useMemo(() => ({user, loading, login, logout, setSession}), [user, loading, login, logout, setSession]);
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
