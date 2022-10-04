import { defineStore } from 'pinia';
import { getUser } from '~/services/user';
import { type IUserState } from '~/models/user';

export const useUserStore = defineStore('user', {
  state: (): IUserState => ({
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
