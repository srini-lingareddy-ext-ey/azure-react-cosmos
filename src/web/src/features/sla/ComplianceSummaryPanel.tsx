import { FC } from 'react';
import { Spinner, Stack, Text } from '@fluentui/react';
import { useSLACompliance } from './hooks/useSLA';
import type { SLAComplianceSummary } from './slaService';

interface ComplianceSummaryPanelProps {
  timeRange?: string;
  onSelectBusinessPlan?: (bp: string) => void;
}

const cardStyle = {
  root: {
    padding: 16,
    border: '1px solid #e0e0e0',
    borderRadius: 4,
    minWidth: 180,
    cursor: 'pointer',
    ':hover': { backgroundColor: '#f3f3f3' },
  },
};

const ComplianceSummaryPanel: FC<ComplianceSummaryPanelProps> = ({
  timeRange = 'last7d',
  onSelectBusinessPlan,
}) => {
  const { data, isLoading, isError, refetch } = useSLACompliance(timeRange);

  if (isLoading) return <Spinner label="Loading compliance..." />;
  if (isError)
    return (
      <Stack horizontal tokens={{ childrenGap: 8 }}>
        <Text>Error loading compliance data.</Text>
        <button onClick={() => refetch()}>Retry</button>
      </Stack>
    );
  if (!data?.summary?.length) return null;

  return (
    <Stack horizontal tokens={{ childrenGap: 16 }} wrap>
      {data.summary.map((s: SLAComplianceSummary) => (
        <Stack
          key={s.businessPlan}
          styles={cardStyle}
          onClick={() => onSelectBusinessPlan?.(s.businessPlan)}
        >
          <Text variant="mediumPlus" styles={{ root: { fontWeight: 600 } }}>
            {s.businessPlan}
          </Text>
          <Text>Met: {s.percentageMet}%</Text>
          <Text>Breaches: {s.breachCount}</Text>
          <Text>At Risk: {s.atRiskCount}</Text>
        </Stack>
      ))}
    </Stack>
  );
};

export default ComplianceSummaryPanel;
