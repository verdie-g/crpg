import type User from '@/models/user';

import { getUser } from '@/services/users-service';

interface State {
  user: User | null;
}

export const useUserStore = defineStore('user', {
  state: (): State => ({
    user: null,
  }),

  getters: {},

  actions: {
    async fetchUser() {
      this.user = await getUser();
    },
  },
});
