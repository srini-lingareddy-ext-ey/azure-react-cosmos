import { lazy, Suspense } from 'react';
import { Routes, Route, Outlet } from 'react-router-dom';
import { Spinner } from '@fluentui/react';
import AppLayout from '../layout/AppLayout';
import HomePage from '../../features/home';
import { LoginPage } from '../../features/auth';
import AuthGuard from '../../auth/AuthGuard';
import RoleGuard from '../../auth/RoleGuard';
import {
  TenantRosterPage,
  TenantDetailPage,
} from '../../features/tenants';
import { UserRosterPage } from '../../features/users';

/* ── Wave 4 lazy imports ── */
const BusinessPlanListPage = lazy(
  () => import('../../features/businessPlans/BusinessPlanListPage')
);
const BusinessPlanDetailPage = lazy(
  () => import('../../features/businessPlans/BusinessPlanDetailPage')
);
const PipelineRegistrationListPage = lazy(
  () => import('../../features/pipelines/PipelineRegistrationListPage')
);
const ConnectionListPage = lazy(
  () => import('../../features/connections/ConnectionListPage')
);
const MonitorListPage = lazy(
  () => import('../../features/monitors/MonitorListPage')
);
const QueryTemplatePage = lazy(
  () => import('../../features/queryTemplates/QueryTemplatePage')
);
const ConnectorListPage = lazy(
  () => import('../../features/connectors/ConnectorListPage')
);
const ConnectorExecutionLogViewer = lazy(
  () => import('../../features/connectors/ConnectorExecutionLogViewer')
);

const LazyFallback = <Spinner label="Loading…" />;

const AppRoutes: React.FC = () => {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<AuthGuard />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route
            path="/admin/users"
            element={
              <RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}>
                <UserRosterPage />
              </RoleGuard>
            }
          />
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

          {/* ── Wave 4: Business Plans ── */}
          <Route
            path="/admin/business-plans"
            element={
              <RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}>
                <Suspense fallback={LazyFallback}>
                  <Outlet />
                </Suspense>
              </RoleGuard>
            }
          >
            <Route index element={<BusinessPlanListPage />} />
            <Route path=":id" element={<BusinessPlanDetailPage />} />
          </Route>

          {/* ── Wave 4: Pipelines ── */}
          <Route
            path="/admin/pipelines"
            element={
              <RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}>
                <Suspense fallback={LazyFallback}>
                  <PipelineRegistrationListPage />
                </Suspense>
              </RoleGuard>
            }
          />

          {/* ── Wave 4: Connections ── */}
          <Route
            path="/admin/connections"
            element={
              <RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}>
                <Suspense fallback={LazyFallback}>
                  <ConnectionListPage />
                </Suspense>
              </RoleGuard>
            }
          />

          {/* ── Wave 4: Monitors ── */}
          <Route
            path="/admin/monitors"
            element={
              <RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}>
                <Suspense fallback={LazyFallback}>
                  <MonitorListPage />
                </Suspense>
              </RoleGuard>
            }
          />

          {/* ── Wave 4: Query Templates ── */}
          <Route
            path="/admin/query-templates"
            element={
              <RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}>
                <Suspense fallback={LazyFallback}>
                  <QueryTemplatePage />
                </Suspense>
              </RoleGuard>
            }
          />

          {/* ── Wave 4: Connectors ── */}
          <Route
            path="/admin/connectors"
            element={
              <RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}>
                <Suspense fallback={LazyFallback}>
                  <Outlet />
                </Suspense>
              </RoleGuard>
            }
          >
            <Route index element={<ConnectorListPage />} />
            <Route path=":id/logs" element={<ConnectorExecutionLogViewer />} />
          </Route>
        </Route>
      </Route>
    </Routes>
  );
};

export default AppRoutes;
