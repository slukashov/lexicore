interface HeaderProps {
    theme: 'light' | 'dark';
    toggleTheme: () => void;
    handleOpenNew: () => void;
    errorMessage: string | null;
    clearError: () => void;
}

export default function Header({
                                   theme, toggleTheme, handleOpenNew, errorMessage, clearError
                               }: HeaderProps) {
    return (
        <>
            <header className="flex flex-col md:flex-row justify-between items-start md:items-end mb-8 gap-4">
                <div>
                    <h1 className="text-4xl font-black tracking-tight text-slate-800 dark:text-slate-50">LexiCore<span className="text-blue-600 dark:text-blue-400">.</span></h1>
                    <p className="text-slate-500 dark:text-slate-400 font-medium tracking-tight">Localization Management System</p>
                </div>
                <div className="flex gap-3 items-center flex-wrap">
                    <button onClick={toggleTheme} className="p-2.5 bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 rounded-lg text-slate-600 dark:text-slate-400 shadow-sm hover:bg-slate-50 dark:hover:bg-slate-800 transition">
                        {theme === 'light' ? '🌙' : '☀️'}
                    </button>
                    <div className="w-px h-8 bg-slate-200 dark:bg-slate-800 mx-1"></div>
                    <button onClick={handleOpenNew} className="px-6 py-2.5 bg-slate-800 dark:bg-blue-600 text-white rounded-lg text-xs font-bold uppercase tracking-wider shadow-md hover:bg-slate-900 dark:hover:bg-blue-700 transition">
                        + New Translation
                    </button>
                </div>
            </header>

            {errorMessage && (
                <div className="mb-6 p-4 bg-red-50 dark:bg-red-950/30 border border-red-100 dark:border-red-900 rounded-xl flex justify-between items-center animate-pulse">
                    <div className="flex items-center gap-3 italic text-red-800 dark:text-red-400 font-medium">
                        <span>⚠️</span> {errorMessage}
                    </div>
                    <button onClick={clearError} className="text-red-400 hover:text-red-600 dark:hover:text-red-300 font-bold">✕</button>
                </div>
            )}
        </>
    );
}