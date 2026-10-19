import {Ornament} from './Ornament';

export function SectionTitle({title, sub, id}: {title: string; sub?: string; id?: string}) {
  return (
    <div id={id} className="scroll-mt-28 text-center mb-10">
      <h2 className="font-deco text-cherry text-5xl md:text-6xl leading-none">{title}</h2>
      {sub ? <Ornament className="mt-4 text-navy">{sub}</Ornament> : null}
    </div>
  );
}
