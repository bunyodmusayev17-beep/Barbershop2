import {useContext} from 'react';
import {AuthContext} from '../providers/AuthContext';

export function useAuth() {
  const v = useContext(AuthContext);
  if (!v) throw new Error('useAuth faqat AuthProvider ichida');
  return v;
}
