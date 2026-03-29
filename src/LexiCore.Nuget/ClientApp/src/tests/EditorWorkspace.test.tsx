import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi, describe, test, expect, beforeEach } from 'vitest';
import type {SupportedLanguage} from "../types/SupportedLanguage.tsx";
import type {LexiCoreEntry} from "../types/LexiCoreEntry.tsx";
import EditorWorkspace from "../components/EditorWorkspace.tsx";

vi.mock('@monaco-editor/react', () => {
    return {
        default: ({ value, onChange, language }: any) => (
            <textarea
                data-testid={`monaco-mock-${language}`}
                value={value}
                onChange={(e) => onChange(e.target.value)}
            />
        ),
    };
});

describe('EditorWorkspace Component', () => {
    const mockSetNewEntry = vi.fn();
    const mockSetIsEditorOpen = vi.fn();
    const mockHandleSave = vi.fn();

    const mockSupportedLanguages: SupportedLanguage[] = [
        { code: 'en-US', name: 'English' },
        { code: 'es-ES', name: 'Spanish' }
    ];

    const defaultEntry: LexiCoreEntry = {
        key: 'test_key',
        culture: 'en-US',
        value: '<div>Hello {{ user }}</div>',
        isDeprecated: false
    };

    const defaultProps = {
        theme: 'light' as const,
        newEntry: defaultEntry,
        setNewEntry: mockSetNewEntry,
        supportedLanguages: mockSupportedLanguages,
        translations: [],
        setIsEditorOpen: mockSetIsEditorOpen,
        handleSave: mockHandleSave,
    };

    beforeEach(() => {
        vi.clearAllMocks();
    });

    test('renders the workspace with the correct key name', () => {
        render(<EditorWorkspace {...defaultProps} />);
        expect(screen.getByText('Editing: test_key')).toBeInTheDocument();
    });

    test('calls setIsEditorOpen(false) when Discard is clicked', async () => {
        const user = userEvent.setup();
        render(<EditorWorkspace {...defaultProps} />);

        await user.click(screen.getByText('Discard'));
        expect(mockSetIsEditorOpen).toHaveBeenCalledWith(false);
    });

    test('calls handleSave when Save Changes is clicked', async () => {
        const user = userEvent.setup();
        render(<EditorWorkspace {...defaultProps} />);

        await user.click(screen.getByText('Save Changes'));
        expect(mockHandleSave).toHaveBeenCalledTimes(1);
    });

    test('updates key name when input changes', async () => {
        const user = userEvent.setup();
        render(<EditorWorkspace {...defaultProps} />);

        const keyInput = screen.getByPlaceholderText('e.g. auth_error_invalid');

        await user.type(keyInput, 'A');

        expect(mockSetNewEntry).toHaveBeenCalled();
    });

    test('toggles the JSON mock data editor when Variables button is clicked', async () => {
        const user = userEvent.setup();
        render(<EditorWorkspace {...defaultProps} />);

        expect(screen.queryByTestId('monaco-mock-json')).not.toBeInTheDocument();

        await user.click(screen.getByText('Variables: OFF'));

        expect(screen.getByText('Variables: ON')).toBeInTheDocument();
        expect(screen.getByTestId('monaco-mock-json')).toBeInTheDocument();
    });

    test('toggles the Live Preview panel when Preview button is clicked', async () => {
        const user = userEvent.setup();
        render(<EditorWorkspace {...defaultProps} />);

        expect(screen.queryByText('Live Render Output')).not.toBeInTheDocument();

        const previewBtn = screen.getByRole('button', { name: /Preview: OFF/i });
        await user.click(previewBtn);

        expect(screen.getByText('Live Render Output')).toBeInTheDocument();
    });
});