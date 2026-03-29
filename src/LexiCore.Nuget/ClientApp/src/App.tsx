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

    const [filter, setFilter] = useState<string>('ALL');
    const [searchQuery, setSearchQuery] = useState<string>('');

    const [isEditorOpen, setIsEditorOpen] = useState<boolean>(false);
    const [newEntry, setNewEntry] = useState<LexiCoreEntry>({key: '', culture: '', value: '', isDeprecated: false});

    useEffect(() => {
        fetchCultures().then(_ => {
        });
        fetchTranslations().then(_ => {
        });
    }, []);

    const fetchCultures = async () => {
        try {
            const res = await fetch('/api/lexi-core/cultures');
            const data = await res.json();
            setSupportedLanguages(data);
            if (data.length > 0) 
                setNewEntry(prev => ({...prev, culture: data[0].code}));
        } catch {
            setErrorMessage("Failed to load cultures.");
        }
    };

    const fetchTranslations = async () => {
        try {
            const res = await fetch('/api/lexi-core');
            setTranslations(await res.json());
        } catch {
            setErrorMessage("Failed to load translations.");
        }
    };

    const handleSave = async () => {
        setErrorMessage(null);
        const res = await fetch('/api/lexi-core', {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify(newEntry)
        });
        if (res.ok) {
            await fetchTranslations();
            setIsEditorOpen(false);
        } else {
            const err = await res.json();
            setErrorMessage(err.error || "Save failed.");
            setIsEditorOpen(false);
        }
    };

    const handleToggleDeprecated = async (key: string, currentStatus: boolean, e: React.MouseEvent) => {
        e.stopPropagation();
        const entry = translations.find(t => t.key === key);
        if (!entry) 
            return;
        await fetch('/api/lexi-core', {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify({...entry, isDeprecated: !currentStatus})
        });
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
        setNewEntry({key: '', culture: supportedLanguages[0]?.code || 'en-US', value: '', isDeprecated: false});
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
            isDeprecated: false
        });
        setIsEditorOpen(true);
    };

    // --- Logic ---
    const isTranslated = (val: string | undefined | null) => val !== undefined && val !== null && val.trim() !== '';

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

    // --- Render ---
    return (
        <div
            className="min-h-screen bg-slate-50 dark:bg-slate-950 text-slate-900 dark:text-slate-100 p-6 font-sans transition-colors duration-200">
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