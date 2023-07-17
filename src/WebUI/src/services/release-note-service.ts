import { type ReleaseNote } from '@/models/release-note';
import { get } from '@/services/crpg-client';

export const getReleaseNotes = () => get<ReleaseNote[]>('/patch-notes');
