import { getPatchNotes } from '@/services/patch-note-service';

export const usePatchNotes = () => {
  const { state: patchNotes, execute: loadPatchNotes } = useAsyncState(() => getPatchNotes(), [], {
    immediate: false,
  });

  return {
    patchNotes,
    loadPatchNotes,
  };
};
