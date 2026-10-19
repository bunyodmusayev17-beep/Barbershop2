import {Link} from 'react-router-dom';
import {useSite} from '../hooks/useSite';

export default function NotFoundPage() {
  const {data} = useSite();
  return (
    <div className="py-32 text-center">
      <p className="font-deco text-8xl text-cherry">404</p>
      <Link to="/" className="mt-6 inline-block underline">
        {data?.settings.businessName ?? '/'}
      </Link>
    </div>
  );
}
