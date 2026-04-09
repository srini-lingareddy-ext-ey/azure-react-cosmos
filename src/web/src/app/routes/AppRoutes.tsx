import { Routes, Route, Outlet } from 'react-router-dom';
import AppLayout from '../layout/AppLayout';
import HomePage from '../../features/home';
import { LoginPage } from '../../features/auth';
import AuthGuard from '../../auth/AuthGuard';
import RoleGuard from '../../auth/RoleGuard';
import {
  TenantRosterPage,
  TenantDetailPage,
} from '../../features/tenants';

const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<AuthGuard />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route
            path="/admin/tenants"
            element={
              <RoleGuard requiredRoles={['PlatformAdmin']}>
                <Outlet />
              </RoleGuard>
            }
          >
            <Route index element={<TenantRosterPage />} />
            <Route path=":id" element={<TenantDetailPage />} />
          </Route>
        </Route>
      </Route>
    </Routes>
  );
};

export default AppRoutes;
