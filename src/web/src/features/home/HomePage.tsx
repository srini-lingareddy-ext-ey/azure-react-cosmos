import { FC } from 'react';
import { Stack, Text } from '@fluentui/react';
import { stackPadding } from '../../styles/styles';
import RoleGuard from '../../auth/RoleGuard';

const HomePage: FC = () => {
  return (
    <Stack tokens={stackPadding}>
      <Text variant="xxLarge">Add your own application code</Text>
      <Text variant="medium">
        This is a good start for a minimal scaffold with React, C# API, and Cosmos
        DB. Replace this page and add your own features. See the README or{' '}
        <a href="https://learn.microsoft.com/azure/developer/azure-developer-cli/">
          Azure Developer CLI docs
        </a>{' '}
        to get started.
      </Text>
      <RoleGuard requiredRoles={['Admin', 'PlatformAdmin']}>
        <Text variant="medium">
          Admin tools: this line is only shown for Admin or Platform Admin roles.
        </Text>
      </RoleGuard>
    </Stack>
  );
};

export default HomePage;
