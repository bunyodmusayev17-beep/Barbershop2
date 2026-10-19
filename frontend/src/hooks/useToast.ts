import {useContext} from 'react';
import {ToastContext} from '../providers/ToastContext';

export const useToast = () => useContext(ToastContext);
