import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  // Match Entra SPA redirect URIs (typically http://localhost:5173). Without strictPort,
  // Vite picks the next free port (e.g. 5174) and MSAL sends that origin → AADSTS50011.
  server: {
    port: 5173,
    strictPort: true,
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/setupTests.ts'],
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html'],
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/setupTests.ts', 'src/**/*.d.ts', 'src/**/*.{test,spec}.{ts,tsx}', 'src/index.tsx', 'src/reportWebVitals.ts'],
    },
  },
})
