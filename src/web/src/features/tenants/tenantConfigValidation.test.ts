import { describe, it, expect } from 'vitest';
import { healthWeightsSumMessage } from './tenantConfigValidation';

describe('healthWeightsSumMessage', () => {
  it('returns null for empty weights', () => {
    expect(healthWeightsSumMessage({})).toBeNull();
    expect(healthWeightsSumMessage(undefined)).toBeNull();
  });

  it('returns null when sum is 100', () => {
    expect(healthWeightsSumMessage({ composite: 100 })).toBeNull();
    expect(healthWeightsSumMessage({ a: 60, b: 40 })).toBeNull();
  });

  it('returns message when sum is not 100', () => {
    const m = healthWeightsSumMessage({ a: 50 });
    expect(m).toContain('100');
    expect(m).toContain('50');
  });
});
