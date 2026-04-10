import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { getCatalog, listConnectors, enableConnector, disableConnector, testConnector, getConnectorLogs } from '../connectorService';
export const connectorsKey = ['connectors'] as const;
export const catalogKey = ['connectorCatalog'] as const;
export const logsKey = (id: string) => ['connectorLogs', id] as const;
export function useConnectorCatalog() { return useQuery({ queryKey: catalogKey, queryFn: getCatalog }); }
export function useConnectors() { return useQuery({ queryKey: connectorsKey, queryFn: listConnectors }); }
export function useEnableConnector(id: string) { const qc = useQueryClient(); return useMutation({ mutationFn: () => enableConnector(id), onSuccess: () => { void qc.invalidateQueries({ queryKey: connectorsKey }); } }); }
export function useDisableConnector(id: string) { const qc = useQueryClient(); return useMutation({ mutationFn: () => disableConnector(id), onSuccess: () => { void qc.invalidateQueries({ queryKey: connectorsKey }); } }); }
export function useTestConnector() { return useMutation({ mutationFn: (id: string) => testConnector(id) }); }
export function useConnectorLogs(id: string | undefined) { return useQuery({ queryKey: logsKey(id ?? '_'), queryFn: () => getConnectorLogs(id!), enabled: Boolean(id) }); }
