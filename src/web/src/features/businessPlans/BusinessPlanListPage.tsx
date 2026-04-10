import { FC, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ChoiceGroup, IChoiceGroupOption, DetailsList, IColumn, SelectionMode, CheckboxVisibility, PrimaryButton, DefaultButton, Spinner, Stack, Text, MessageBar, MessageBarType } from '@fluentui/react';
import { useBusinessPlans, useActivateBusinessPlan, useDeactivateBusinessPlan } from './hooks/useBusinessPlans';
import type { BusinessPlanResponse } from './businessPlanTypes';

type Filter = 'all' | 'true' | 'false';
const opts: IChoiceGroupOption[] = [{ key: 'all', text: 'All' }, { key: 'true', text: 'Active' }, { key: 'false', text: 'Inactive' }];

const BusinessPlanListPage: FC = () => {
  const navigate = useNavigate();
  const [filter, setFilter] = useState<Filter>('all');
  const isActive = filter === 'all' ? undefined : filter === 'true';
  const { data, isLoading, isError } = useBusinessPlans(isActive);
  const items = useMemo(() => data?.items ?? [], [data]);

  const columns: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Name', fieldName: 'name', minWidth: 150, isResizable: true },
    { key: 'domain', name: 'Domain', fieldName: 'domain', minWidth: 120 },
    { key: 'isActive', name: 'Active', minWidth: 70, onRender: (i: BusinessPlanResponse) => <span>{i.isActive ? 'Yes' : 'No'}</span> },
    { key: 'actions', name: '', minWidth: 200, onRender: (i: BusinessPlanResponse) => <ActionButtons item={i} /> },
  ], []);

  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
        <Text variant="xxLarge">Business Plans</Text>
        <PrimaryButton text="Create" onClick={() => navigate('/admin/business-plans/new')} />
      </Stack>
      <ChoiceGroup label="Status" options={opts} selectedKey={filter} onChange={(_, o) => setFilter((o?.key as Filter) ?? 'all')} />
      {isError && <MessageBar messageBarType={MessageBarType.error}>Failed to load business plans.</MessageBar>}
      {isLoading ? <Spinner label="Loading..." /> : (
        <DetailsList items={items} columns={columns} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={(i) => i.id} onItemInvoked={(i) => navigate(`/admin/business-plans/${i.id}`)} />
      )}
    </Stack>
  );
};

const ActionButtons: FC<{ item: BusinessPlanResponse }> = ({ item }) => {
  const activate = useActivateBusinessPlan(item.id);
  const deactivate = useDeactivateBusinessPlan(item.id);
  return (
    <Stack horizontal tokens={{ childrenGap: 8 }}>
      {item.isActive
        ? <DefaultButton text="Deactivate" onClick={() => deactivate.mutate()} disabled={deactivate.isPending} />
        : <DefaultButton text="Activate" onClick={() => activate.mutate()} disabled={activate.isPending} />
      }
    </Stack>
  );
};

export default BusinessPlanListPage;
