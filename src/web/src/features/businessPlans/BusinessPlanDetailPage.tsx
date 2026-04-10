import { FC } from 'react';
import { useParams } from 'react-router-dom';
import { Spinner, Stack, Text } from '@fluentui/react';
import { useBusinessPlan } from './hooks/useBusinessPlans';

const BusinessPlanDetailPage: FC = () => {
  const { id } = useParams<{ id: string }>();
  const { data, isLoading } = useBusinessPlan(id);
  if (isLoading) return <Spinner label="Loading..." />;
  if (!data) return <Text>Not found.</Text>;
  return (
    <Stack tokens={{ childrenGap: 12 }}>
      <Text variant="xxLarge">{data.name}</Text>
      <Text>Domain: {data.domain ?? '—'}</Text>
      <Text>Description: {data.description ?? '—'}</Text>
      <Text>Active: {data.isActive ? 'Yes' : 'No'}</Text>
      {data.defaultSlaWindow && <Text>SLA Window: {data.defaultSlaWindow.windowType} / {data.defaultSlaWindow.windowValue} min (buffer: {data.defaultSlaWindow.atRiskBufferMinutes} min)</Text>}
    </Stack>
  );
};
export default BusinessPlanDetailPage;
