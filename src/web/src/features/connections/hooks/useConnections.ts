import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { listConnections, createConnection, deleteConnection, testConnection } from '../connectionService';
import type { CreateConnectionRequest } from '../connectionTypes';
export const connectionsKey = ['connections'] as const;
export function useConnections() { return useQuery({ queryKey: connectionsKey, queryFn: listConnections }); }
export function useCreateConnection() { const qc = useQueryClient(); return useMutation({ mutationFn: (b: CreateConnectionRequest) => createConnection(b), onSuccess: () => { void qc.invalidateQueries({ queryKey: connectionsKey }); } }); }
export function useDeleteConnection() { const qc = useQueryClient(); return useMutation({ mutationFn: (id: string) => deleteConnection(id), onSuccess: () => { void qc.invalidateQueries({ queryKey: connectionsKey }); } }); }
export function useTestConnection() { return useMutation({ mutationFn: (id: string) => testConnection(id) }); }
