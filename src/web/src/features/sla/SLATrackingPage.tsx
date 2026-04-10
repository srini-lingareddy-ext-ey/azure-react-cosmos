import { FC, useState } from 'react';
import { Stack, Text, Panel, PanelType, Spinner } from '@fluentui/react';
import SLAStatusTable from './SLAStatusTable';
import ComplianceSummaryPanel from './ComplianceSummaryPanel';
import SLATrendChart from './SLATrendChart';
import SLADrillDown from './SLADrillDown';
import { useSLAHistory } from './hooks/useSLA';
import type { SLABreachHistory } from './slaService';

const SLATrackingPage: FC = () => {
  const [historyPipeline, setHistoryPipeline] = useState<string | null>(null);
  const [drillDownBP, setDrillDownBP] = useState<string | null>(null);
  const historyData = useSLAHistory(historyPipeline ?? '');

  return (
    <Stack tokens={{ childrenGap: 24 }}>
      <Text variant="xxLarge">SLA Tracking</Text>

      <ComplianceSummaryPanel
        timeRange="last7d"
        onSelectBusinessPlan={setDrillDownBP}
      />

      <SLATrendChart days={7} />

      <SLAStatusTable onSelectPipeline={setHistoryPipeline} />

      <SLADrillDown
        businessPlan={drillDownBP}
        onDismiss={() => setDrillDownBP(null)}
      />

      <Panel
        isOpen={!!historyPipeline}
        onDismiss={() => setHistoryPipeline(null)}
        type={PanelType.medium}
        headerText="SLA Breach History"
      >
        {historyData.isLoading ? (
          <Spinner />
        ) : (
          historyData.data?.map((b: SLABreachHistory) => (
            <Stack
              key={b.id}
              tokens={{ childrenGap: 4 }}
              styles={{ root: { padding: 8, borderBottom: '1px solid #e0e0e0' } }}
            >
              <Text>Detected: {new Date(b.breachDetectedAt).toLocaleString()}</Text>
              <Text>Window Closed: {new Date(b.slaWindowClosedAt).toLocaleString()}</Text>
              {b.completedAt && (
                <Text>Completed: {new Date(b.completedAt).toLocaleString()}</Text>
              )}
              {b.minutesOverdue != null && (
                <Text>Overdue: {b.minutesOverdue} min</Text>
              )}
            </Stack>
          ))
        )}
      </Panel>
    </Stack>
  );
};

export default SLATrackingPage;
