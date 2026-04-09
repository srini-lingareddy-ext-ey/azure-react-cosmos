import { FC, useMemo, useState } from 'react';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import {
  ChoiceGroup,
  IChoiceGroupOption,
  Link,
  MessageBar,
  MessageBarType,
  PrimaryButton,
  Spinner,
  Stack,
  Text,
  DetailsList,
  IColumn,
  SelectionMode,
  CheckboxVisibility,
} from '@fluentui/react';
import { useTenants } from './hooks/useTenants';
import type { TenantResponse } from './tenantTypes';
import CreateTenantModal from './CreateTenantModal';

type StatusFilter = 'all' | 'active' | 'inactive';

const PAGE_SIZE = 10;

const filterOptions: IChoiceGroupOption[] = [
  { key: 'all', text: 'All' },
  { key: 'active', text: 'Active' },
  { key: 'inactive', text: 'Inactive' },
];

function matchesFilter(t: TenantResponse, filter: StatusFilter): boolean {
  if (filter === 'all') return true;
  if (filter === 'active') return t.status === 'Active';
  return t.status === 'Inactive';
}

function formatDate(iso: string | null | undefined): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString();
}

const TenantRosterPage: FC = () => {
  const navigate = useNavigate();
  const { data, isLoading, isError, error, refetch, isFetching } = useTenants();
  const [filter, setFilter] = useState<StatusFilter>('all');
  const [page, setPage] = useState(0);
  const [createOpen, setCreateOpen] = useState(false);

  const filtered = useMemo(() => {
    const rows = data ?? [];
    return rows.filter((t) => matchesFilter(t, filter));
  }, [data, filter]);

  const pageCount = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const safePage = Math.min(page, pageCount - 1);
  const pageSlice = useMemo(() => {
    const start = safePage * PAGE_SIZE;
    return filtered.slice(start, start + PAGE_SIZE);
  }, [filtered, safePage]);

  const columns: IColumn[] = useMemo(
    () => [
      {
        key: 'name',
        name: 'Name',
        fieldName: 'name',
        minWidth: 120,
        isResizable: true,
      },
      {
        key: 'displayName',
        name: 'Display name',
        fieldName: 'displayName',
        minWidth: 160,
        isResizable: true,
      },
      {
        key: 'status',
        name: 'Status',
        fieldName: 'status',
        minWidth: 90,
        maxWidth: 110,
      },
      {
        key: 'userCount',
        name: 'Users',
        minWidth: 70,
        maxWidth: 90,
        onRender: () => <span title="Not provided by API">—</span>,
      },
      {
        key: 'createdAt',
        name: 'Created',
        minWidth: 160,
        onRender: (item: TenantResponse) => (
          <span>{formatDate(item.createdAt)}</span>
        ),
      },
      {
        key: 'open',
        name: '',
        minWidth: 120,
        onRender: (item: TenantResponse) => (
          <RouterLink to={`/admin/tenants/${item.id}`}>Open</RouterLink>
        ),
      },
    ],
    []
  );

  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
        <Text variant="xxLarge">Tenants</Text>
        <PrimaryButton text="Create tenant" onClick={() => setCreateOpen(true)} />
      </Stack>

      <ChoiceGroup
        label="Status"
        options={filterOptions}
        selectedKey={filter}
        onChange={(_, opt) => {
          setFilter((opt?.key as StatusFilter) ?? 'all');
          setPage(0);
        }}
      />

      {isError ? (
        <MessageBar messageBarType={MessageBarType.error}>
          {(error as Error)?.message ?? 'Failed to load tenants.'}{' '}
          <Link onClick={() => void refetch()}>Retry</Link>
        </MessageBar>
      ) : null}

      {isLoading || (isFetching && !data) ? (
        <Spinner label="Loading tenants…" />
      ) : (
        <>
          <DetailsList
            items={pageSlice}
            columns={columns}
            getKey={(item) => item.id}
            selectionMode={SelectionMode.none}
            checkboxVisibility={CheckboxVisibility.hidden}
            onItemInvoked={(item) => navigate(`/admin/tenants/${item.id}`)}
          />
          <Stack horizontal horizontalAlign="space-between" verticalAlign="center">
            <Text variant="small">
              {filtered.length === 0
                ? 'No tenants match this filter.'
                : `Showing ${safePage * PAGE_SIZE + 1}–${Math.min(
                    (safePage + 1) * PAGE_SIZE,
                    filtered.length
                  )} of ${filtered.length}`}
            </Text>
            <Stack horizontal tokens={{ childrenGap: 8 }}>
              <PrimaryButton
                text="Previous"
                disabled={safePage <= 0}
                onClick={() => setPage((p) => Math.max(0, p - 1))}
              />
              <PrimaryButton
                text="Next"
                disabled={safePage >= pageCount - 1}
                onClick={() => setPage((p) => Math.min(pageCount - 1, p + 1))}
              />
            </Stack>
          </Stack>
        </>
      )}

      <CreateTenantModal
        isOpen={createOpen}
        onDismiss={() => setCreateOpen(false)}
        onCreated={(id) => {
          void refetch();
          navigate(`/admin/tenants/${id}`);
        }}
      />
    </Stack>
  );
};

export default TenantRosterPage;
