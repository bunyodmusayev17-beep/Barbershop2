import {MotionConfig} from 'framer-motion';
import {Suspense} from 'react';
import {BrowserRouter, Route, Routes} from 'react-router-dom';
import {Layout} from './components/layout/Layout';
import {Preloader} from './components/routing/Preloader';
import {RouteFallback} from './components/routing/RouteFallback';
import {ScrollManager} from './components/routing/ScrollManager';
import {TelegramBridge} from './components/routing/TelegramBridge';
import {
  AppointmentsView,
  BarbersPage,
  BookingPage,
  CalendarPage,
  DashboardPage,
  GalleryPage,
  LoginPage,
  ProfilePage,
  ReviewsPage,
  ServicesPage,
  SettingsPage,
  StaffArea,
  TimeOffPage,
  UsersPage,
} from './lib/pages';
import HomePage from './pages/HomePage';
import NotFoundPage from './pages/NotFoundPage';
import {AuthProvider} from './providers/AuthProvider';
import {QueryProvider} from './providers/QueryProvider';

export default function App() {
  return (
    <QueryProvider>
      {/* Telefonda "harakatni kamaytirish" yoqilgan bo'lsa — barcha animatsiyalar o'chadi */}
      <MotionConfig reducedMotion="user">
        <AuthProvider>
          <BrowserRouter>
            <ScrollManager />
            <Preloader />
            <TelegramBridge />
            <Suspense fallback={<RouteFallback />}>
              <Routes>
                <Route element={<Layout />}>
                  <Route index element={<HomePage />} />
                  <Route path="book" element={<BookingPage />} />
                  <Route path="login" element={<LoginPage />} />
                  <Route path="*" element={<NotFoundPage />} />
                </Route>

                <Route path="admin" element={<StaffArea role="Admin" />}>
                  <Route index element={<DashboardPage />} />
                  <Route path="appointments" element={<AppointmentsView />} />
                  <Route path="calendar" element={<CalendarPage />} />
                  <Route path="services" element={<ServicesPage />} />
                  <Route path="barbers" element={<BarbersPage />} />
                  <Route path="timeoff" element={<TimeOffPage />} />
                  <Route path="gallery" element={<GalleryPage />} />
                  <Route path="reviews" element={<ReviewsPage />} />
                  <Route path="users" element={<UsersPage />} />
                  <Route path="settings" element={<SettingsPage />} />
                </Route>

                <Route path="barber" element={<StaffArea role="Barber" />}>
                  <Route index element={<AppointmentsView />} />
                  <Route path="calendar" element={<CalendarPage />} />
                  <Route path="timeoff" element={<TimeOffPage />} />
                  <Route path="profile" element={<ProfilePage />} />
                </Route>
              </Routes>
            </Suspense>
          </BrowserRouter>
        </AuthProvider>
      </MotionConfig>
    </QueryProvider>
  );
}
