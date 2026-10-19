import {createContext} from 'react';
import type {LoginResponse, UserDto} from '../api/types';

export type AuthState = {
  user: UserDto | null;
  loading: boolean;
  login: (login: string, password: string) => Promise<UserDto>;
  logout: () => void;
  /** Tayyor sessiyani o'rnatish (Mini App'dan parolsiz kirish). */
  setSession: (res: LoginResponse) => void;
};

export const AuthContext = createContext<AuthState | null>(null);
