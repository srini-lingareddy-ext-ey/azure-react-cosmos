import { describe, it, expect } from 'vitest';
import { isAuthMeUser, isUserNotProvisionedResponse } from './authMe';

describe('isAuthMeUser', () => {
  it('accepts a profile with userId', () => {
    expect(isAuthMeUser({ userId: 'oid-1', tenants: [] })).toBe(true);
  });

  it('accepts a profile with id', () => {
    expect(isAuthMeUser({ id: 'oid-1' })).toBe(true);
  });

  it('accepts a profile with non-empty tenants only', () => {
    expect(
      isAuthMeUser({
        tenants: [{ tenantId: 't1', tenantName: 'A', role: 'Viewer' }],
      })
    ).toBe(true);
  });

  it('rejects API error envelopes', () => {
    expect(
      isAuthMeUser({
        traceId: 'x',
        errorCode: 'USER_NOT_PROVISIONED',
        message: 'no',
      })
    ).toBe(false);
  });

  it('rejects arbitrary objects with only id number', () => {
    expect(isAuthMeUser({ id: 1 })).toBe(false);
  });

  it('rejects empty tenants array without userId or id', () => {
    expect(isAuthMeUser({ tenants: [] })).toBe(false);
  });

  it('rejects empty object', () => {
    expect(isAuthMeUser({})).toBe(false);
  });

  it('rejects tenant with missing tenantId', () => {
    expect(isAuthMeUser({ tenants: [{ tenantName: 'x' }] })).toBe(false);
  });
});

describe('isUserNotProvisionedResponse', () => {
  it('detects USER_NOT_PROVISIONED', () => {
    expect(
      isUserNotProvisionedResponse({
        errorCode: 'USER_NOT_PROVISIONED',
        message: 'x',
      })
    ).toBe(true);
  });

  it('returns false for success-like shapes', () => {
    expect(isUserNotProvisionedResponse({ userId: 'u' })).toBe(false);
  });
});
