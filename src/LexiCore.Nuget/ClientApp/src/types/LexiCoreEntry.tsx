export interface LexiCoreEntry {
    id?: number;
    key: string;
    culture: string;
    value: string;
    variablesJson?: string;
    isDeprecated: boolean;
}