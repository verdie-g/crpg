import { getReleaseNotes } from '@/services/release-note-service';

export const useReleaseNotes = () => {
  const { state: releaseNotes, execute: loadReleaseNotes } = useAsyncState(
    () => getReleaseNotes(),
    [],
    {
      immediate: false,
    }
  );

  return {
    releaseNotes,
    loadReleaseNotes,
  };
};
