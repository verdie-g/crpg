import { type NavigationGuard, type RouteLocationNormalized } from 'vue-router/auto';
import { useUserStore } from '@/stores/user';

export const characterValidate: NavigationGuard = async to => {
  const userStore = useUserStore();

  if (!userStore.charactersOnceFetched) {
    await userStore.fetchCharacters();
  }

  if (!userStore.validateCharacter(Number(to.params!.id as string))) {
    // redirect to active char
    if (userStore.activeCharacterId !== null) {
      return {
        name: 'CharactersIdInventory',
        params: { id: String(userStore.activeCharacterId) },
      } as RouteLocationNormalized<'CharactersIdInventory'>;
    }

    return { name: 'Characters' };
  }

  return true;
};

export const activeCharacterRedirect: NavigationGuard = async _to => {
  const userStore = useUserStore();

  if (!userStore.charactersOnceFetched) {
    await userStore.fetchCharacters();
  }

  if (userStore.activeCharacterId !== null) {
    return {
      name: 'CharactersIdInventory',
      params: { id: String(userStore.activeCharacterId) },
    } as RouteLocationNormalized<'CharactersIdInventory'>;
  }

  return true;
};
