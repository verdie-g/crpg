import { type PatchNote } from '@/models/patch-note';
import { get } from '@/services/crpg-client';

export const getPatchNotes = () => get<PatchNote[]>('/patch-notes');
