import { getToken, signIn } from '@/services/auth-service';
import { notify } from '@/services/notifications-service';
import { sleep } from '@/utils/promise';

export const API_BASE_URL = 'http://localhost:5000/api';

async function send(method: string, path: string, body?: any): Promise<any> {
  const res = await fetch(API_BASE_URL + path, {
    method,
    headers: { Authorization: `Bearer ${getToken()}` },
    body,
  });

  if (res.status === 401) {
    notify('Session expired');
    sleep(1000).then(() => signIn());
    return {};
  }

  return res.json();
}

export function get(path: string): Promise<any> {
  return send('GET', path);
}

export function post(path: string, body: any): Promise<any> {
  return send('POST', path, body);
}

export function put(path: string, body: any): Promise<any> {
  return send('PUT', path, body);
}

export function del(path: string): Promise<any> {
  return send('DELETE', path);
}
