import {useEffect} from 'react';
import {useLocation} from 'react-router-dom';

/** "/#services" kabi havolalar: sahifa chizilgandan keyin bo'limga o'tish; boshqa o'tishlarda — tepaga */
export function ScrollManager() {
  const {pathname, hash} = useLocation();
  useEffect(() => {
    if (!hash) {
      window.scrollTo({top: 0});
      return;
    }
    let tries = 0;
    const tick = () => {
      const el = document.getElementById(hash.slice(1));
      if (el) el.scrollIntoView({behavior: 'smooth', block: 'start'});
      else if (tries++ < 20) setTimeout(tick, 100);
    };
    tick();
  }, [pathname, hash]);
  return null;
}
