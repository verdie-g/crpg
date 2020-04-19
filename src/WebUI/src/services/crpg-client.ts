import { getToken, challenge } from '@/services/auth-service';
import { NotificationType, notify } from '@/services/notifications-service';
import { sleep } from '@/utils/promise';

export const API_BASE_URL = 'http://localhost:8000/api';

async function send(method: string, path: string, body?: any): Promise<any> {
  const res = await fetch(API_BASE_URL + path, {
    method,
    headers: {
      Authorization: `Bearer ${getToken()}`,
      'Content-Type': 'application/json',
    },
    body: body != null ? JSON.stringify(body) : undefined,
  });

  if (res.status === 401) {
    notify('Session expired', NotificationType.Warning);
    sleep(1000).then(() => challenge());
    return {};
  }

  if (res.status >= 500) {
    notify('Server error', NotificationType.Error);
    throw new Error('Server error');
  }

  if (res.status >= 400) {
    notify('Bad Request', NotificationType.Warning);
    throw new Error(await res.json());
  }

  return res.status !== 204 ? res.json() : {};
}

export function get(path: string): Promise<any> {
  return send('GET', path);
}

export function post(path: string, body?: any): Promise<any> {
  return send('POST', path, body);
}

export function put(path: string, body?: any): Promise<any> {
  return send('PUT', path, body);
}

export function del(path: string): Promise<any> {
  return send('DELETE', path);
}
