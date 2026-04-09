import { FC } from 'react';
import { Stack, Text } from '@fluentui/react';

const UnprovisionedUserPage: FC = () => {
  return (
    <Stack
      horizontalAlign="center"
      verticalAlign="center"
      tokens={{ childrenGap: 16 }}
      styles={{ root: { minHeight: '100vh', padding: 24 } }}
    >
      <Stack tokens={{ childrenGap: 12 }} styles={{ root: { maxWidth: 520 } }}>
        <Text variant="xxLarge">Account not provisioned</Text>
        <Text variant="medium">
          You signed in successfully, but your account is not yet assigned to an
          organization in this application.
        </Text>
        <Text variant="medium">
          Ask your administrator to grant you access to a tenant. After they
          assign a role, refresh this page or sign out and sign in again.
        </Text>
      </Stack>
    </Stack>
  );
};

export default UnprovisionedUserPage;
