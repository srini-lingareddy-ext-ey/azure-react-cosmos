import { FC, useMemo } from 'react';
import { Spinner, Stack, Text } from '@fluentui/react';
import { useSLATrend } from './hooks/useSLA';
import type { SLATrendPoint } from './slaService';

interface SLATrendChartProps {
  days?: number;
}

const CHART_WIDTH = 600;
const CHART_HEIGHT = 200;
const PADDING = { top: 20, right: 20, bottom: 30, left: 40 };
const COLORS = [
  '#0078d4', '#e81123', '#107c10', '#ff8c00', '#5c2d91',
  '#008575', '#d83b01', '#004b1c',
];

const SLATrendChart: FC<SLATrendChartProps> = ({ days = 7 }) => {
  const { data, isLoading, isError, refetch } = useSLATrend(days);

  const { seriesMap, allDates, maxVal, note } = useMemo(() => {
    const trend = data?.trend ?? [];
    const map = new Map<string, SLATrendPoint[]>();
    const dates = new Set<string>();
    trend.forEach((p: SLATrendPoint) => {
      dates.add(p.date);
      const arr = map.get(p.businessPlan) ?? [];
      arr.push(p);
      map.set(p.businessPlan, arr);
    });
    const sortedDates = Array.from(dates).sort();
    let max = 100;
    trend.forEach((p: SLATrendPoint) => {
      if (p.complianceRate > max) max = p.complianceRate;
    });
    return {
      seriesMap: map,
      allDates: sortedDates,
      maxVal: max,
      note: data?.dataAvailabilityNote,
    };
  }, [data]);

  if (isLoading) return <Spinner label="Loading trend..." />;
  if (isError)
    return (
      <Stack horizontal tokens={{ childrenGap: 8 }}>
        <Text>Error loading trend data.</Text>
        <button onClick={() => refetch()}>Retry</button>
      </Stack>
    );
  if (allDates.length === 0) return <Text>No trend data available.</Text>;

  const plotW = CHART_WIDTH - PADDING.left - PADDING.right;
  const plotH = CHART_HEIGHT - PADDING.top - PADDING.bottom;
  const xScale = (i: number) => PADDING.left + (allDates.length > 1 ? (i / (allDates.length - 1)) * plotW : plotW / 2);
  const yScale = (v: number) => PADDING.top + plotH - (v / maxVal) * plotH;

  const seriesEntries = Array.from(seriesMap.entries());

  return (
    <Stack tokens={{ childrenGap: 8 }}>
      <Text variant="mediumPlus" styles={{ root: { fontWeight: 600 } }}>
        Compliance Trend ({days} days)
      </Text>
      <svg width={CHART_WIDTH} height={CHART_HEIGHT} style={{ border: '1px solid #e0e0e0', borderRadius: 4 }}>
        {/* Y-axis labels */}
        {[0, 25, 50, 75, 100].map((v) => (
          <g key={`y-${v}`}>
            <line x1={PADDING.left} y1={yScale(v)} x2={PADDING.left + plotW} y2={yScale(v)} stroke="#e0e0e0" />
            <text x={PADDING.left - 4} y={yScale(v) + 4} textAnchor="end" fontSize={10} fill="#666">{v}%</text>
          </g>
        ))}
        {/* X-axis labels */}
        {allDates.map((d, i) => (
          <text key={d} x={xScale(i)} y={CHART_HEIGHT - 8} textAnchor="middle" fontSize={10} fill="#666">
            {d.slice(5)}
          </text>
        ))}
        {/* Series lines */}
        {seriesEntries.map(([bp, points], si) => {
          const sorted = [...points].sort((a, b) => a.date.localeCompare(b.date));
          const pathD = sorted
            .map((p, pi) => {
              const xi = allDates.indexOf(p.date);
              const x = xScale(xi);
              const y = yScale(p.complianceRate);
              return `${pi === 0 ? 'M' : 'L'} ${x} ${y}`;
            })
            .join(' ');
          return (
            <g key={bp}>
              <path d={pathD} fill="none" stroke={COLORS[si % COLORS.length]} strokeWidth={2} />
              {sorted.map((p) => {
                const xi = allDates.indexOf(p.date);
                return (
                  <circle
                    key={`${bp}-${p.date}`}
                    cx={xScale(xi)}
                    cy={yScale(p.complianceRate)}
                    r={3}
                    fill={COLORS[si % COLORS.length]}
                  />
                );
              })}
            </g>
          );
        })}
      </svg>
      {/* Legend */}
      <Stack horizontal tokens={{ childrenGap: 16 }}>
        {seriesEntries.map(([bp], si) => (
          <Stack key={bp} horizontal tokens={{ childrenGap: 4 }} verticalAlign="center">
            <div style={{ width: 12, height: 12, borderRadius: 2, backgroundColor: COLORS[si % COLORS.length] }} />
            <Text variant="small">{bp}</Text>
          </Stack>
        ))}
      </Stack>
      {note && (
        <Text variant="small" styles={{ root: { color: '#666', fontStyle: 'italic' } }}>
          {note}
        </Text>
      )}
    </Stack>
  );
};

export default SLATrendChart;
