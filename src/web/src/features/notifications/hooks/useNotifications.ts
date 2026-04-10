import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchChannels, createChannel, deleteChannel, fetchRoutingRules, createRoutingRule, deleteRoutingRule, fetchMaintenanceWindows, createMaintenanceWindow, deleteMaintenanceWindow, fetchDeliveryLogs } from '../notificationService';

export function useChannels() { return useQuery({ queryKey: ['notifChannels'], queryFn: fetchChannels }); }
export function useCreateChannel() { const qc = useQueryClient(); return useMutation({ mutationFn: createChannel, onSuccess: () => qc.invalidateQueries({ queryKey: ['notifChannels'] }) }); }
export function useDeleteChannel() { const qc = useQueryClient(); return useMutation({ mutationFn: deleteChannel, onSuccess: () => qc.invalidateQueries({ queryKey: ['notifChannels'] }) }); }

export function useRoutingRules() { return useQuery({ queryKey: ['notifRules'], queryFn: fetchRoutingRules }); }
export function useCreateRoutingRule() { const qc = useQueryClient(); return useMutation({ mutationFn: createRoutingRule, onSuccess: () => qc.invalidateQueries({ queryKey: ['notifRules'] }) }); }
export function useDeleteRoutingRule() { const qc = useQueryClient(); return useMutation({ mutationFn: deleteRoutingRule, onSuccess: () => qc.invalidateQueries({ queryKey: ['notifRules'] }) }); }

export function useMaintenanceWindows() { return useQuery({ queryKey: ['notifWindows'], queryFn: fetchMaintenanceWindows }); }
export function useCreateMaintenanceWindow() { const qc = useQueryClient(); return useMutation({ mutationFn: createMaintenanceWindow, onSuccess: () => qc.invalidateQueries({ queryKey: ['notifWindows'] }) }); }
export function useDeleteMaintenanceWindow() { const qc = useQueryClient(); return useMutation({ mutationFn: deleteMaintenanceWindow, onSuccess: () => qc.invalidateQueries({ queryKey: ['notifWindows'] }) }); }

export function useDeliveryLogs(params: { status?: string; from?: string; to?: string; limit?: number; offset?: number }) {
  return useQuery({ queryKey: ['deliveryLogs', params], queryFn: () => fetchDeliveryLogs(params) });
}
