import { setToken, getToken, challenge } from '@/services/auth-service';
import { NotificationType, notify } from '@/services/notifications-service';
import { sleep } from '@/utils/promise';
import Result from '@/models/result';

export const API_BASE_URL = process.env.VUE_APP_API_BASE_URL;

async function send(method: string, path: string, body?: any): Promise<any> {
  const response = await fetch(API_BASE_URL + path, {
    method,
    headers: {
      Authorization: `Bearer ${getToken()}`,
      'Content-Type': 'application/json',
    },
    body: body != null ? JSON.stringify(body) : undefined,
  });

  // if the token was about to expire, the server issues a new one in the Refresh-Authorization header
  const refreshedToken = response.headers.get('Refresh-Authorization');
  if (refreshedToken !== null) {
    setToken(refreshedToken);
  }

  if (response.status === 401) {
    notify('Session expired', NotificationType.Warning);
    sleep(1000).then(() => challenge());
    return {};
  }

  const result: Result<any> = response.status !== 204 ? await response.json() : {};

  if (response.status >= 500) {
    notify(result.errors![0].title!, NotificationType.Error);
    throw new Error('Server error');
  }

  if (response.status >= 400) {
    notify(result.errors![0].title!, NotificationType.Warning);
    throw new Error('Bad request');
  }

  return result.data;
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
