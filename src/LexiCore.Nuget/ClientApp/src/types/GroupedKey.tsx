export interface GroupedKey {
    key: string;
    isDeprecated: boolean;
    values: Record<string, string>;
    translatedCount: number;
    isCompleted: boolean;
}