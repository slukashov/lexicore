import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi, describe, test, expect, beforeEach } from 'vitest';
import '@testing-library/jest-dom/vitest';
import Header from "../src/components/Header";


describe('Header Component', () => {
    const mockToggleTheme = vi.fn();
    const mockHandleExport = vi.fn();
    const mockHandleImport = vi.fn();
    const mockHandleOpenNew = vi.fn();
    const mockClearError = vi.fn();

    const defaultProps = {
        theme: 'light' as const,
        toggleTheme: mockToggleTheme,
        handleExport: mockHandleExport,
        handleImport: mockHandleImport,
        handleOpenNew: mockHandleOpenNew,
        errorMessage: null,
        clearError: mockClearError,
    };

    beforeEach(() => {
        vi.clearAllMocks();
    });

    test('renders the application title', () => {
        render(<Header {...defaultProps} />);

        expect(screen.getByText('LexiCore')).toBeInTheDocument();
        expect(screen.getByText('Localization Management System')).toBeInTheDocument();
    });

    test('triggers handleOpenNew when "+ New Translation" is clicked', async () => {
        const user = userEvent.setup();
        render(<Header {...defaultProps} />);

        const newButton = screen.getByText('+ New Translation');
        await user.click(newButton);

        expect(mockHandleOpenNew).toHaveBeenCalledTimes(1);
    });

    test('displays correct icon and triggers toggleTheme on click', async () => {
        const user = userEvent.setup();
        const { rerender } = render(<Header {...defaultProps} theme="light" />);

        const themeButton = screen.getByText('🌙');
        await user.click(themeButton);

        expect(mockToggleTheme).toHaveBeenCalledTimes(1);

        rerender(<Header {...defaultProps} theme="dark" />);
        expect(screen.getByText('☀️')).toBeInTheDocument();
    });

    test('displays error message and handles clear action', async () => {
        const user = userEvent.setup();
        render(<Header {...defaultProps} errorMessage="Database connection failed" />);

        expect(screen.getByText('Database connection failed')).toBeInTheDocument();

        const closeButton = screen.getByText('✕');
        await user.click(closeButton);

        expect(mockClearError).toHaveBeenCalledTimes(1);
    });
});