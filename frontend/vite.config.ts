import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import path from 'node:path'
import { defineConfig } from 'vitest/config'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      '@': path.resolve(import.meta.dirname, './src'),
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./src/test/setup.ts'],
    include: ['src/**/*.test.{ts,tsx}'],
    coverage: {
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/components/ui/**', 'src/main.tsx', 'src/test/**', 'src/**/*.test.{ts,tsx}'],
    },
  },
  server: {
    // Reachable from a phone on the same network (camera access still needs HTTPS).
    host: true,
    // Lets a dev tunnel (e.g. cloudflared) reach the server by its public hostname.
    allowedHosts: ['.trycloudflare.com'],
    proxy: {
      '/api': {
        target: process.env.VITE_API_PROXY_TARGET ?? 'https://localhost:7028',
        secure: false,
        changeOrigin: true,
      },
    },
  },
})
