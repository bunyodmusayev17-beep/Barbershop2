import {useState} from 'react';

/** Ro'yxatni sahifalarga bo'lish. Ro'yxat yoki sahifa hajmi o'zgarsa, sahifa chegaradan chiqmaydi. */
export function usePaged<T>(items: T[], size: number) {
  const pages = Math.max(1, Math.ceil(items.length / size));
  const [page, setPage] = useState(0);
  const current = Math.min(page, pages - 1);
  return {page: current, pages, setPage, items: items.slice(current * size, current * size + size)};
}
