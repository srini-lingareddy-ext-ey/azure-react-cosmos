import { FC } from 'react';
import { Panel, PanelType, Spinner, Stack, Text, Link } from '@fluentui/react';
import { useEventDetail, useClassificationRule } from './hooks/useEventLog';

interface EventDetailPanelProps {
  eventId: string | null;
  onDismiss: () => void;
}

const EventDetailPanel: FC<EventDetailPanelProps> = ({ eventId, onDismiss }) => {
  const { data: detail, isLoading } = useEventDetail(eventId ?? '');
  const ruleId = detail?.classificationRuleId;
  const { data: rule } = useClassificationRule(ruleId);

  const ruleDescription = !ruleId || ruleId === 'default'
    ? 'Default classification'
    : rule?.description ?? ruleId;

  return (
    <Panel isOpen={!!eventId} onDismiss={onDismiss} type={PanelType.medium} headerText="Event Detail">
      {isLoading ? <Spinner /> : detail ? (
        <Stack tokens={{ childrenGap: 12 }} styles={{ root: { padding: '16px 0' } }}>
          <Text variant="mediumPlus"><b>Event ID:</b> {detail.eventId}</Text>
          <Text><b>Type:</b> {detail.eventType}</Text>
          <Text><b>Severity:</b> {detail.severity}</Text>
          <Text><b>Classification:</b> {detail.classification}</Text>
          <Text><b>Rule:</b> {ruleDescription}</Text>
          <Text><b>Source:</b> {detail.sourceSystem}</Text>
          <Text><b>Monitor:</b> {detail.monitorName}</Text>
          {detail.businessPlan && <Text><b>Business Plan:</b> {detail.businessPlan}</Text>}
          {detail.notificationStatus && <Text><b>Notification:</b> {detail.notificationStatus}</Text>}
          {detail.incidentId && <Link href={`/incidents/${detail.incidentId}`}>View Incident</Link>}
          {detail.pipelineId && <Link href={`/pipelines?pipelineId=${detail.pipelineId}`}>View Pipeline</Link>}
          <Text variant="mediumPlus" styles={{ root: { marginTop: 16 } }}><b>Raw Payload</b></Text>
          <pre style={{ background: '#f4f4f4', padding: 12, borderRadius: 4, overflow: 'auto', fontSize: 12 }}>
            {JSON.stringify(detail.rawPayload, null, 2)}
          </pre>
        </Stack>
      ) : <Text>Event not found.</Text>}
    </Panel>
  );
};

export default EventDetailPanel;
