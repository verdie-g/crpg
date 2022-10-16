import { get } from '@/services/crpg-client';

import type User from '@/models/user';

export const getUser = () => get<User>('/users/self');
