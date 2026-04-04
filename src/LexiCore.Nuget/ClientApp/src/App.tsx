import React, {useState, useEffect} from 'react';
import Header from './components/Header';
import TranslationTable from './components/TranslationTable';
import EditorWorkspace from './components/EditorWorkspace';
import type {LexiCoreEntry} from "./types/LexiCoreEntry.tsx";
import type {SupportedLanguage} from "./types/SupportedLanguage.tsx";
import type {GroupedKey} from "./types/GroupedKey.tsx";

export default function App() {
    const [theme, setTheme] = useState<'light' | 'dark'>('light');
    const [translations, setTranslations] = useState<LexiCoreEntry[]>([]);
    const [supportedLanguages, setSupportedLanguages] = useState<SupportedLanguage[]>([]);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [authStatus, setAuthStatus] = useState<200 | 401 | 403 | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [filter, setFilter] = useState<string>('ALL');
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [isEditorOpen, setIsEditorOpen] = useState<boolean>(false);
    const [newEntry, setNewEntry] = useState<LexiCoreEntry>({key: '', culture: '', value: '', variablesJson: '{}', isDeprecated: false});

    const API_BASE = '/api/lexi-core';

    useEffect(() => {
        const initApp = async () => {
            setIsLoading(true);
            await Promise.all([fetchCultures(), fetchTranslations()]);
            setIsLoading(false);
        };
        initApp().then(() => {});
    }, []);

    const fetchCultures = async () => {
        try {
            const res = await fetch(`${API_BASE}/cultures`);
            if (res.status === 401) { setAuthStatus(401); return; }
            if (res.status === 403) { setAuthStatus(403); return; }

            if (res.ok) {
                const data = await res.json();
                setSupportedLanguages(data);
                if (data.length > 0)
                    setNewEntry(prev => ({...prev, culture: data[0].code}));
            }
        } catch {
            setErrorMessage("Failed to load cultures.");
        }
    };

    const fetchTranslations = async () => {
        try {
            const res = await fetch(`${API_BASE}`);
            if (res.status === 401) { setAuthStatus(401); return; }
            if (res.status === 403) { setAuthStatus(403); return; }

            if (res.ok) {
                setTranslations(await res.json());
                setAuthStatus(200); // Mark as fully authorized if successful
            }
        } catch {
            setErrorMessage("Failed to load translations.");
        }
    };

    const handleSave = async () => {
        setErrorMessage(null);
        const res = await fetch(`${API_BASE}`, {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify(newEntry)
        });

        if (res.status === 401 || res.status === 403) {
            setErrorMessage("Error: Your session expired or you lack permissions to save.");
            return;
        }

        if (res.ok) {
            await fetchTranslations();
            setIsEditorOpen(false);
        } else {
            const err = await res.json().catch(() => ({}));
            setErrorMessage(err.error || "Save failed.");
            setIsEditorOpen(false);
        }
    };

    const handleToggleDeprecated = async (key: string, currentStatus: boolean, e: React.MouseEvent) => {
        e.stopPropagation();
        const entry = translations.find(t => t.key === key);
        if (!entry) return;

        const res = await fetch(`${API_BASE}`, {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify({...entry, isDeprecated: !currentStatus})
        });

        if (res.status === 401 || res.status === 403) {
            setErrorMessage("Error: You do not have permission to modify this entry.");
            return;
        }

        await fetchTranslations();
    };

    const toggleTheme = () => {
        if (theme === 'light') {
            document.documentElement.classList.add('dark');
            setTheme('dark');
        } else {
            document.documentElement.classList.remove('dark');
            setTheme('light');
        }
    };

    const handleOpenNew = () => {
        setNewEntry({key: '', culture: supportedLanguages[0]?.code || 'en-US', value: '', variablesJson: '{}', isDeprecated: false});
        setIsEditorOpen(true);
    };

    const handleRowClick = (keyName: string) => {
        const existing = translations.find(t => t.key === keyName && t.culture === 'en-US') || translations.find(t => t.key === keyName);
        if (existing)
            setNewEntry(existing);
        else setNewEntry({
            key: keyName,
            culture: supportedLanguages[0]?.code || 'en-US',
            value: '',
            variablesJson: '{}',
            isDeprecated: false
        });
        setIsEditorOpen(true);
    };

    const isTranslated = (val: string | undefined | null) => val !== undefined && val !== null && val.trim() !== '';


    if (isLoading) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 transition-colors duration-200">
                <div className="text-lg font-medium animate-pulse">Loading LexiCore...</div>
            </div>
        );
    }

    if (authStatus === 401) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 p-6 font-sans transition-colors duration-200">
                <div className="max-w-md text-center bg-white dark:bg-slate-900 p-8 rounded-xl shadow-sm border border-slate-200 dark:border-slate-800">
                    <h2 className="text-2xl font-bold text-amber-600 dark:text-amber-500 mb-4">🔒 Authentication Required</h2>
                    <p className="mb-2">You must be logged in to view and manage LexiCore translations.</p>
                    <p className="text-sm text-slate-500 dark:text-slate-400">Please log in to your host application and try again.</p>
                </div>
            </div>
        );
    }

    if (authStatus === 403) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 p-6 font-sans transition-colors duration-200">
                <div className="max-w-md text-center bg-white dark:bg-slate-900 p-8 rounded-xl shadow-sm border border-slate-200 dark:border-slate-800">
                    <h2 className="text-2xl font-bold text-red-600 dark:text-red-500 mb-4">✋ Access Denied</h2>
                    <p className="mb-2">You do not have the required permissions to access the localization engine.</p>
                    <p className="text-sm text-slate-500 dark:text-slate-400">If you believe this is an error, please contact your administrator.</p>
                </div>
            </div>
        );
    }

    const keysMap: Record<string, any> = {};
    translations.forEach(t => {
        if (!keysMap[t.key]) keysMap[t.key] = {key: t.key, isDeprecated: t.isDeprecated, values: {}};
        keysMap[t.key].values[t.culture] = t.value;
    });

    const groupedKeys: GroupedKey[] = Object.values(keysMap)
        .map(k => {
            const actualTranslatedCount = supportedLanguages.filter(lang => isTranslated(k.values[lang.code])).length;
            return {
                ...k,
                translatedCount: actualTranslatedCount,
                isCompleted: actualTranslatedCount === supportedLanguages.length && supportedLanguages.length > 0
            };
        })
        .filter(k => {
            if (!k.key.toLowerCase().includes(searchQuery.toLowerCase())) return false;
            if (filter === 'DEPRECATED') return k.isDeprecated;
            if (filter === 'MISSING') return !k.isDeprecated && !k.isCompleted;
            return true;
        });

    return (
        <div data-testid="main-dashboard" className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 p-6 font-sans transition-colors duration-200">
            <div className="max-w-7xl mx-auto">
                <Header
                    theme={theme} toggleTheme={toggleTheme} handleOpenNew={handleOpenNew} errorMessage={errorMessage}
                    clearError={() => setErrorMessage(null)}
                />
                <TranslationTable
                    groupedKeys={groupedKeys} supportedLanguages={supportedLanguages} filter={filter}
                    setFilter={setFilter} searchQuery={searchQuery} setSearchQuery={setSearchQuery}
                    handleRowClick={handleRowClick} handleToggleDeprecated={handleToggleDeprecated}
                    isTranslated={isTranslated}
                />
            </div>

            {isEditorOpen && (
                <EditorWorkspace
                    theme={theme} newEntry={newEntry} setNewEntry={setNewEntry} supportedLanguages={supportedLanguages}
                    translations={translations} setIsEditorOpen={setIsEditorOpen} handleSave={handleSave}
                />
            )}
        </div>
    );
}