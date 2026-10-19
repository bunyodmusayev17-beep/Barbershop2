import {useState} from 'react';
import type {LocalizedText} from '../../api/types';
import {cn} from '../../lib/cn';
import {Input, Textarea} from './FormControls';

const LANGS: {key: keyof LocalizedText; label: string}[] = [
  {key: 'uz', label: "O'zbekcha"},
  {key: 'ru', label: 'Ruscha'},
  {key: 'en', label: 'Inglizcha'},
];

/** 3 tilli matnni tahrirlash: til tablari, o'zbekchasi majburiy. */
export function LangFields({label, value, onChange, multiline, required}: {label: string; value: LocalizedText; onChange: (v: LocalizedText) => void; multiline?: boolean; required?: boolean}) {
  const [tab, setTab] = useState<keyof LocalizedText>('uz');
  const Comp = multiline ? Textarea : Input;
  return (
    <div>
      <div className="mb-1 flex items-center justify-between gap-2">
        <span className="text-xs font-bold tracking-wider uppercase text-navy/70">{label}</span>
        <div className="flex gap-1">
          {LANGS.map((l) => (
            <button
              key={l.key}
              type="button"
              onClick={() => setTab(l.key)}
              className={cn(
                'rounded-full px-2 py-0.5 text-[11px] font-bold uppercase',
                tab === l.key ? 'bg-navy text-cream' : 'text-navy/60 hover:bg-cream-dark',
                !value[l.key] && tab !== l.key && 'text-cherry',
              )}
              title={l.label}
            >
              {l.key}
              {!value[l.key] ? ' •' : ''}
            </button>
          ))}
        </div>
      </div>
      <Comp
        value={value[tab]}
        required={required && tab === 'uz'}
        placeholder={LANGS.find((l) => l.key === tab)?.label}
        onChange={(e: {target: {value: string}}) => onChange({...value, [tab]: e.target.value})}
      />
    </div>
  );
}
