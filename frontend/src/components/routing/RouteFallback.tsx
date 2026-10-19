import {Spinner} from '../ui/Spinner';

/** Sahifa kodi hali yuklanmagan bo'lsa: yuqorida yuguruvchi chiziq + spinner (hech qachon "muzlab" turmaydi) */
export function RouteFallback() {
  return (
    <>
      <div className="route-progress" />
      <div className="flex min-h-[60vh] items-center justify-center">
        <Spinner className="h-8 w-8 text-navy" />
      </div>
    </>
  );
}
