/**
 * Centralized API client for all backend HTTP communication (REQ-FOUNDATION-016).
 * Feature services use this client; components must not call raw fetch/axios.
 */
import axios, {
  AxiosInstance,
  InternalAxiosRequestConfig,
} from 'axios';
import config from '../config';
import { mapAxiosError } from './apiErrors';

export interface QueryOptions {
  top?: number;
  skip?: number;
}

export interface Entity {
  id?: string;
  created?: Date;
  updated?: Date;
}

export const CORRELATION_HEADER = 'X-Correlation-ID';

/** Sent on authenticated requests when tenant context is selected (WO-6). */
export const TENANT_ID_HEADER = 'X-Tenant-Id';

type TokenGetter = () => Promise<string | null>;

let tokenGetter: TokenGetter = async () => null;

type TenantIdGetter = () => string | null;

let tenantIdGetter: TenantIdGetter = () => null;

/** Optional: use a stable correlation id per user action (e.g. from telemetry). Default: new UUID per request. */
export type CorrelationIdProvider = () => string;
let correlationIdProvider: CorrelationIdProvider | null = null;

export function setCorrelationIdProvider(provider: CorrelationIdProvider | null): void {
  correlationIdProvider = provider;
}

/**
 * Registered from AuthContext when MSAL is active.
 */
export function registerApiTokenGetter(getter: TokenGetter): void {
  tokenGetter = getter;
}

export function resetApiTokenGetter(): void {
  tokenGetter = async () => null;
}

export function registerTenantIdGetter(getter: TenantIdGetter): void {
  tenantIdGetter = getter;
}

export function resetTenantIdGetter(): void {
  tenantIdGetter = () => null;
}

type ConfigWithCorrelation = InternalAxiosRequestConfig & {
  _correlationId?: string;
};

export function attachBearerTokenInterceptor(instance: AxiosInstance): void {
  instance.interceptors.request.use(
    async (cfg: InternalAxiosRequestConfig) => {
      if (!config.auth.isEnabled) {
        return cfg;
      }
      try {
        const token = await tokenGetter();
        if (token) {
          cfg.headers.Authorization = `Bearer ${token}`;
        }
      } catch {
        /* non-fatal */
      }
      return cfg;
    },
    (err) => Promise.reject(err)
  );
}

export function attachTenantIdHeaderInterceptor(instance: AxiosInstance): void {
  instance.interceptors.request.use(
    (cfg: InternalAxiosRequestConfig) => {
      if (!config.auth.isEnabled) {
        return cfg;
      }
      const tenantId = tenantIdGetter();
      if (tenantId) {
        cfg.headers.set(TENANT_ID_HEADER, tenantId);
      }
      return cfg;
    },
    (err) => Promise.reject(err)
  );
}

function attachCorrelationInterceptor(instance: AxiosInstance): void {
  instance.interceptors.request.use((cfg: InternalAxiosRequestConfig) => {
    const c = cfg as ConfigWithCorrelation;
    const id = correlationIdProvider?.() ?? crypto.randomUUID();
    c.headers.set(CORRELATION_HEADER, id);
    c._correlationId = id;
    return c;
  });
}

const MUTATING = new Set(['post', 'put', 'patch', 'delete']);

function attachIdempotencyInterceptor(instance: AxiosInstance): void {
  instance.interceptors.request.use((cfg: InternalAxiosRequestConfig) => {
    const key = cfg.idempotencyKey;
    if (
      key &&
      cfg.method &&
      MUTATING.has(cfg.method.toLowerCase())
    ) {
      cfg.headers.set('Idempotency-Key', key);
    }
    return cfg;
  });
}

function attachErrorResponseInterceptor(instance: AxiosInstance): void {
  instance.interceptors.response.use(
    (res) => res,
    (err) => {
      if (axios.isAxiosError(err)) {
        return Promise.reject(mapAxiosError(err));
      }
      return Promise.reject(err);
    }
  );
}

/**
 * Applies correlation, Bearer (MSAL), idempotency headers, and normalized errors to an instance.
 */
export function applyStandardApiInterceptors(instance: AxiosInstance): void {
  attachCorrelationInterceptor(instance);
  attachBearerTokenInterceptor(instance);
  attachTenantIdHeaderInterceptor(instance);
  attachIdempotencyInterceptor(instance);
  attachErrorResponseInterceptor(instance);
}

export function createApiInstance(overrides?: { baseURL?: string }): AxiosInstance {
  const instance = axios.create({
    baseURL: overrides?.baseURL ?? config.api.baseUrl,
    headers: {
      'Content-Type': 'application/json',
    },
  });
  applyStandardApiInterceptors(instance);
  return instance;
}

export const apiClient = createApiInstance();

/**
 * Generic REST helper for resource-oriented routes. Prefer feature services + {@link apiClient} for new code.
 */
export abstract class RestService<T extends Entity> {
  protected client: AxiosInstance;

  public constructor(baseRoute: string) {
    const base = `${config.api.baseUrl.replace(/\/$/, '')}/${baseRoute.replace(/^\//, '')}`;
    this.client = createApiInstance({ baseURL: base });
  }

  public async getList(queryOptions?: QueryOptions): Promise<T[]> {
    const response = await this.client.request<T[]>({
      method: 'GET',
      data: queryOptions,
    });
    return response.data;
  }

  public async get(id: string): Promise<T> {
    const response = await this.client.request<T>({
      method: 'GET',
      url: id,
    });
    return response.data;
  }

  public async save(entity: T): Promise<T> {
    return entity.id ? await this.put(entity) : await this.post(entity);
  }

  public async delete(id: string): Promise<void> {
    await this.client.request<void>({
      method: 'DELETE',
      url: id,
    });
  }

  private async post(entity: T): Promise<T> {
    const response = await this.client.request<T>({
      method: 'POST',
      data: entity,
    });
    return response.data;
  }

  private async put(entity: T): Promise<T> {
    const response = await this.client.request<T>({
      method: 'PUT',
      url: entity.id,
      data: entity,
    });
    return response.data;
  }

  public async patch(id: string, entity: Partial<T>): Promise<T> {
    const response = await this.client.request<T>({
      method: 'PATCH',
      url: id,
      data: entity,
    });
    return response.data;
  }
}
