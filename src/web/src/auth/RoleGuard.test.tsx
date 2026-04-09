import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../test-utils';
import RoleGuard from './RoleGuard';
import type { AuthContextValue } from './AuthContext';

const baseAuth: AuthContextValue = {
  user: null,
  isAuthenticated: true,
  isLoading: false,
  apiUnauthorized: false,
  isUnprovisioned: false,
  activeRole: null,
  activeTenant: null,
  setActiveTenant: () => {},
  logout: () => {},
};

vi.mock('./useAuth', () => ({
  useAuth: vi.fn(() => baseAuth),
}));

vi.mock('../config', () => ({
  default: {
    auth: { isEnabled: true },
    api: { baseUrl: 'http://localhost:3100' },
    observability: { connectionString: '' },
  },
}));

import { useAuth } from './useAuth';

describe('RoleGuard', () => {
  beforeEach(() => {
    vi.mocked(useAuth).mockReturnValue({
      ...baseAuth,
      activeRole: 'Viewer',
    });
  });

  it('renders children when active role is in requiredRoles', () => {
    vi.mocked(useAuth).mockReturnValue({
      ...baseAuth,
      activeRole: 'Admin',
    });
    renderWithProviders(
      <RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}>
        <span>Secret</span>
      </RoleGuard>
    );
    expect(screen.getByText('Secret')).toBeInTheDocument();
  });

  it('renders AccessDeniedView when role does not match', () => {
    vi.mocked(useAuth).mockReturnValue({
      ...baseAuth,
      activeRole: 'Viewer',
    });
    renderWithProviders(
      <RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}>
        <span>Secret</span>
      </RoleGuard>
    );
    expect(screen.queryByText('Secret')).not.toBeInTheDocument();
    expect(screen.getByText(/Access denied/i)).toBeInTheDocument();
  });
});
