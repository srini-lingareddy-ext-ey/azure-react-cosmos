import { FC, PropsWithChildren, useState } from 'react';
import { ThemeProvider } from '@fluentui/react';
import { DarkTheme } from '../styles/theme';
import { MemoryRouter } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { createTestQueryClient } from '../query/createTestQueryClient';
import { AuthContextProvider } from '../auth/AuthContext';
import { ToastProvider } from '../components/shared/ToastProvider';

export const TestProviders: FC<PropsWithChildren> = ({ children }) => {
  const [queryClient] = useState(createTestQueryClient);
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider applyTo="body" theme={DarkTheme}>
        <ToastProvider>
          <AuthContextProvider>
            <MemoryRouter>{children}</MemoryRouter>
          </AuthContextProvider>
        </ToastProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
};
