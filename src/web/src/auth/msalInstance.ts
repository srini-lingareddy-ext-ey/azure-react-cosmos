import { PublicClientApplication } from '@azure/msal-browser';
import { createMsalInstance } from './msalConfig';

let msalSingleton: PublicClientApplication | null = null;

/**
 * Single PublicClientApplication per page load (avoids duplicate clients under React StrictMode).
 */
export function getMsalInstance(): PublicClientApplication {
  if (!msalSingleton) {
    msalSingleton = createMsalInstance();
  }
  return msalSingleton;
}
