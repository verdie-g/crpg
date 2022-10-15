import { get } from '@/services/crpg-client';
import { type User } from '@/models/example';

export const getUser = () => get<User>('/user');
