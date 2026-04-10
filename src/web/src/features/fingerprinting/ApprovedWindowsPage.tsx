import { FC } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text, DefaultButton } from '@fluentui/react';
import { useApprovedWindows, useDeleteWindow } from './hooks/useFingerprinting';
import type { ApprovedWindowView } from './fingerprintingService';

const ApprovedWindowsPage: FC = () => {
  const { data, isLoading, isError, refetch } = useApprovedWindows();
  const del = useDeleteWindow();

  const columns: IColumn[] = [
    { key: 'name', name: 'Name', fieldName: 'name', minWidth: 160 },
    { key: 'start', name: 'Start', minWidth: 160, onRender: (i: ApprovedWindowView) => <Text>{new Date(i.startTime).toLocaleString()}</Text> },
    { key: 'end', name: 'End', minWidth: 160, onRender: (i: ApprovedWindowView) => <Text>{new Date(i.endTime).toLocaleString()}</Text> },
    { key: 'scope', name: 'Scope', minWidth: 120, onRender: (i: ApprovedWindowView) => <Text>{i.scopeType}{i.scopeValue ? `: ${i.scopeValue}` : ''}</Text> },
    { key: 'del', name: '', minWidth: 80, onRender: (i: ApprovedWindowView) => <DefaultButton text="Delete" onClick={() => del.mutate(i.id)} /> },
  ];

  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Approved Change Windows</Text>
      {isLoading ? <Spinner /> : isError ? (
        <Stack horizontal tokens={{ childrenGap: 8 }}><Text>Error loading windows.</Text><button onClick={() => refetch()}>Retry</button></Stack>
      ) : !data?.length ? <Text>No approved windows configured.</Text> : (
        <DetailsList items={data} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={(i: ApprovedWindowView) => i.id} />
      )}
    </Stack>
  );
};

export default ApprovedWindowsPage;
