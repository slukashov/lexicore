import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi, describe, test, expect, beforeEach } from 'vitest';
import '@testing-library/jest-dom/vitest';
import TranslationTable from "../src/components/TranslationTable";
import {SupportedLanguage} from "../src/types/SupportedLanguage";
import {GroupedKey} from "../src/types/GroupedKey";

describe('TranslationTable Component', () => {
    const mockSetFilter = vi.fn();
    const mockSetSearchQuery = vi.fn();
    const mockHandleRowClick = vi.fn();
    const mockHandleToggleDeprecated = vi.fn((_key: string, _currentStatus: boolean, e: any) => {
        e.stopPropagation();
    });

    const mockIsTranslated = vi.fn((val: string | undefined | null) => {
        return val !== undefined && val !== null && val.trim() !== '';
    });

    const mockSupportedLanguages: SupportedLanguage[] = [
        { code: 'en-US', name: 'English' },
        { code: 'es-ES', name: 'Spanish' }
    ];

    const mockGroupedKeys: GroupedKey[] = [
        {
            key: 'welcome_message',
            isDeprecated: false,
            values: { 'en-US': 'Welcome', 'es-ES': '' }, // Missing Spanish
            translatedCount: 1,
            isCompleted: false
        },
        {
            key: 'logout_btn',
            isDeprecated: true, 
            values: { 'en-US': 'Logout', 'es-ES': 'Salir' },
            translatedCount: 2,
            isCompleted: true
        }
    ];

    const defaultProps = {
        groupedKeys: mockGroupedKeys,
        supportedLanguages: mockSupportedLanguages,
        filter: 'ALL',
        setFilter: mockSetFilter,
        searchQuery: '',
        setSearchQuery: mockSetSearchQuery,
        handleRowClick: mockHandleRowClick,
        handleToggleDeprecated: mockHandleToggleDeprecated,
        isTranslated: mockIsTranslated,
    };

    beforeEach(() => vi.clearAllMocks());

    test('renders the empty state when no keys are provided', () => {
        render(<TranslationTable {...defaultProps} groupedKeys={[]} />);
        expect(screen.getByText(/No translation keys found/i)).toBeInTheDocument();
    });

    test('renders translation rows correctly', () => {
        render(<TranslationTable {...defaultProps} />);

        expect(screen.getByText('welcome_message')).toBeInTheDocument();
        expect(screen.getByText('logout_btn')).toBeInTheDocument();

        expect(screen.getByText('PENDING 1 LANGS')).toBeInTheDocument();
        expect(screen.getByText('ARCHIVED')).toBeInTheDocument();
    });

    test('calls setFilter when a filter tab is clicked', async () => {
        const user = userEvent.setup();
        render(<TranslationTable {...defaultProps} />);

        const missingTab = screen.getByRole('button', { name: 'MISSING' });
        await user.click(missingTab);

        expect(mockSetFilter).toHaveBeenCalledWith('MISSING');
    });

    test('calls setSearchQuery when typing in the search box', async () => {
        const user = userEvent.setup();
        render(<TranslationTable {...defaultProps} />);

        const searchInput = screen.getByPlaceholderText('Search translation keys...');
        await user.type(searchInput, 'login');

        expect(mockSetSearchQuery).toHaveBeenCalled();
    });

    test('calls handleRowClick when a table row is clicked', async () => {
        const user = userEvent.setup();
        render(<TranslationTable {...defaultProps} />);

        const rowKey = screen.getByText('welcome_message');
        await user.click(rowKey);

        expect(mockHandleRowClick).toHaveBeenCalledWith('welcome_message');
    });

    test('calls handleToggleDeprecated when the action button is clicked', async () => {
        const user = userEvent.setup();
        render(<TranslationTable {...defaultProps} />);

        const deprecateBtn = screen.getByRole('button', { name: 'Deprecate' });
        await user.click(deprecateBtn);

        expect(mockHandleToggleDeprecated).toHaveBeenCalledWith(
            'welcome_message',
            false,
            expect.anything()
        );
        expect(mockHandleRowClick).not.toHaveBeenCalled();
    });
});