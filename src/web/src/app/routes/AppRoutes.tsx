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

/* ── Phase 4 monitoring lazy imports ── */
const PipelineStatusPage = lazy(
  () => import('../../features/pipelineMonitoring/PipelineStatusPage')
);
const DataQualityPage = lazy(
  () => import('../../features/dataQuality/DataQualityPage')
);
const SLATrackingPage = lazy(
  () => import('../../features/sla/SLATrackingPage')
);
const InfrastructurePage = lazy(
  () => import('../../features/infrastructureMonitoring/InfrastructurePage')
);

/* ── Phase 5 lazy imports ── */
const EventLogPage = lazy(
  () => import('../../features/eventLog/EventLogPage')
);
const ClassificationRulesPage = lazy(
  () => import('../../features/classificationRules/ClassificationRulesPage')
);
const ClassificationAuditLogPage = lazy(
  () => import('../../features/classificationRules/ClassificationAuditLogPage')
);
const ArtifactRegistryPage = lazy(
  () => import('../../features/fingerprinting/ArtifactRegistryPage')
);
const ApprovedWindowsPage = lazy(
  () => import('../../features/fingerprinting/ApprovedWindowsPage')
);
const FingerprintAuditTrailPage = lazy(
  () => import('../../features/fingerprinting/FingerprintAuditTrailPage')
);

/* ── Phase 6 lazy imports ── */
const IncidentListPage = lazy(
  () => import('../../features/incidents/IncidentListPage')
);
const IncidentDetailPage = lazy(
  () => import('../../features/incidents/IncidentDetailPage')
);
const ChannelListPage = lazy(
  () => import('../../features/notifications/ChannelListPage')
);
const RoutingRuleListPage = lazy(
  () => import('../../features/notifications/RoutingRuleListPage')
);
const MaintenanceWindowPage = lazy(
  () => import('../../features/notifications/MaintenanceWindowPage')
);
const DeliveryLogPage = lazy(
  () => import('../../features/notifications/DeliveryLogPage')
);
const ServiceNowConfigPage = lazy(
  () => import('../../features/serviceNow/ServiceNowConfigPage')
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

          {/* ── Phase 4: Pipeline Monitoring ── */}
          <Route path="/pipelines" element={<Suspense fallback={LazyFallback}><PipelineStatusPage /></Suspense>} />

          {/* ── Phase 4: Data Quality ── */}
          <Route path="/data-quality" element={<Suspense fallback={LazyFallback}><DataQualityPage /></Suspense>} />

          {/* ── Phase 4: SLA Tracking ── */}
          <Route path="/sla" element={<Suspense fallback={LazyFallback}><SLATrackingPage /></Suspense>} />

          {/* ── Phase 4: Infrastructure Monitoring ── */}
          <Route path="/infrastructure" element={<Suspense fallback={LazyFallback}><InfrastructurePage /></Suspense>} />

          {/* ── Phase 5: Event Log ── */}
          <Route path="/events" element={<Suspense fallback={LazyFallback}><EventLogPage /></Suspense>} />

          {/* ── Phase 5: Classification Rules (Admin) ── */}
          <Route path="/admin/classification-rules" element={<RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}><Suspense fallback={LazyFallback}><Outlet /></Suspense></RoleGuard>}>
            <Route index element={<ClassificationRulesPage />} />
            <Route path="audit-log" element={<ClassificationAuditLogPage />} />
          </Route>

          {/* ── Phase 5: Fingerprinting (Admin) ── */}
          <Route path="/admin/fingerprinting" element={<RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}><Suspense fallback={LazyFallback}><Outlet /></Suspense></RoleGuard>}>
            <Route path="artifacts" element={<ArtifactRegistryPage />} />
            <Route path="windows" element={<ApprovedWindowsPage />} />
            <Route path="audit-trail" element={<FingerprintAuditTrailPage />} />
          </Route>

          {/* ── Phase 6: Incidents ── */}
          <Route path="/incidents" element={<Suspense fallback={LazyFallback}><IncidentListPage /></Suspense>} />
          <Route path="/incidents/:id" element={<Suspense fallback={LazyFallback}><IncidentDetailPage /></Suspense>} />

          {/* ── Phase 6: Notifications Config (Admin) ── */}
          <Route path="/admin/notifications" element={<RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}><Suspense fallback={LazyFallback}><Outlet /></Suspense></RoleGuard>}>
            <Route path="channels" element={<ChannelListPage />} />
            <Route path="routing-rules" element={<RoutingRuleListPage />} />
            <Route path="maintenance-windows" element={<MaintenanceWindowPage />} />
            <Route path="delivery-log" element={<DeliveryLogPage />} />
          </Route>

          {/* ── Phase 6: ServiceNow Config (Admin) ── */}
          <Route path="/admin/servicenow" element={<RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}><Suspense fallback={LazyFallback}><ServiceNowConfigPage /></Suspense></RoleGuard>} />

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
