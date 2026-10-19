import {useQuery} from '@tanstack/react-query';
import {publicApi} from '../api/public';

export function useSite() {
  return useQuery({queryKey: ['site'], queryFn: publicApi.site, staleTime: 60_000});
}
