import { api } from '~/boot/api';
import { type IUserState } from '~/models/user';

export const getUser = () => api.get<IUserState>('user');
