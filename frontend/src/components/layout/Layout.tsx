import {Suspense, useEffect, useState} from 'react';
import {Link, NavLink, Outlet, useLocation} from 'react-router-dom';
import {useLang} from '../../i18n';
import {useSite} from '../../hooks/useSite';
import {LangSwitch} from './LangSwitch';
import {BarberPole} from '../ui/BarberPole';
import {ButtonLink} from '../ui/Button';
import {Spinner} from '../ui/Spinner';

/** Ichki sahifa kodi yuklanayotganda: header/footer joyida qoladi, faqat kontent o'rnida chiziq + spinner */
function RouteFallback() {
  return (
    <>
      <div className="route-progress" />
      <div className="flex min-h-[60vh] items-center justify-center">
        <Spinner className="h-8 w-8 text-navy" />
      </div>
    </>
  );
}
import {cn} from '../../lib/cn';
import {BRAND} from '../../constants/brand';
import {Crown} from '../ui/Crown';

const SECTIONS = ['services', 'barbers', 'reviews', 'contacts'] as const;

function Logo() {
  return (
    <Link to="/" className="flex flex-col items-center leading-none" aria-label={BRAND.name}>
      <Crown className="mb-0.5 w-4 text-cherry md:mb-1 md:w-5" />
      <span className="font-deco text-navy text-xl md:text-2xl">{BRAND.title}</span>
      <span className="font-deco text-cherry text-[10px] !tracking-[0.5em] pl-[0.5em]">{BRAND.subtitle}</span>
    </Link>
  );
}

export function Layout() {
  const {t} = useLang();
  const [open, setOpen] = useState(false);
  const {pathname, hash} = useLocation();
  const onBooking = pathname.startsWith('/book');

  // Sahifa tagidagi fon ham kigiz rangida (scroll oxirida oq chiziq ko'rinmasin)
  useEffect(() => {
    document.body.classList.add('lb-body');
    return () => document.body.classList.remove('lb-body');
  }, []);

  // Boshqa sahifaga/bo'limga o'tilganda mobil menyu yopiladi
  useEffect(() => setOpen(false), [pathname, hash]);

  return (
    <div className="theme-letterboard min-h-screen flex flex-col">
      <header className="sticky top-0 z-40 bg-cream border-b-2 border-navy">
        <BarberPole className="!h-2" />
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-2 md:py-3">
          <Logo />
          <nav className="hidden md:flex items-center gap-6 text-sm font-semibold tracking-widest uppercase">
            {SECTIONS.map((s) => (
              <a key={s} href={`/#${s}`} className="hover:text-cherry transition">
                {t(`nav.${s}`)}
              </a>
            ))}
          </nav>
          <div className="flex items-center gap-3">
            {/* hidden va inline-flex bitta elementda to'qnashmasligi uchun alohida o'ramda */}
            <div className="hidden sm:block">
              <LangSwitch />
            </div>
            {!onBooking ? (
              <div className="hidden sm:block">
                <ButtonLink to="/book" className="!px-4 !py-2">
                  {t('nav.book')}
                </ButtonLink>
              </div>
            ) : null}
            <button
              type="button"
              className="md:hidden rounded-full border-2 border-navy w-9 h-9 flex items-center justify-center"
              aria-label="Menu"
              aria-expanded={open}
              onClick={() => setOpen((o) => !o)}
            >
              <span aria-hidden className="text-lg">{open ? '✕' : '☰'}</span>
            </button>
          </div>
        </div>
        {open ? (
          <div className="md:hidden border-t-2 border-navy px-4 py-4 flex flex-col gap-3">
            {SECTIONS.map((s) => (
              <a key={s} href={`/#${s}`} onClick={() => setOpen(false)} className="py-1 font-semibold tracking-widest uppercase">
                {t(`nav.${s}`)}
              </a>
            ))}
            <div className="flex items-center justify-between pt-2">
              <LangSwitch />
              <ButtonLink to="/book" className="!px-4 !py-2">
                {t('nav.book')}
              </ButtonLink>
            </div>
          </div>
        ) : null}
      </header>

      {/* key — har sahifa almashganda ochilish animatsiyasi qayta ishlaydi */}
      <main key={pathname} className="page-enter flex-1">
        <Suspense fallback={<RouteFallback />}>
          <Outlet />
        </Suspense>
      </main>

      <Footer />
    </div>
  );
}

function Footer() {
  const {t, pick} = useLang();
  const {data} = useSite();
  const s = data?.settings;
  return (
    <footer className="band-dark bg-navy text-cream mt-20">
      <BarberPole />
      <div className="mx-auto grid max-w-6xl gap-10 px-4 py-12 md:grid-cols-3">
        <div>
          <Crown className="mb-2 w-8 text-cherry" />
          <p className="font-deco text-4xl text-cream">{BRAND.title}</p>
          <p className="font-deco text-xs !tracking-[0.5em] text-cherry">{BRAND.subtitle}</p>
          {s ? <p className="mt-4 text-sm opacity-80">{pick(s.address)}</p> : null}
        </div>
        <div>
          <p className="mb-3 text-xs font-bold tracking-[0.3em] uppercase text-cherry">{t('footer.hours')}</p>
          <ul className="space-y-1 text-sm">
            {s?.hours
              .slice()
              .sort((a, b) => ((a.day + 6) % 7) - ((b.day + 6) % 7))
              .map((h) => (
                <li key={h.day} className="flex justify-between gap-4 max-w-60">
                  <span className="opacity-80">{(t('days', {returnObjects: true}) as string[])[h.day]}</span>
                  <span>{h.open ? `${h.open}–${h.close}` : t('common.closed')}</span>
                </li>
              ))}
          </ul>
        </div>
        <div>
          <p className="mb-3 text-xs font-bold tracking-[0.3em] uppercase text-cherry">{t('footer.contacts')}</p>
          {s ? (
            <ul className="space-y-2 text-sm">
              <li>
                <a href={`tel:${s.phone.replace(/\s/g, '')}`} className="hover:text-cherry">{s.phone}</a>
              </li>
              {s.telegram ? (
                <li>
                  <a href={`https://t.me/${s.telegram}`} target="_blank" rel="noreferrer" className="hover:text-cherry">
                    Telegram: @{s.telegram}
                  </a>
                </li>
              ) : null}
              {s.instagram ? (
                <li>
                  <a href={`https://instagram.com/${s.instagram}`} target="_blank" rel="noreferrer" className="hover:text-cherry">
                    Instagram: @{s.instagram}
                  </a>
                </li>
              ) : null}
            </ul>
          ) : null}
        </div>
      </div>
      <div className="border-t border-cream/20">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-2 px-4 py-4 text-xs opacity-70">
          <span>© {new Date().getFullYear()} {BRAND.name} · {t('footer.rights')}</span>
          <span className="flex gap-4">
            <NavLink to="/privacy" className={({isActive}) => cn('hover:text-cherry', isActive && 'text-cherry')}>
              {t('footer.privacy')}
            </NavLink>
            <NavLink to="/login" className={({isActive}) => cn('hover:text-cherry', isActive && 'text-cherry')}>
              {t('footer.staff')}
            </NavLink>
          </span>
        </div>
      </div>
    </footer>
  );
}
