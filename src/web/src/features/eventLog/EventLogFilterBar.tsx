import { FC } from 'react';
import { Stack, Dropdown, IDropdownOption } from '@fluentui/react';

const classificationOpts: IDropdownOption[] = [
  { key: '', text: 'All Classifications' },
  { key: 'incident', text: 'Incident' },
  { key: 'alert', text: 'Alert' },
  { key: 'availabilityIssue', text: 'Availability Issue' },
  { key: 'slaBreach', text: 'SLA Breach' },
  { key: 'informational', text: 'Informational' },
];

const severityOpts: IDropdownOption[] = [
  { key: '', text: 'All Severities' },
  { key: 'info', text: 'Info' },
  { key: 'warning', text: 'Warning' },
  { key: 'critical', text: 'Critical' },
];

interface EventLogFilterBarProps {
  classification: string;
  severity: string;
  onClassificationChange: (val: string) => void;
  onSeverityChange: (val: string) => void;
}

const EventLogFilterBar: FC<EventLogFilterBarProps> = ({
  classification, severity, onClassificationChange, onSeverityChange,
}) => (
  <Stack horizontal tokens={{ childrenGap: 12 }}>
    <Dropdown label="Classification" options={classificationOpts} selectedKey={classification}
      onChange={(_, o) => onClassificationChange((o?.key as string) ?? '')} styles={{ root: { minWidth: 180 } }} />
    <Dropdown label="Severity" options={severityOpts} selectedKey={severity}
      onChange={(_, o) => onSeverityChange((o?.key as string) ?? '')} styles={{ root: { minWidth: 150 } }} />
  </Stack>
);

export default EventLogFilterBar;
