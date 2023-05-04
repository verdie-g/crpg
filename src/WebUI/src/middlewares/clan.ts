import { type NavigationGuard, type RouteLocationNormalized } from 'vue-router/auto';
import { useUserStore } from '@/stores/user';

import { canUpdateClanValidate, canManageApplicationsValidate } from '@/services/clan-service';

export const clanIdParamValidate: NavigationGuard = to => {
  if (Number.isNaN(Number(to.params.id as string))) {
    return { name: 'PageNotFound' } as RouteLocationNormalized<'PageNotFound'>;
  }

  return true;
};

export const canUpdateClan: NavigationGuard = async to => {
  const userStore = useUserStore();

  if (userStore.clan === null) {
    await userStore.getUserClanAndRole();
  }

  if (
    userStore.clan?.id !== Number(to.params.id as string) ||
    (userStore.clanMemberRole !== null && !canUpdateClanValidate(userStore.clanMemberRole))
  ) {
    return { name: 'Clans' } as RouteLocationNormalized<'Clans'>;
  }

  return true;
};

export const canManageApplications: NavigationGuard = async to => {
  const userStore = useUserStore();

  if (userStore.clan === null) {
    await userStore.getUserClanAndRole();
  }

  if (
    userStore.clan?.id !== Number(to.params.id as string) ||
    (userStore.clanMemberRole !== null && !canManageApplicationsValidate(userStore.clanMemberRole))
  ) {
    return { name: 'Clans' } as RouteLocationNormalized<'Clans'>;
  }

  return true;
};

export const clanExistValidate: NavigationGuard = async () => {
  const userStore = useUserStore();

  if (userStore.clan === null) {
    await userStore.getUserClanAndRole();
  }

  if (userStore.clan !== null) {
    return {
      name: 'ClansId',
      params: { id: String(userStore.clan.id) },
    } as RouteLocationNormalized<'ClansId'>;
  }

  return true;
};
