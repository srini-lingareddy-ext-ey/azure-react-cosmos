import { FC, PropsWithChildren } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { ThemeProvider } from '@fluentui/react';
import { DarkTheme } from '../../styles/theme';
import Telemetry from '../../components/shared/Telemetry';
import config from '../../config';
import MsalAppProvider from '../../auth/MsalAppProvider';
import { AuthContextProvider } from '../../auth/AuthContext';
import { ToastProvider } from '../../components/shared/ToastProvider';
import AppQueryProvider from '../../query/AppQueryProvider';

const AppProviders: FC<PropsWithChildren<unknown>> = ({ children }) => {
  const routerTree = (
    <BrowserRouter>
      <Telemetry>{children}</Telemetry>
    </BrowserRouter>
  );

  return (
    <ThemeProvider applyTo="body" theme={DarkTheme}>
      <AppQueryProvider>
        <ToastProvider>
          {config.auth.isEnabled ? (
            <MsalAppProvider>
              <AuthContextProvider>{routerTree}</AuthContextProvider>
            </MsalAppProvider>
          ) : (
            <AuthContextProvider>{routerTree}</AuthContextProvider>
          )}
        </ToastProvider>
      </AppQueryProvider>
    </ThemeProvider>
  );
};

export default AppProviders;
