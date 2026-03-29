import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi, describe, test, expect, beforeEach, afterEach } from 'vitest';
import App from '../App';

vi.mock('@monaco-editor/react', () => {
    return {
        default: () => <textarea data-testid="monaco-mock" />
    };
});

global.URL.createObjectURL = vi.fn();

describe('App Integration Tests', () => {
    const mockCultures = [
        { code: 'en-US', name: 'English' },
        { code: 'fr-FR', name: 'French' }
    ];

    const mockTranslations = [
        { id: 1, key: 'welcome_message', culture: 'en-US', value: 'Hello', isDeprecated: false }
    ];

    beforeEach(() => {
        global.fetch = vi.fn((url) => {
            if (url === '/api/LexiCore/cultures') {
                return Promise.resolve({
                    ok: true,
                    json: () => Promise.resolve(mockCultures),
                });
            }
            if (url === '/api/LexiCore') {
                return Promise.resolve({
                    ok: true,
                    json: () => Promise.resolve(mockTranslations),
                });
            }
            return Promise.reject(new Error('Unknown API endpoint'));
        }) as any;

        document.documentElement.classList.remove('dark');
    });

    afterEach(() => {
        vi.clearAllMocks();
    });

    test('fetches data on mount and renders the table', async () => {
        render(<App />);

        expect(global.fetch).toHaveBeenCalledWith('/api/LexiCore/cultures');
        expect(global.fetch).toHaveBeenCalledWith('/api/LexiCore');

        await waitFor(() => expect(screen.getByText('welcome_message')).toBeInTheDocument());

        expect(screen.getByText('PENDING 1 LANGS')).toBeInTheDocument();
    });

    test('opens the EditorWorkspace when + New Translation is clicked', async () => {
        const user = userEvent.setup();
        render(<App />);

        await waitFor(() => expect(screen.getByText('welcome_message')).toBeInTheDocument());

        expect(screen.queryByText('Workspace Mode')).not.toBeInTheDocument();

        const newBtn = screen.getByText('+ New Translation');
        await user.click(newBtn);

        expect(screen.getByText('New Translation')).toBeInTheDocument();
        expect(screen.getByText('Workspace Mode')).toBeInTheDocument();
    });

    test('opens the EditorWorkspace pre-filled when a row is clicked', async () => {
        const user = userEvent.setup();
        render(<App />);

        await waitFor(() => expect(screen.getByText('welcome_message')).toBeInTheDocument());

        const row = screen.getByText('welcome_message');
        await user.click(row);

        expect(screen.getByText('Editing: welcome_message')).toBeInTheDocument();
    });

    test('toggles dark mode on the document element', async () => {
        const user = userEvent.setup();
        render(<App />);

        expect(document.documentElement.classList.contains('dark')).toBe(false);

        const toggleBtn = screen.getByText('🌙');
        await user.click(toggleBtn);

        expect(document.documentElement.classList.contains('dark')).toBe(true);
        expect(screen.getByText('☀️')).toBeInTheDocument();
    });

    test('displays an error if the fetch fails', async () => {
        global.fetch = vi.fn(() => Promise.reject(new Error('Network error'))) as any;

        render(<App />);

        await waitFor(() => expect(screen.getByText(/Failed to load/i)).toBeInTheDocument());
    });
});