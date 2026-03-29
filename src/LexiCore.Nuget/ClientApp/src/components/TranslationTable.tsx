import React from 'react';
import type {SupportedLanguage} from "../types/SupportedLanguage.tsx";
import type {GroupedKey} from "../types/GroupedKey.tsx";

interface TranslationTableProps {
    groupedKeys: GroupedKey[];
    supportedLanguages: SupportedLanguage[];
    filter: string;
    setFilter: (f: string) => void;
    searchQuery: string;
    setSearchQuery: (q: string) => void;
    handleRowClick: (key: string) => void;
    handleToggleDeprecated: (key: string, currentStatus: boolean, e: React.MouseEvent) => void;
    isTranslated: (val: string | undefined | null) => boolean;
}

export default function TranslationTable({
                                             groupedKeys, supportedLanguages, filter, setFilter, searchQuery, setSearchQuery, handleRowClick, handleToggleDeprecated, isTranslated
                                         }: TranslationTableProps) {
    return (
        <div className="bg-white dark:bg-slate-900 rounded-2xl shadow-sm border border-slate-200 dark:border-slate-800 overflow-hidden">
            <div className="p-6 border-b border-slate-100 dark:border-slate-800 flex flex-col md:flex-row justify-between items-center gap-4 bg-white dark:bg-slate-900 sticky top-0 z-10">
                <div className="flex bg-slate-100 dark:bg-slate-950 p-1 rounded-xl">
                    {['ALL', 'MISSING', 'DEPRECATED'].map(f => (
                        <button
                            key={f}
                            onClick={() => setFilter(f)}
                            className={`px-5 py-2 rounded-lg text-[11px] font-black transition-all ${filter === f ? 'bg-white dark:bg-slate-800 text-blue-600 dark:text-blue-400 shadow-sm' : 'text-slate-500 hover:text-slate-800 dark:hover:text-slate-300'}`}
                        >
                            {f}
                        </button>
                    ))}
                </div>
                <div className="flex gap-3 w-full md:w-auto items-center">
                    <span className="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase hidden md:inline-block">Click any row to Edit 👆</span>
                    <input
                        type="text"
                        placeholder="Search translation keys..."
                        value={searchQuery}
                        onChange={e => setSearchQuery(e.target.value)}
                        className="px-4 py-2 bg-slate-50 dark:bg-slate-950 border border-slate-200 dark:border-slate-800 rounded-xl text-xs outline-none focus:ring-2 focus:ring-blue-100 dark:focus:ring-blue-900 w-full md:w-72 dark:text-white"
                    />
                </div>
            </div>

            <div className="overflow-x-auto min-h-125">
                <table className="w-full text-left">
                    <thead>
                    <tr className="text-[10px] font-black text-slate-400 dark:text-slate-500 uppercase tracking-widest border-b border-slate-100 dark:border-slate-800 bg-slate-50/30 dark:bg-slate-950/50">
                        <th className="px-8 py-5">Global Key</th>
                        <th className="px-8 py-5">Lifecycle</th>
                        <th className="px-8 py-5 text-right">Coverage</th>
                        <th className="px-8 py-5 text-right">Actions</th>
                    </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-50 dark:divide-slate-800/50">
                    {groupedKeys.map((k) => (
                        <tr
                            key={k.key}
                            onClick={() => handleRowClick(k.key)}
                            className={`hover:bg-blue-50/50 dark:hover:bg-slate-800/50 transition cursor-pointer group ${k.isDeprecated ? 'opacity-50 grayscale hover:bg-slate-50 dark:hover:bg-slate-900' : ''}`}
                        >
                            <td className="px-8 py-5">
                                <span className="font-mono text-sm font-bold text-slate-700 dark:text-slate-300 group-hover:text-blue-700 dark:group-hover:text-blue-400 transition-colors">{k.key}</span>
                            </td>
                            <td className="px-8 py-5">
                                {k.isDeprecated ? (
                                    <span className="px-2 py-1 bg-slate-100 dark:bg-slate-800 text-slate-500 dark:text-slate-400 rounded text-[9px] font-black">ARCHIVED</span>
                                ) : k.isCompleted ? (
                                    <span className="px-2 py-1 bg-slate-100 dark:bg-slate-800/70 text-slate-600 dark:text-slate-300 rounded text-[9px] font-black">FULLY TRANSLATED</span>
                                ) : (
                                    <span className="px-2 py-1 bg-orange-50 dark:bg-orange-900/30 text-orange-700 dark:text-orange-400 rounded text-[9px] font-black">PENDING {supportedLanguages.length - k.translatedCount} LANGS</span>
                                )}
                            </td>
                            <td className="px-8 py-5">
                                <div className="flex justify-end gap-1.5">
                                    {supportedLanguages.map(lang => {
                                        const isDone = isTranslated(k.values[lang.code]);
                                        return (
                                            <div
                                                key={lang.code}
                                                className={`w-7 h-7 rounded-lg flex items-center justify-center text-[9px] font-black border transition-all ${isDone ? 'bg-slate-600 dark:bg-slate-700 border-slate-700 dark:border-slate-600 text-white shadow-sm' : 'bg-white dark:bg-slate-950 border-slate-200 dark:border-slate-800 text-slate-300 dark:text-slate-600 group-hover:border-blue-200 dark:group-hover:border-blue-800'}`}
                                                title={isDone ? 'Translated' : 'Missing Translation'}
                                            >
                                                {lang.code.substring(0,2).toUpperCase()}
                                            </div>
                                        );
                                    })}
                                </div>
                            </td>
                            <td className="px-8 py-5 text-right">
                                <button
                                    onClick={(e) => handleToggleDeprecated(k.key, k.isDeprecated, e)}
                                    className="text-[10px] font-bold text-slate-400 dark:text-slate-500 hover:text-slate-900 dark:hover:text-slate-200 border border-transparent hover:border-slate-200 dark:hover:border-slate-700 px-3 py-1.5 rounded-lg transition hover:bg-white dark:hover:bg-slate-800"
                                >
                                    {k.isDeprecated ? 'Restore' : 'Deprecate'}
                                </button>
                            </td>
                        </tr>
                    ))}
                    {groupedKeys.length === 0 && (
                        <tr>
                            <td colSpan={4} className="px-8 py-20 text-center text-slate-400 dark:text-slate-600 italic text-sm font-medium bg-slate-50/50 dark:bg-slate-900/50">
                                No translation keys found. Start by clicking "+ New Translation" at the top!
                            </td>
                        </tr>
                    )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}