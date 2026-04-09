import { FC } from 'react';
import { PrimaryButton, Stack, Text } from '@fluentui/react';
import { useMsal } from '@azure/msal-react';
import { getLoginRequestScopes } from './msalConfig';

/**
 * Shown when GET /api/v1/auth/me returns 401 so the user can start a fresh interactive sign-in.
 */
const SessionExpiredPage: FC = () => {
  const { instance } = useMsal();

  return (
    <Stack
      horizontalAlign="center"
      verticalAlign="center"
      tokens={{ childrenGap: 16 }}
      styles={{ root: { minHeight: '100vh', padding: 24 } }}
    >
      <Stack tokens={{ childrenGap: 12 }} styles={{ root: { maxWidth: 520 } }}>
        <Text variant="xxLarge">Sign-in required</Text>
        <Text variant="medium">
          Your session could not be verified with the application. Sign in again
          to continue.
        </Text>
        <PrimaryButton
          text="Sign in again"
          onClick={() => {
            void instance.loginRedirect({
              scopes: getLoginRequestScopes(),
              prompt: 'login',
            });
          }}
        />
      </Stack>
    </Stack>
  );
};

export default SessionExpiredPage;
