import { defineConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath, URL } from 'node:url';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [vue(), tailwindcss()],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
      },
    },
    test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: ['./tests/unit/setup.ts'],
      exclude: ['node_modules/', '**/e2e/**', '**/playwright/**', '**/*.e2e.{test,spec}.{js,ts,jsx,tsx}'],
      coverage: {
        reporter: ['text', 'json', 'html'],
        exclude: ['node_modules/', 'tests/', '**/*.d.ts', 'src/main.ts'],
      },
    },
    server: {
      port: 3000,
      proxy: {
        '/api': {
          target: env.VITE_API_BASE_URL || 'http://localhost:7000',
          changeOrigin: true,
          secure: false,
        },
      },
    },
    build: {
      target: 'esnext',
      sourcemap: false,
      minify: 'esbuild',
      cssCodeSplit: false,
    },
    define: {
      'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV),
      global: 'globalThis',
    },
  };
});
