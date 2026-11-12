/// <reference types="vitest/config" />
import { hefestoPlugin } from '@hefesto/plugin-build';
import { defineConfig } from 'vite';

export default defineConfig({
  plugins: [hefestoPlugin()],
  test: { environment: 'jsdom', setupFiles: ['./vitest.setup.ts'] },
});
