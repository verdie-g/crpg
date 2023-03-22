import { type NavigationGuard, type RouteLocationNormalized } from 'vue-router/auto';
import { useUserStore } from '@/stores/user';

export const clanIdParamValidate: NavigationGuard = to => {
  if (Number.isNaN(Number(to.params.id as string))) {
    return { name: 'PageNotFound' } as RouteLocationNormalized<'PageNotFound'>;
  }

  return true;
};

export const clanExistValidate: NavigationGuard = async () => {
  const userStore = useUserStore();

  if (userStore.clan === null) {
    await userStore.getUserClan();
  }

  if (userStore.clan !== null) {
    return {
      name: 'ClansId',
      params: { id: String(userStore.clan.id) },
    } as RouteLocationNormalized<'ClansId'>;
  }

  return true;
};
