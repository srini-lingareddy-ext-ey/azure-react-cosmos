const EPS = 0.0001;

/**
 * Returns an error message when stored weights exist and do not sum to 100 (WO-9 server rule).
 */
export function healthWeightsSumMessage(
  weights: Record<string, number> | undefined | null
): string | null {
  if (!weights || Object.keys(weights).length === 0) {
    return null;
  }
  const sum = Object.values(weights).reduce((a, b) => a + b, 0);
  if (Math.abs(sum - 100) > EPS) {
    return `Health score weights must sum to 100 (current total: ${sum.toFixed(3)}).`;
  }
  return null;
}
