import User from '@/models/user';
import {
  get, post, put, del,
} from './trpg-client';

export function getUser(): Promise<User> {
  return get('/users/self');
}
