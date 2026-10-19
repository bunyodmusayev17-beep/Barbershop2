import {motion} from 'framer-motion';
import {useCallback, useState, type ReactNode} from 'react';
import type {PublicBarber, PublicImage, PublicSettings, SiteResponse} from '../api/types';
import {BarberPole} from '../components/ui/BarberPole';
import {ButtonLink} from '../components/ui/Button';
import {Card} from '../components/ui/Card';
import {Monogram} from '../components/ui/Monogram';
import {Ornament} from '../components/ui/Ornament';
import {SectionTitle} from '../components/ui/SectionTitle';
import {Spinner} from '../components/ui/Spinner';
import {Stars} from '../components/ui/Stars';
import {HeroPole} from '../components/fx/HeroPole';
import {CombIcon, FloatingDecor, Marquee, RazorIcon, ScissorsIcon, SplitLetters, TiltCard} from '../components/fx/effects';
import {useLang} from '../i18n';
import {useSite} from '../hooks/useSite';
import {Pager} from '../components/ui/Pager';
import {useMedia} from '../hooks/useMedia';
import {usePaged} from '../hooks/usePaged';
import {BRAND} from '../constants/brand';
import {Modal} from '../components/admin/Modal';
import {cn} from '../lib/cn';

function Reveal({children, delay = 0, className}: {children: ReactNode; delay?: number; className?: string}) {
  return (
    <motion.div
      className={className}
      initial={{opacity: 0, y: 24}}
      whileInView={{opacity: 1, y: 0}}
      viewport={{once: true, margin: '-60px'}}
      transition={{duration: 0.5, delay}}
    >
      {children}
    </motion.div>
  );
}

/** Umumiy ish vaqti: hamma kun bir xil bo'lsa "Har kuni 11:00–23:00" */
function hoursLabel(s: PublicSettings, everyDay: string) {
  const open = s.hours.filter((h) => h.open);
  if (open.length === 7 && open.every((h) => h.open === open[0].open && h.close === open[0].close))
    return `${everyDay} ${open[0].open}–${open[0].close}`;
  return open.length ? `${open[0].open}–${open[0].close}` : '';
}

export default function HomePage() {
  const {data, isLoading, isError} = useSite();
  if (isLoading)
    return (
      <div className="flex min-h-[60vh] items-center justify-center text-navy">
        <Spinner className="h-8 w-8" />
      </div>
    );
  if (isError || !data) return <p className="py-20 text-center">⚠</p>;

  return (
    <>
      <Hero data={data} />
      <ServicesMarquee data={data} />
      <Services data={data} />
      <Barbers data={data} />
      <Reviews data={data} />
      <Gallery data={data} />
      <Location data={data} />
      <CtaBand />
    </>
  );
}

function Hero({data}: {data: SiteResponse}) {
  const {t} = useLang();
  const s = data.settings;
  return (
    <section className="relative overflow-hidden">
      <FloatingDecor className="top-10 left-[4%] text-navy/10" speed={60} rotate={30}>
        <ScissorsIcon className="h-20 w-20 md:h-28 md:w-28" />
      </FloatingDecor>
      <FloatingDecor className="right-[40%] bottom-16 hidden text-cherry/10 md:block" speed={90} rotate={-20}>
        <CombIcon className="h-24 w-24" />
      </FloatingDecor>
      {/* Pole doim sarlavhaning o'ng tomonida. Telefonda tugmalar pastda to'liq kenglikda, kompyuterda — matn ostida */}
      <div className="mx-auto grid max-w-6xl grid-cols-[minmax(0,1fr)_104px] items-center gap-x-3 gap-y-6 px-4 py-8 [grid-template-areas:'text_pole'_'cta_cta'] md:grid-cols-[1.4fr_1fr] md:gap-x-10 md:gap-y-8 md:py-20 md:[grid-template-areas:'text_pole'_'cta_pole']">
        <motion.div initial={{opacity: 0, y: 20}} animate={{opacity: 1, y: 0}} transition={{duration: 0.6}} className="relative z-10 [grid-area:text] md:self-end">
          <Ornament className="justify-start text-[10px] tracking-[0.3em] text-navy md:text-xs md:tracking-[0.35em]">{t('hero.est')}</Ornament>
          <h1 className="mt-4 font-deco text-cherry text-5xl leading-[0.9] sm:text-7xl md:mt-5 md:text-9xl">
            <SplitLetters text={BRAND.title} delay={0.15} />
          </h1>
          <p className="mt-2 font-deco text-navy text-base tracking-[0.45em] sm:text-2xl md:mt-3 md:text-3xl md:tracking-[0.55em]">{BRAND.subtitle}</p>
          <div className="my-4 h-0.5 w-24 bg-navy md:my-6 md:w-40" />
          <p className="md:text-lg">{t('hero.tagline')}</p>
          <p className="mt-1 text-xs opacity-80 md:text-sm">{hoursLabel(s, t('hero.everyDay'))}</p>
          <div className="mt-3 flex flex-wrap items-center gap-x-2 gap-y-1 md:mt-5">
            <Stars value={s.rating} className="md:text-xl" />
            <span className="font-bold">{s.rating.toFixed(1)}</span>
            <span className="text-xs opacity-70 md:text-sm">· {t('hero.ratingCount', {count: s.ratingCount})}</span>
          </div>
        </motion.div>

        <motion.div
          initial={{opacity: 0, y: 20}}
          animate={{opacity: 1, y: 0}}
          transition={{duration: 0.6, delay: 0.1}}
          className="relative z-10 flex flex-col gap-3 [grid-area:cta] sm:flex-row md:gap-4 md:self-start"
        >
          <ButtonLink to="/book">{t('hero.cta')} ★</ButtonLink>
          <ButtonLink to="/#services" variant="outline">
            {t('hero.ctaSecondary')}
          </ButtonLink>
        </motion.div>

        <motion.div
          initial={{opacity: 0, scale: 0.9}}
          animate={{opacity: 1, scale: 1}}
          transition={{duration: 0.7, delay: 0.15}}
          className="[grid-area:pole]"
        >
          <HeroPole className="mx-auto h-[280px] w-full max-md:scale-[0.82] md:h-[460px] md:max-w-[360px]" />
        </motion.div>
      </div>
      <BarberPole />
    </section>
  );
}

/** Hero ostidagi cheksiz lenta: xizmat nomlari */
function ServicesMarquee({data}: {data: SiteResponse}) {
  const {pick} = useLang();
  const items = [BRAND.name, ...data.services.map((s) => pick(s.name))];
  return <Marquee items={items} className="band-dark border-b-2 border-navy bg-navy py-3 font-deco text-2xl text-cream md:text-3xl" />;
}

function Services({data}: {data: SiteResponse}) {
  const {t, pick, money} = useLang();
  return (
    <section className="mx-auto max-w-6xl px-4 pt-12 md:pt-20">
      <SectionTitle id="services" title={t('sections.services')} sub={t('sections.servicesSub')} />
      <div className="mx-auto grid max-w-5xl gap-3 md:grid-cols-2">
        {data.services.map((s, i) => (
          <Reveal key={s.id} delay={i * 0.05}>
            <TiltCard max={5}>
            {/* Telefonda ixcham: tavsif 2 qator, tugma o'ngda; kattaroq ekranda — to'liq */}
            <Card className="flex h-full flex-row items-center gap-3 p-3 sm:px-4 sm:py-3">
              <div className="min-w-0 flex-1">
                <div className="flex items-baseline justify-between gap-2 sm:gap-3">
                  <h3 className="font-bold sm:text-lg">{pick(s.name)}</h3>
                  <span className="whitespace-nowrap text-sm font-bold text-cherry sm:text-base">{money(s.price)}</span>
                </div>
                <p className="mt-0.5 line-clamp-2 text-xs opacity-80 sm:mt-1 sm:line-clamp-none sm:text-sm">{pick(s.description)}</p>
                <p className="mt-1 text-[10px] font-semibold tracking-widest uppercase opacity-60 sm:text-[11px]">⏱ {t('common.minutes', {count: s.durationMinutes})}</p>
              </div>
              <ButtonLink to={`/book?service=${s.id}`} variant="outline" className="shrink-0 !px-3 !py-1.5 !text-[11px] !tracking-[0.1em] sm:!px-4 sm:!py-2 sm:!text-sm sm:!tracking-[0.15em]">
                {t('common.bookThis')}
              </ButtonLink>
            </Card>
            </TiltCard>
          </Reveal>
        ))}
      </div>
    </section>
  );
}

function Barbers({data}: {data: SiteResponse}) {
  const {t, pick} = useLang();
  const days = t('daysShort', {returnObjects: true}) as string[];
  // Kompyuterda 2 qator × 3, telefon/planshetda 2 qator × 2 (yoki 4 ta ustma-ust)
  const wide = useMedia('(min-width: 1024px)');
  const paged = usePaged(data.barbers, wide ? 6 : 4);
  const [shown, setShown] = useState<PublicBarber | null>(null);
  const worksOf = (id: number) => data.gallery.filter((g) => g.barberId === id);
  const go = (p: number) => {
    paged.setPage(p);
    document.getElementById('barbers')?.scrollIntoView({behavior: 'smooth', block: 'start'});
  };
  return (
    <section className="mx-auto max-w-6xl px-4 pt-14 md:pt-24">
      <SectionTitle id="barbers" title={t('sections.barbers')} sub={t('sections.barbersSub')} />
      {/* Telefonda 2 ustunli ixcham kartochkalar, kattaroq ekranda — avvalgidek */}
      <div className="mx-auto grid max-w-4xl grid-cols-2 gap-3 sm:gap-4 lg:grid-cols-3">
        {paged.items.map((b, i) => (
          <Reveal key={b.id} delay={i * 0.08}>
            <TiltCard>
            <Card className="flex h-full flex-col overflow-hidden text-center">
              {/* Rasm bosilsa — usta haqida va uning ishlari oynasi */}
              <button type="button" onClick={() => setShown(b)} aria-label={b.name} className="relative block w-full bg-cream-dark px-3 pt-4 pb-3 sm:px-5 sm:pt-5 sm:pb-4">
                {b.photoUrl ? (
                  <img src={b.photoUrl} alt={b.name} className="mx-auto h-16 w-16 rounded-full border-2 border-cherry object-cover sm:h-24 sm:w-24 sm:border-[3px]" />
                ) : (
                  <Monogram name={b.name} className="mx-auto h-16 w-16 text-3xl sm:h-24 sm:w-24 sm:text-5xl" />
                )}
              </button>
              <div className="flex flex-1 flex-col p-3 sm:p-4">
                <h3 className="font-deco text-xl text-navy sm:text-3xl">{b.name}</h3>
                <p className="mt-1 text-[9px] font-bold tracking-[0.2em] uppercase text-cherry sm:text-xs sm:tracking-[0.3em]">{pick(b.title)}</p>
                <p className="mt-1.5 line-clamp-2 text-xs opacity-80 sm:mt-2 sm:line-clamp-none sm:text-sm">{pick(b.bio)}</p>
                {b.daysOff.length ? (
                  <p className="mt-2 text-[10px] opacity-60 sm:mt-3 sm:text-xs">
                    {t('common.dayOff')}: {b.daysOff.map((d) => days[d]).join(', ')}
                  </p>
                ) : null}
                {worksOf(b.id).length ? (
                  <button type="button" onClick={() => setShown(b)} className="mt-2 text-[11px] font-bold tracking-[0.15em] uppercase text-cherry underline-offset-4 hover:underline sm:text-xs">
                    {t('works.view', {count: worksOf(b.id).length})}
                  </button>
                ) : null}
                <div className="mt-auto pt-3 sm:pt-4">
                  <ButtonLink to={`/book?barber=${b.id}`} variant="outline" className="!px-3 !py-1.5 !text-[11px] !tracking-[0.1em] sm:!px-4 sm:!py-2 sm:!text-sm sm:!tracking-[0.15em]">
                    {t('common.bookThis')}
                  </ButtonLink>
                </div>
              </div>
            </Card>
            </TiltCard>
          </Reveal>
        ))}
      </div>
      <Pager page={paged.page} pages={paged.pages} onChange={go} className="mt-8" />
      <BarberDialog barber={shown} works={shown ? worksOf(shown.id) : []} onClose={() => setShown(null)} />
    </section>
  );
}

/** Usta haqida: rasmi, qisqacha va o'zi yuklagan ish rasmlari */
function BarberDialog({barber, works, onClose}: {barber: PublicBarber | null; works: PublicImage[]; onClose: () => void}) {
  const {t, pick} = useLang();
  const [zoom, setZoom] = useState<PublicImage | null>(null);
  const close = useCallback(() => {
    setZoom(null);
    onClose();
  }, [onClose]);
  if (!barber) return null;
  return (
    <Modal open title={barber.name} onClose={close} wide>
      <div className="flex items-center gap-4">
        {barber.photoUrl ? (
          <img src={barber.photoUrl} alt={barber.name} className="h-20 w-20 shrink-0 rounded-full border-2 border-cherry object-cover" />
        ) : (
          <Monogram name={barber.name} className="h-20 w-20 shrink-0 text-4xl" />
        )}
        <div>
          <p className="text-xs font-bold tracking-[0.3em] uppercase text-cherry">{pick(barber.title)}</p>
          <p className="mt-1 text-sm opacity-80">{pick(barber.bio)}</p>
        </div>
      </div>
      <p className="mt-6 mb-3 text-xs font-bold tracking-[0.3em] uppercase opacity-70">{t('works.title')}</p>
      {zoom ? (
        <button type="button" onClick={() => setZoom(null)} className="block w-full" aria-label="←">
          <img src={zoom.url} alt={pick(zoom.caption)} className="max-h-[60vh] w-full rounded-2xl object-contain" />
        </button>
      ) : works.length ? (
        <div className="grid grid-cols-3 gap-2 sm:grid-cols-4">
          {works.map((w) => (
            <button key={w.id} type="button" onClick={() => setZoom(w)} className="overflow-hidden rounded-xl border-2 border-navy/30">
              <img src={w.url} alt={pick(w.caption)} loading="lazy" className="aspect-square w-full object-cover transition duration-300 hover:scale-105" />
            </button>
          ))}
        </div>
      ) : (
        <p className="text-sm opacity-60">{t('works.empty')}</p>
      )}
      <div className="mt-6 text-center">
        <ButtonLink to={`/book?barber=${barber.id}`}>{t('common.bookThis')}</ButtonLink>
      </div>
    </Modal>
  );
}

function Reviews({data}: {data: SiteResponse}) {
  const {t} = useLang();
  if (!data.reviews.length) return null;
  const s = data.settings;
  return (
    <section className="mx-auto max-w-6xl px-4 pt-14 md:pt-24">
      <SectionTitle id="reviews" title={t('sections.reviews')} sub={t('sections.reviewsSub')} />
      <div className="mb-8 flex items-center justify-center gap-3">
        <span className="font-deco text-5xl text-navy md:text-6xl">{s.rating.toFixed(1)}</span>
        <div className="text-left">
          <Stars value={s.rating} className="text-2xl" />
          <p className="text-sm opacity-70">{t('hero.ratingCount', {count: s.ratingCount})}</p>
        </div>
      </div>
      <div className="mx-auto grid max-w-5xl gap-4 md:grid-cols-3">
        {data.reviews.map((r, i) => (
          <Reveal key={r.id} delay={i * 0.06}>
            <Card className="relative h-full p-4 text-sm md:p-5">
              <span aria-hidden className="absolute -top-5 left-5 font-deco text-7xl leading-none text-cherry">“</span>
              <Stars value={r.rating} />
              <p className="mt-3">{r.text}</p>
              <p className="mt-4 text-sm font-bold">— {r.authorName}</p>
            </Card>
          </Reveal>
        ))}
      </div>
    </section>
  );
}

function Gallery({data}: {data: SiteResponse}) {
  const {t, pick} = useLang();
  // Filtr: "Hammasi" (null) yoki ishi bor usta
  const [barberId, setBarberId] = useState<number | null>(null);
  const [zoom, setZoom] = useState<PublicImage | null>(null);
  const wide = useMedia('(min-width: 768px)');
  const authors = data.barbers.filter((b) => data.gallery.some((g) => g.barberId === b.id));
  const items = barberId ? data.gallery.filter((g) => g.barberId === barberId) : data.gallery;
  const paged = usePaged(items, wide ? 12 : 8);
  if (!data.gallery.length) return null;
  const choose = (id: number | null) => {
    setBarberId(id);
    paged.setPage(0);
  };
  const chip = (active: boolean) =>
    cn('rounded-full border-2 px-4 py-1.5 text-xs font-bold tracking-[0.15em] uppercase transition', active ? 'border-cherry bg-cherry text-on-accent' : 'border-navy/40 hover:border-cherry');
  return (
    <section className="mx-auto max-w-6xl px-4 pt-14 md:pt-24">
      <SectionTitle id="gallery" title={t('sections.gallery')} sub={authors.length ? t('sections.gallerySub') : undefined} />
      {authors.length ? (
        <div className="mb-6 flex flex-wrap justify-center gap-2">
          <button type="button" className={chip(barberId === null)} onClick={() => choose(null)}>
            {t('common.all')}
          </button>
          {authors.map((b) => (
            <button key={b.id} type="button" className={chip(barberId === b.id)} onClick={() => choose(b.id)}>
              {b.name}
            </button>
          ))}
        </div>
      ) : null}
      <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
        {paged.items.map((g, i) => (
          <Reveal key={g.id} delay={(i % 4) * 0.05}>
            <button type="button" onClick={() => setZoom(g)} className="block w-full overflow-hidden rounded-2xl border-2 border-navy">
              <img src={g.url} alt={pick(g.caption)} loading="lazy" className="aspect-square w-full object-cover transition duration-500 hover:scale-105" />
            </button>
          </Reveal>
        ))}
      </div>
      <Pager page={paged.page} pages={paged.pages} onChange={paged.setPage} className="mt-8" />
      <Modal open={!!zoom} title={data.barbers.find((b) => b.id === zoom?.barberId)?.name ?? t('sections.gallery')} onClose={() => setZoom(null)} wide>
        {zoom ? <img src={zoom.url} alt={pick(zoom.caption)} className="max-h-[70vh] w-full rounded-2xl object-contain" /> : null}
        {zoom && pick(zoom.caption) ? <p className="mt-3 text-center text-sm opacity-80">{pick(zoom.caption)}</p> : null}
      </Modal>
    </section>
  );
}

function Location({data}: {data: SiteResponse}) {
  const {t, pick} = useLang();
  const s = data.settings;
  const days = t('days', {returnObjects: true}) as string[];
  const mapSrc = `https://yandex.uz/map-widget/v1/?ll=${s.longitude}%2C${s.latitude}&z=16&pt=${s.longitude}%2C${s.latitude}%2Cpm2rdm`;
  const route = `https://yandex.uz/maps/?rtext=~${s.latitude}%2C${s.longitude}&rtt=auto`;
  return (
    <section className="mx-auto max-w-6xl px-4 pt-14 md:pt-24">
      <SectionTitle id="contacts" title={t('sections.location')} sub={t('sections.locationSub')} />
      <div className="grid gap-6 md:grid-cols-[1.5fr_1fr]">
        <Card className="overflow-hidden">
          <iframe title="Map" src={mapSrc} className="h-80 w-full md:h-full md:min-h-96" loading="lazy" />
        </Card>
        <Card className="flex flex-col gap-4 p-6">
          <p className="text-lg font-bold">{pick(s.address)}</p>
          <a href={`tel:${s.phone.replace(/\s/g, '')}`} className="text-2xl font-bold text-cherry hover:underline">
            {s.phone}
          </a>
          <ul className="space-y-1 text-sm">
            {s.hours
              .slice()
              .sort((a, b) => ((a.day + 6) % 7) - ((b.day + 6) % 7))
              .map((h) => (
                <li key={h.day} className="flex justify-between border-b border-dashed border-navy/30 py-1">
                  <span>{days[h.day]}</span>
                  <span className="font-semibold">{h.open ? `${h.open}–${h.close}` : t('common.closed')}</span>
                </li>
              ))}
          </ul>
          <div className="mt-auto flex flex-wrap gap-3 pt-2">
            <a href={route} target="_blank" rel="noreferrer" className="inline-flex items-center rounded-full border-2 border-navy px-4 py-2 text-xs font-bold tracking-widest uppercase hover:bg-navy hover:text-cream transition">
              {t('common.route')} ↗
            </a>
            {s.telegram ? (
              <a href={`https://t.me/${s.telegram}`} target="_blank" rel="noreferrer" className="inline-flex items-center rounded-full border-2 border-navy px-4 py-2 text-xs font-bold tracking-widest uppercase hover:bg-navy hover:text-cream transition">
                Telegram ↗
              </a>
            ) : null}
          </div>
        </Card>
      </div>
    </section>
  );
}

function CtaBand() {
  const {t} = useLang();
  return (
    <section className="relative mx-auto max-w-6xl px-4 pt-14 md:pt-24">
      <FloatingDecor className="-top-2 right-6 text-navy/15" speed={50} rotate={40}>
        <RazorIcon className="h-16 w-16 md:h-20 md:w-20" />
      </FloatingDecor>
      <div className="overflow-hidden band-dark rounded-3xl border-2 border-navy bg-navy text-center text-cream">
        <BarberPole />
        <div className="px-5 py-9 md:px-6 md:py-12">
          <p className="font-deco text-4xl md:text-6xl">{t('hero.cta')}</p>
          <p className="mx-auto mt-3 max-w-md opacity-80">{t('hero.tagline')}</p>
          <ButtonLink to="/book" className="mt-8 !shadow-[0_4px_0_0_var(--color-cream)]">
            {t('nav.book')} ★
          </ButtonLink>
        </div>
        <BarberPole />
      </div>
    </section>
  );
}
