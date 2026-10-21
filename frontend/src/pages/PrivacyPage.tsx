import {useLang} from '../i18n';
import {useSite} from '../hooks/useSite';
import {BRAND} from '../constants/brand';

type Section = {title: string; body: string};

/** Maxfiylik siyosati — Play Market talab qiladi (ilova sahifasida shu manzil ko'rsatiladi: /privacy) */
const TEXT: Record<'uz' | 'ru' | 'en', {title: string; updated: string; sections: Section[]}> = {
  uz: {
    title: 'Maxfiylik siyosati',
    updated: 'Oxirgi yangilanish: 2026-yil 21-oktabr',
    sections: [
      {title: "Qanday ma'lumot yig'iladi", body: "Onlayn navbat olganda: ismingiz, telefon raqamingiz, tanlangan xizmat, usta, sana va vaqt, ixtiyoriy izoh. Xodimlar uchun: login, parol (shifrlangan holda) va ixtiyoriy Telegram username. Ustalar o'z rasmi va ish rasmlarini yuklashi mumkin."},
      {title: 'Nima uchun ishlatiladi', body: "Faqat navbatni qayd etish, usta va admin mijoz bilan bog'lanishi, eslatma va hisobotlar uchun. Ma'lumotlar sotilmaydi, reklama uchun ishlatilmaydi va uchinchi shaxslarga berilmaydi."},
      {title: 'Saqlash va xavfsizlik', body: "Ma'lumotlar himoyalangan serverda (HTTPS) saqlanadi. Parollar shifrlangan holda saqlanadi. Ilova telefoningizda faqat kirish belgisini (token) saqlaydi."},
      {title: "Ma'lumotni o'chirish", body: "Navbatingiz yoki ma'lumotlaringizni o'chirishni xohlasangiz, quyidagi telefon orqali murojaat qiling — ular o'chiriladi."},
    ],
  },
  ru: {
    title: 'Политика конфиденциальности',
    updated: 'Последнее обновление: 21 октября 2026 г.',
    sections: [
      {title: 'Какие данные собираются', body: 'При онлайн-записи: имя, номер телефона, выбранная услуга, мастер, дата и время, необязательный комментарий. Для сотрудников: логин, пароль (в зашифрованном виде) и необязательный Telegram username. Мастера могут загружать своё фото и фото работ.'},
      {title: 'Для чего используются', body: 'Только для записи, связи мастера и администратора с клиентом, напоминаний и отчётов. Данные не продаются, не используются для рекламы и не передаются третьим лицам.'},
      {title: 'Хранение и безопасность', body: 'Данные хранятся на защищённом сервере (HTTPS). Пароли хранятся в зашифрованном виде. Приложение хранит на телефоне только токен входа.'},
      {title: 'Удаление данных', body: 'Чтобы удалить запись или свои данные, свяжитесь с нами по телефону ниже — они будут удалены.'},
    ],
  },
  en: {
    title: 'Privacy policy',
    updated: 'Last updated: October 21, 2026',
    sections: [
      {title: 'What we collect', body: 'When you book online: your name, phone number, chosen service, barber, date and time, and an optional comment. For staff: login, password (stored hashed) and an optional Telegram username. Barbers may upload their photo and photos of their work.'},
      {title: 'How it is used', body: 'Only to record bookings, let the barber and admin contact you, and for reminders and reports. Data is not sold, not used for advertising and not shared with third parties.'},
      {title: 'Storage and security', body: 'Data is stored on a secured server (HTTPS). Passwords are stored hashed. The app stores only a sign-in token on your phone.'},
      {title: 'Deleting your data', body: 'To delete a booking or your data, contact us by the phone number below and it will be deleted.'},
    ],
  },
};

export default function PrivacyPage() {
  const {lang} = useLang();
  const {data} = useSite();
  const t = TEXT[lang as keyof typeof TEXT] ?? TEXT.uz;
  return (
    <section className="mx-auto max-w-3xl px-4 py-12">
      <h1 className="font-deco text-[2.1rem] text-cherry md:text-5xl">{t.title}</h1>
      <p className="mt-2 text-sm opacity-60">
        {BRAND.name} · {t.updated}
      </p>
      <div className="mt-8 space-y-6">
        {t.sections.map((s) => (
          <div key={s.title}>
            <h2 className="font-deco text-xl text-navy">{s.title}</h2>
            <p className="mt-2 leading-relaxed opacity-85">{s.body}</p>
          </div>
        ))}
      </div>
      {data?.settings.phone ? (
        <p className="mt-8 font-bold">
          📞 <a href={`tel:${data.settings.phone.replace(/\s/g, '')}`} className="text-cherry">{data.settings.phone}</a>
        </p>
      ) : null}
    </section>
  );
}
