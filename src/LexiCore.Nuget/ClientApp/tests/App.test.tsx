import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import '@testing-library/jest-dom/vitest';
import App from "../src/App";

global.fetch = vi.fn();

describe('LexiCore App Component', () => {
    beforeEach(() => {
        vi.resetAllMocks();
        (window as any).__LEXICORE_API_BASE__ = undefined;
    });

    it('renders the loading state initially', () => {
        (global.fetch as any).mockImplementation(() => new Promise(() => {}));

        render(<App />);
        expect(screen.getByText(/Loading LexiCore.../i)).toBeInTheDocument();
    });

    it('renders the Authentication Required page on 401 status', async () => {
        (global.fetch as any).mockImplementation(() =>
            Promise.resolve({
                ok: false,
                status: 401,
                json: () => Promise.resolve({}),
            })
        );

        render(<App />);

        await waitFor(() => {
            expect(screen.getByText('🔒 Authentication Required')).toBeInTheDocument();
        });

        expect(screen.queryByTestId('main-dashboard')).not.toBeInTheDocument();
    });

    it('renders the Access Denied page on 403 status', async () => {
        (global.fetch as any).mockImplementation(() =>
            Promise.resolve({
                ok: false,
                status: 403,
                json: () => Promise.resolve({}),
            })
        );

        render(<App />);

        await waitFor(() => {
            expect(screen.getByText('✋ Access Denied')).toBeInTheDocument();
        });
    });

    it('renders the main dashboard components when API returns 200 OK', async () => {
        (global.fetch as any).mockImplementation((url: string) => {
            if (url.includes('/cultures')) {
                return Promise.resolve({
                    ok: true,
                    status: 200,
                    json: () => Promise.resolve([{ code: 'en-US', name: 'English' }]),
                });
            }
            return Promise.resolve({
                ok: true,
                status: 200,
                json: () => Promise.resolve([{ key: 'test', culture: 'en-US', value: 'Hello' }]),
            });
        });

        render(<App />);

        await waitFor(() => {
            expect(screen.queryByText(/Loading LexiCore.../i)).not.toBeInTheDocument();
            expect(screen.getByTestId('main-dashboard')).toBeInTheDocument();
        });
    });

    it('uses the injected window variable for the API base path', async () => {
        (global.fetch as any).mockImplementation(() =>
            Promise.resolve({
                ok: true,
                status: 200,
                json: () => Promise.resolve([]),
            })
        );

        render(<App />);

        await waitFor(() => {
            expect(screen.getByTestId('main-dashboard')).toBeInTheDocument();
        });

        expect(global.fetch).toHaveBeenCalledWith('/api/lexi-core/cultures');
        expect(global.fetch).toHaveBeenCalledWith('/api/lexi-core');
    });
});