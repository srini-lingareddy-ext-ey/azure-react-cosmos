import { FC } from 'react';
import { Spinner, Stack, Text, MessageBar, MessageBarType } from '@fluentui/react';
import { useClassificationRules } from './hooks/useClassificationRules';
import type { ClassificationRuleView } from './classificationRulesService';

const outcomeColor = (o: string): string | undefined => {
  const lower = o.toLowerCase();
  if (lower === 'incident') return '#d13438';
  if (lower === 'alert') return '#c19c00';
  if (lower === 'availabilityissue') return '#e81123';
  return undefined;
};

const ClassificationRulesPage: FC = () => {
  const { data, isLoading, isError, refetch } = useClassificationRules();

  return (
    <Stack tokens={{ childrenGap: 16 }}>
      <Text variant="xxLarge">Classification Rules</Text>
      <MessageBar messageBarType={MessageBarType.info}>
        Rules are managed via CI/CD deployment. Contact your platform administrator to request changes.
      </MessageBar>
      {isLoading ? <Spinner label="Loading rules..." /> : isError ? (
        <Stack horizontal tokens={{ childrenGap: 8 }}><Text>Error loading rules.</Text><button onClick={() => refetch()}>Retry</button></Stack>
      ) : !data?.length ? <Text>No classification rules configured.</Text> : (
        <Stack tokens={{ childrenGap: 12 }}>
          {data.map((rule: ClassificationRuleView, idx: number) => (
            <Stack key={rule.ruleId} styles={{ root: { padding: 12, border: '1px solid #e0e0e0', borderRadius: 4 } }}>
              <Stack horizontal tokens={{ childrenGap: 8 }} verticalAlign="center">
                <Text variant="large" styles={{ root: { fontWeight: 700 } }}>#{idx + 1}</Text>
                <Text variant="mediumPlus">{rule.description}</Text>
                <Text styles={{ root: { color: outcomeColor(rule.outcome), fontWeight: 600, marginLeft: 'auto' } }}>{rule.outcome}</Text>
              </Stack>
              <Text variant="small" styles={{ root: { color: '#666', marginTop: 4 } }}>
                {rule.conditions.map((c: { field: string; operator: string; value: string }) => `${c.field} ${c.operator} ${c.value}`).join(' AND ')}
              </Text>
            </Stack>
          ))}
        </Stack>
      )}
    </Stack>
  );
};

export default ClassificationRulesPage;
