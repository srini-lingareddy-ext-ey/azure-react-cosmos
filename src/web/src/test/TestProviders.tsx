import { FC, PropsWithChildren, useState } from 'react';
import { ThemeProvider } from '@fluentui/react';
import { DarkTheme } from '../styles/theme';
import { MemoryRouter } from 'react-router-dom';
import { QueryClientProvider } from '@tanstack/react-query';
import { createTestQueryClient } from '../query/createTestQueryClient';
import { AuthContextProvider } from '../auth/AuthContext';

export const TestProviders: FC<PropsWithChildren> = ({ children }) => {
  const [queryClient] = useState(createTestQueryClient);
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider applyTo="body" theme={DarkTheme}>
        <AuthContextProvider>
          <MemoryRouter>{children}</MemoryRouter>
        </AuthContextProvider>
      </ThemeProvider>
    </QueryClientProvider>
  );
};
