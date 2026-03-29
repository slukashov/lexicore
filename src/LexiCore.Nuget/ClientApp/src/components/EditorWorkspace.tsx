import React, {useState, useEffect} from 'react';
import Editor from '@monaco-editor/react';
import {Liquid} from 'liquidjs';
import type {SupportedLanguage} from "../types/SupportedLanguage.tsx";
import type {LexiCoreEntry} from "../types/LexiCoreEntry.tsx";

const engine = new Liquid();

interface EditorWorkspaceProps {
    theme: 'light' | 'dark';
    newEntry: LexiCoreEntry;
    setNewEntry: React.Dispatch<React.SetStateAction<LexiCoreEntry>>;
    supportedLanguages: SupportedLanguage[];
    translations: LexiCoreEntry[];
    setIsEditorOpen: (isOpen: boolean) => void;
    handleSave: () => void;
}

export default function EditorWorkspace({
                                            theme,
                                            newEntry,
                                            setNewEntry,
                                            supportedLanguages,
                                            translations,
                                            setIsEditorOpen,
                                            handleSave
                                        }: EditorWorkspaceProps) {
    const [isPreviewMode, setIsPreviewMode] = useState<boolean>(false);
    const [showMockData, setShowMockData] = useState<boolean>(false);
    const [mockData, setMockData] = useState<string>('{\n  \n}');
    const [liveError, setLiveError] = useState<string | null>(null);
    const [previewHtml, setPreviewHtml] = useState<string>('');

    useEffect(() => {
        if (!isPreviewMode) return;
        const timeoutId = setTimeout(async () => {
            try {
                const parsed = JSON.parse(mockData || '{}');
                const rendered = await engine.parseAndRender(newEntry.value, parsed);
                setPreviewHtml(rendered);
                setLiveError(null);
            } catch (err: any) {
                setLiveError("Error: " + err.message);
            }
        }, 300);
        return () => clearTimeout(timeoutId);
    }, [newEntry.value, mockData, isPreviewMode]);

    return (
        <div className="fixed inset-0 z-50 flex flex-col bg-slate-50 dark:bg-slate-950 animate-in fade-in duration-200">
            <div
                className="h-16 flex justify-between items-center px-6 bg-white dark:bg-slate-900 border-b border-slate-200 dark:border-slate-800 shadow-sm z-10 shrink-0">
                <div className="flex items-center gap-4">
                    <div
                        className="w-10 h-10 bg-blue-50 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 rounded-lg flex items-center justify-center text-lg font-black">
                        L.
                    </div>
                    <div>
                        <h2 className="text-sm font-black text-slate-800 dark:text-slate-100">
                            {newEntry.key ? `Editing: ${newEntry.key}` : 'New Translation'}
                        </h2>
                        <p className="text-[10px] font-bold text-slate-400 dark:text-slate-500 uppercase tracking-widest mt-0.5">Workspace
                            Mode</p>
                    </div>
                </div>

                <div className="flex items-center gap-3">
                    <button
                        onClick={() => setShowMockData(!showMockData)}
                        className={`px-4 py-2 rounded-lg text-xs font-bold uppercase tracking-wider transition-all flex items-center gap-2 ${showMockData ? 'bg-amber-100 dark:bg-amber-900/40 text-amber-700 dark:text-amber-400 shadow-inner' : 'bg-slate-100 dark:bg-slate-800 text-slate-500 dark:text-slate-400 hover:bg-slate-200 dark:hover:bg-slate-700'}`}
                    >
                        Variables: {showMockData ? 'ON' : 'OFF'}
                    </button>
                    <button
                        onClick={() => setIsPreviewMode(!isPreviewMode)}
                        className={`px-4 py-2 rounded-lg text-xs font-bold uppercase tracking-wider transition-all flex items-center gap-2 ${isPreviewMode ? 'bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-400 shadow-inner' : 'bg-slate-100 dark:bg-slate-800 text-slate-500 dark:text-slate-400 hover:bg-slate-200 dark:hover:bg-slate-700'}`}
                    >
                        <div
                            className={`w-2 h-2 rounded-full ${isPreviewMode ? 'bg-blue-600 dark:bg-blue-400 animate-pulse' : 'bg-slate-400 dark:bg-slate-600'}`}></div>
                        Preview: {isPreviewMode ? 'ON' : 'OFF'}
                    </button>

                    <div className="w-px h-6 bg-slate-200 dark:bg-slate-800 mx-1"></div>

                    <button
                        onClick={() => setIsEditorOpen(false)}
                        className="px-6 py-2 bg-white dark:bg-slate-900 border border-slate-300 dark:border-slate-700 text-slate-700 dark:text-slate-300 rounded-lg text-xs font-bold uppercase tracking-widest hover:bg-slate-50 dark:hover:bg-slate-800 transition shadow-sm"
                    >
                        Discard
                    </button>
                    <button
                        onClick={() => handleSave()}
                        className="px-6 py-2 bg-slate-800 dark:bg-blue-600 text-white rounded-lg text-xs font-bold uppercase tracking-widest hover:bg-slate-900 dark:hover:bg-blue-700 transition shadow-md active:scale-95"
                    >
                        Save Changes
                    </button>
                </div>
            </div>

            <div className="grow flex overflow-hidden">

                <div
                    className={`flex flex-col p-6 overflow-y-auto transition-all duration-300 ${isPreviewMode ? 'w-1/2 border-r border-slate-200 dark:border-slate-800 bg-slate-50/50 dark:bg-slate-900/50' : 'w-full max-w-5xl mx-auto'}`}>
                    <div className="grid grid-cols-2 gap-6 mb-6 shrink-0">
                        <div className="space-y-1.5">
                            <label
                                className="text-[10px] font-bold text-slate-500 dark:text-slate-400 uppercase tracking-widest">Key
                                Name</label>
                            <input
                                value={newEntry.key}
                                onChange={e => {
                                    const newKey = e.target.value;
                                    setNewEntry({...newEntry, key: newKey});
                                    const existing = translations.find(t => t.key === newKey && t.culture === newEntry.culture);
                                    if (existing) setNewEntry(existing);
                                }}
                                placeholder="e.g. auth_error_invalid"
                                className="w-full px-4 py-2 bg-white dark:bg-slate-950 border border-slate-200 dark:border-slate-800 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none transition font-mono text-sm shadow-sm dark:text-slate-100"
                            />
                        </div>
                        <div className="space-y-1.5">
                            <label
                                className="text-[10px] font-bold text-slate-500 dark:text-slate-400 uppercase tracking-widest">Culture
                                Target</label>
                            <select
                                value={newEntry.culture}
                                onChange={e => {
                                    const newCult = e.target.value;
                                    setNewEntry({...newEntry, culture: newCult});
                                    const existing = translations.find(t => t.key === newEntry.key && t.culture === newCult);
                                    if (existing) setNewEntry(existing);
                                    else setNewEntry(prev => ({...prev, culture: newCult, value: ''}));
                                }}
                                className="w-full px-4 py-2 bg-white dark:bg-slate-950 border border-slate-200 dark:border-slate-800 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 font-semibold text-slate-700 dark:text-slate-200 shadow-sm"
                            >
                                {supportedLanguages.map(l => <option key={l.code}
                                                                     value={l.code}>{l.name} ({l.code})</option>)}
                            </select>
                        </div>
                    </div>

                    <div className="flex flex-col grow min-h-75 mb-6">
                        <label
                            className="text-[10px] font-bold text-slate-500 dark:text-slate-400 uppercase tracking-widest mb-1.5 flex justify-between">
                            <span>DotLiquid HTML Template</span>
                            <span className="text-slate-400 dark:text-slate-500 font-mono">language: html</span>
                        </label>
                        <div
                            className="grow border border-slate-300 dark:border-slate-700 rounded-xl overflow-hidden shadow-inner focus-within:ring-2 focus-within:ring-blue-500 bg-white dark:bg-[#1e1e1e]">
                            <Editor
                                height="100%"
                                language="html"
                                theme={theme === 'dark' ? 'vs-dark' : 'light'}
                                value={newEntry.value}
                                onChange={(value) => setNewEntry({...newEntry, value: value || ''})}
                                options={{
                                    minimap: {enabled: false},
                                    wordWrap: 'on',
                                    fontSize: 14,
                                    padding: {top: 16, bottom: 16},
                                    scrollBeyondLastLine: false,
                                    automaticLayout: true
                                }}
                            />
                        </div>
                    </div>

                    {showMockData && (
                        <div className="flex flex-col h-48 shrink-0 animate-in slide-in-from-bottom-2 fade-in">
                            <label
                                className="text-[10px] font-bold text-slate-500 dark:text-slate-400 uppercase tracking-widest mb-1.5 flex justify-between">
                                <span>Test Variables (JSON)</span>
                                <span className="text-slate-400 dark:text-slate-500 font-mono">language: json</span>
                            </label>
                            <div
                                className="grow border border-slate-300 dark:border-slate-700 rounded-xl overflow-hidden shadow-inner focus-within:ring-2 focus-within:ring-blue-500 bg-white dark:bg-[#1e1e1e]">
                                <Editor
                                    height="100%"
                                    language="json"
                                    theme={theme === 'dark' ? 'vs-dark' : 'light'}
                                    value={mockData}
                                    onChange={(value) => setMockData(value || '')}
                                    options={{
                                        minimap: {enabled: false},
                                        fontSize: 13,
                                        padding: {top: 12, bottom: 12},
                                        scrollBeyondLastLine: false,
                                        automaticLayout: true
                                    }}
                                />
                            </div>
                        </div>
                    )}
                </div>

                {isPreviewMode && (
                    <div
                        className="w-1/2 bg-white dark:bg-slate-950 flex flex-col shadow-inner animate-in slide-in-from-right-8 duration-300">
                        <div
                            className="h-10 border-b border-slate-100 dark:border-slate-800 bg-slate-50 dark:bg-slate-900 flex items-center px-6 shrink-0">
                            <span
                                className="text-[10px] font-black uppercase tracking-widest text-slate-400 dark:text-slate-500">Live Render Output</span>
                        </div>
                        <div className="grow p-8 overflow-y-auto">
                            {liveError ? (
                                <div
                                    className="bg-red-50 dark:bg-red-950/30 border border-red-100 dark:border-red-900 rounded-xl p-6">
                                    <h3 className="text-red-800 dark:text-red-400 font-bold mb-2 flex items-center gap-2">
                                        <span>⚠️</span> Rendering Error</h3>
                                    <p className="text-red-600 dark:text-red-300 text-sm font-mono whitespace-pre-wrap">{liveError}</p>
                                </div>
                            ) : (
                                <div className="prose prose-slate dark:prose-invert max-w-none w-full"
                                     dangerouslySetInnerHTML={{__html: previewHtml}}/>
                            )}
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}