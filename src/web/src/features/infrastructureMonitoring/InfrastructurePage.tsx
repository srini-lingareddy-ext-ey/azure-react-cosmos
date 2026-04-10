import { FC, useMemo, useState } from 'react';
import { DetailsList, IColumn, SelectionMode, CheckboxVisibility, Spinner, Stack, Text, Dropdown, IDropdownOption, Panel, PanelType, Icon } from '@fluentui/react';
import { useInfraStatus, useComponentNodes, useNodeMetrics, useProductAvailability } from './hooks/useInfrastructureMonitoring';
import type { ComponentHealthDto, ProductHealthDto, NodeStatusDto, NodeMetricDto } from './infrastructureMonitoringService';
const statusOpts: IDropdownOption[] = [{ key: '', text: 'All' }, { key: 'healthy', text: 'Healthy' }, { key: 'warning', text: 'Warning' }, { key: 'critical', text: 'Critical' }];
const sColor = (s: string) => s === 'Critical' ? 'red' : s === 'Warning' ? '#c19c00' : s === 'Healthy' ? 'green' : 'grey';
const InfrastructurePage: FC = () => {
  const [filter, setFilter] = useState('');
  const { data, isLoading, isError, refetch } = useInfraStatus(filter || undefined);
  const [expandedComp, setExpandedComp] = useState<string | null>(null);
  const [selectedNode, setSelectedNode] = useState<string | null>(null);
  const [selectedProduct, setSelectedProduct] = useState<string | null>(null);
  const nodes = useComponentNodes(expandedComp ?? '');
  const metrics = useNodeMetrics(selectedNode ?? '');
  const prodAvail = useProductAvailability(selectedProduct ?? '');
  const components = useMemo(() => (data?.components ?? []).sort((a: ComponentHealthDto, b: ComponentHealthDto) => { const o: Record<string, number> = { Critical: 0, Warning: 1, Healthy: 2, Unknown: 3 }; return (o[a.status] ?? 9) - (o[b.status] ?? 9); }), [data]);
  const products = useMemo(() => data?.products ?? [], [data]);
  const compCols: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Component', minWidth: 180, onRender: (i: ComponentHealthDto) => <Text style={{ cursor: 'pointer' }} onClick={() => setExpandedComp(p => p === i.componentId ? null : i.componentId)}>{i.componentName} {i.isStale && <Icon iconName="Clock" />}</Text> },
    { key: 'type', name: 'Type', fieldName: 'componentType', minWidth: 80 },
    { key: 'status', name: 'Status', minWidth: 80, onRender: (i: ComponentHealthDto) => <Text styles={{ root: { color: sColor(i.status), fontWeight: 600 } }}>{i.status}</Text> },
    { key: 'nodes', name: 'Nodes', minWidth: 100, onRender: (i: ComponentHealthDto) => <Text>{i.nodeCount} ({i.unhealthyNodeCount} unhealthy)</Text> },
  ], []);
  const prodCols: IColumn[] = useMemo(() => [
    { key: 'name', name: 'Product', minWidth: 180, onRender: (i: ProductHealthDto) => <Text style={{ cursor: 'pointer' }} onClick={() => setSelectedProduct(i.productId)}>{i.productName}</Text> },
    { key: 'avail', name: 'Availability 24h', minWidth: 100, onRender: (i: ProductHealthDto) => <Text>{i.isStale ? 'N/A' : `${i.availability24h.toFixed(2)}%`}</Text> },
    { key: 'status', name: 'Status', minWidth: 80, onRender: (i: ProductHealthDto) => <Text styles={{ root: { color: sColor(i.status), fontWeight: 600 } }}>{i.status}</Text> },
  ], []);
  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Infrastructure Monitoring</Text>
      <Dropdown label="Status" options={statusOpts} selectedKey={filter} onChange={(_, o) => setFilter((o?.key as string) ?? '')} styles={{ root: { maxWidth: 200 } }} />
      {isLoading ? <Spinner /> : isError ? <Stack><Text>Error loading infrastructure data.</Text><button onClick={() => refetch()}>Retry</button></Stack> : <>
        <Text variant="xLarge">Components</Text>
        {components.length === 0 ? <Text>No components registered</Text> : <DetailsList items={components} columns={compCols} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.componentId} />}
        {expandedComp && <Stack styles={{ root: { padding: '0 16px' } }}><Text variant="large">Nodes</Text>{nodes.isLoading ? <Spinner /> : nodes.data?.map((n: NodeStatusDto) => <Stack key={n.nodeId} horizontal tokens={{ childrenGap: 12 }} styles={{ root: { padding: 4, cursor: 'pointer' } }} onClick={() => setSelectedNode(n.nodeId)}><Text styles={{ root: { color: sColor(n.status) } }}>{n.nodeName}</Text><Text>{n.status}</Text></Stack>)}</Stack>}
        <Text variant="xLarge">Products</Text>
        {products.length === 0 ? <Text>No products registered</Text> : <DetailsList items={products} columns={prodCols} selectionMode={SelectionMode.none} checkboxVisibility={CheckboxVisibility.hidden} getKey={i => i.productId} />}
      </>}
      <Panel isOpen={!!selectedNode} onDismiss={() => setSelectedNode(null)} type={PanelType.medium} headerText="Node Metrics">
        {metrics.isLoading ? <Spinner /> : metrics.data?.map((m: NodeMetricDto) => <Stack key={m.metricName} tokens={{ childrenGap: 4 }} styles={{ root: { padding: 8, borderBottom: '1px solid #e0e0e0' } }}><Text variant="mediumPlus">{m.displayName ?? m.metricName} ({m.unit ?? ''})</Text><Text styles={{ root: { color: sColor(m.status) } }}>Current: {m.currentValue != null ? m.currentValue.toFixed(1) : 'N/A'}{m.warningThreshold != null && ` | Warning: ${m.warningThreshold}`}{m.criticalThreshold != null && ` | Critical: ${m.criticalThreshold}`}</Text><Text>Sparkline: {m.sparkline.length} points</Text></Stack>)}
      </Panel>
      <Panel isOpen={!!selectedProduct} onDismiss={() => setSelectedProduct(null)} type={PanelType.medium} headerText="Product Availability">
        {prodAvail.isLoading ? <Spinner /> : prodAvail.data ? <Stack tokens={{ childrenGap: 8 }}><Text variant="large">Availability: {prodAvail.data.status === 'Unknown' ? 'N/A' : `${prodAvail.data.availability24h.toFixed(2)}%`}</Text><Text>Status: {prodAvail.data.status}</Text>{prodAvail.data.status === 'Unknown' && <Text>Heartbeat data unavailable - connector may be disconnected</Text>}<Text variant="mediumPlus">Trend ({prodAvail.data.trend.length} days)</Text>{prodAvail.data.trend.map((t: { date: string; availabilityPercent: number }) => <Text key={t.date}>{t.date}: {t.availabilityPercent.toFixed(2)}%</Text>)}</Stack> : null}
      </Panel>
    </Stack>
  );
};
export default InfrastructurePage;

