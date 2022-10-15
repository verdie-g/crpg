import { getUser } from '@/services/example';
import type { User } from '@/models/example';

export const useUserStore = defineStore('user', {
  state: (): User => ({
    name: '',
    role: '',
  }),

  getters: {
    namePlusRole: state => `${state.name} ${state.role}`,
  },

  actions: {
    async fetch() {
      this.$patch(await getUser());
    },
  },
});
