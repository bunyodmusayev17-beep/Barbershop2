import {useMutation, useQuery, useQueryClient} from '@tanstack/react-query';
import {useRef} from 'react';
import {profileApi, type BarberProfile} from '../../api/profile';
import {Btn} from '../../components/admin/Btn';
import {Empty} from '../../components/admin/Empty';
import {PageHeader} from '../../components/admin/PageHeader';
import {Panel} from '../../components/admin/Panel';
import {Monogram} from '../../components/ui/Monogram';
import {Spinner} from '../../components/ui/Spinner';
import {useToast} from '../../hooks/useToast';
import {errorText} from '../../lib/format';

const ACCEPT = 'image/jpeg,image/png,image/webp';

/** Usta kabineti: o'z rasmini qo'yadi va ishlarining rasmlarini yuklaydi — saytda darhol ko'rinadi */
export default function ProfilePage() {
  const qc = useQueryClient();
  const toast = useToast();
  const photoInput = useRef<HTMLInputElement>(null);
  const worksInput = useRef<HTMLInputElement>(null);
  const profile = useQuery({queryKey: ['staff-profile'], queryFn: profileApi.get});

  const done = (message: string) => (p: BarberProfile) => {
    qc.setQueryData(['staff-profile'], p);
    qc.invalidateQueries({queryKey: ['site']});
    toast(message);
  };
  const onError = (e: unknown) => toast(errorText(e), 'error');

  const uploadPhoto = useMutation({mutationFn: profileApi.uploadPhoto, onSuccess: done('Rasm yangilandi'), onError});
  const deletePhoto = useMutation({mutationFn: profileApi.deletePhoto, onSuccess: done("Rasm o'chirildi"), onError});
  const uploadWorks = useMutation({mutationFn: profileApi.uploadWorks, onSuccess: done('Ishlar yuklandi'), onError});
  const deleteWork = useMutation({mutationFn: profileApi.deleteWork, onSuccess: done("O'chirildi"), onError});

  if (profile.isLoading) return <Spinner className="h-8 w-8" />;
  if (!profile.data) return <Empty>{profile.error ? errorText(profile.error) : 'Profil topilmadi'}</Empty>;
  const p = profile.data;
  const left = p.maxWorks - p.works.length;

  return (
    <>
      <PageHeader title="Profilim" sub="Rasmingiz va ishlaringiz saytda darhol ko'rinadi" />

      <Panel title="Mening rasmim" className="mb-6">
        <div className="flex flex-wrap items-center gap-5">
          {p.photoUrl ? (
            <img src={p.photoUrl} alt={p.name} className="h-28 w-28 rounded-full border-4 border-cherry object-cover" />
          ) : (
            <Monogram name={p.name} className="h-28 w-28 text-5xl" />
          )}
          <div className="grid gap-2">
            <p className="font-deco text-3xl text-navy">{p.name}</p>
            <input
              ref={photoInput}
              type="file"
              accept={ACCEPT}
              className="hidden"
              onChange={(e) => {
                const f = e.target.files?.[0];
                if (f) uploadPhoto.mutate(f);
                e.target.value = '';
              }}
            />
            <div className="flex flex-wrap gap-2">
              <Btn onClick={() => photoInput.current?.click()} disabled={uploadPhoto.isPending}>
                {uploadPhoto.isPending ? <Spinner /> : p.photoUrl ? 'Rasmni almashtirish' : "Rasm qo'yish"}
              </Btn>
              {p.photoUrl ? (
                <Btn variant="danger" onClick={() => confirm("Rasmingizni o'chirasizmi?") && deletePhoto.mutate()} disabled={deletePhoto.isPending}>
                  O'chirish
                </Btn>
              ) : null}
            </div>
            <p className="text-xs opacity-60">Yuzingiz aniq ko'rinadigan, kvadratga yaqin rasm · JPG, PNG, WEBP · 5 MB gacha</p>
          </div>
        </div>
      </Panel>

      <Panel title={`Mening ishlarim · ${p.works.length} / ${p.maxWorks}`}>
        <div className="mb-4 flex flex-wrap items-center gap-3">
          <input
            ref={worksInput}
            type="file"
            multiple
            accept={ACCEPT}
            className="hidden"
            onChange={(e) => {
              const files = Array.from(e.target.files ?? []).slice(0, Math.min(10, left));
              if (files.length) uploadWorks.mutate(files);
              e.target.value = '';
            }}
          />
          <Btn onClick={() => worksInput.current?.click()} disabled={uploadWorks.isPending || left <= 0}>
            {uploadWorks.isPending ? <Spinner /> : '+ Ish rasmini yuklash'}
          </Btn>
          <p className="text-xs opacity-60">
            {left > 0 ? `Bir vaqtda 10 tagacha · yana ${left} ta yuklash mumkin` : "Limit to'ldi — yangisini yuklash uchun eskisini o'chiring"}
          </p>
        </div>
        {!p.works.length ? (
          <Empty>Hali ish rasmi yo'q. Qilgan soch turmaklaringizni yuklang — mijozlar saytda ko'radi.</Empty>
        ) : (
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
            {p.works.map((w) => (
              <figure key={w.id} className="group relative overflow-hidden rounded-2xl border-2 border-navy/15 bg-white">
                <img src={w.url} alt="" loading="lazy" className="aspect-square w-full object-cover" />
                <button
                  type="button"
                  aria-label="O'chirish"
                  onClick={() => confirm("Bu rasmni o'chirasizmi?") && deleteWork.mutate(w.id)}
                  disabled={deleteWork.isPending}
                  className="absolute top-2 right-2 flex h-9 w-9 items-center justify-center rounded-full bg-ink/70 text-white transition hover:bg-rose-600"
                >
                  ✕
                </button>
              </figure>
            ))}
          </div>
        )}
      </Panel>
    </>
  );
}
