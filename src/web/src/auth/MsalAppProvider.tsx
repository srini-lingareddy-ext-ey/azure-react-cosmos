import { FC, PropsWithChildren, useEffect, useState } from 'react';
import { MsalProvider } from '@azure/msal-react';
import { Spinner, Stack } from '@fluentui/react';
import { getMsalInstance } from './msalInstance';

/**
 * Initializes MSAL before rendering children (required for redirect flows).
 */
const MsalAppProvider: FC<PropsWithChildren> = ({ children }) => {
  const [ready, setReady] = useState(false);
  const instance = getMsalInstance();

  useEffect(() => {
    let cancelled = false;
    instance.initialize().then(() => {
      if (!cancelled) {
        setReady(true);
      }
    });
    return () => {
      cancelled = true;
    };
  }, [instance]);

  if (!ready) {
    return (
      <Stack
        horizontalAlign="center"
        verticalAlign="center"
        styles={{ root: { minHeight: '100vh' } }}
      >
        <Spinner label="Loading sign-in…" />
      </Stack>
    );
  }

  return <MsalProvider instance={instance}>{children}</MsalProvider>;
};

export default MsalAppProvider;
