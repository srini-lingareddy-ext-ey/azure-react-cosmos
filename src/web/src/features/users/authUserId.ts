import type { AuthMeUser } from '../../auth/authMe';

export function getAuthUserId(user: AuthMeUser | null): string | null {
  if (!user) return null;
  const id = user.userId ?? user.id;
  return typeof id === 'string' && id.length > 0 ? id : null;
}
