/// <reference types="vitest" />
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import '@testing-library/jest-dom/vitest';

export default defineConfig({
    plugins: [react()],
    base: './',
    css: {
        transformer: 'postcss',
        lightningcss: { }
    },
    build: {
        outDir: 'dist',
        assetsDir: 'assets',
        emptyOutDir: true
    },
    test: {
        environment: 'jsdom',
        globals: true,
        setupFiles: './setupTests.ts',
    }
})