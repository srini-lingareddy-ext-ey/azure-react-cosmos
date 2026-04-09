import { Routes, Route } from 'react-router-dom';
import AppLayout from '../layout/AppLayout';
import HomePage from '../../features/home';
import { LoginPage } from '../../features/auth';
import AuthGuard from '../../auth/AuthGuard';

const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<AuthGuard />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<HomePage />} />
        </Route>
      </Route>
    </Routes>
  );
};

export default AppRoutes;
