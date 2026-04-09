import { FC } from 'react';
import { Stack, Text } from '@fluentui/react';

const AccessDeniedView: FC = () => {
  return (
    <Stack
      tokens={{ childrenGap: 8 }}
      styles={{
        root: {
          borderLeft: '3px solid #a4262c',
          padding: '12px 16px',
          backgroundColor: 'rgba(164, 38, 44, 0.08)',
        },
      }}
      role="status"
    >
      <Text variant="mediumPlus">Access denied</Text>
      <Text variant="small">
        You don&apos;t have permission to view this section. Contact your
        administrator if you need access.
      </Text>
    </Stack>
  );
};

export default AccessDeniedView;
