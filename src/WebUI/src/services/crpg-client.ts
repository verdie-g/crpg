import { getToken, signIn } from '@/services/auth-service';
import { NotificationType, notify } from '@/services/notifications-service';
import { sleep } from '@/utils/promise';
import { ErrorType, Result } from '@/models/result';

export const API_BASE_URL = process.env.VUE_APP_API_BASE_URL;

async function trySend(method: string, path: string, body?: any): Promise<Result<any>> {
  const token = await getToken();
  const response = await fetch(API_BASE_URL + path, {
    method,
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: body != null ? JSON.stringify(body) : undefined,
  });

  if (response.status === 401) {
    notify('Session expired', NotificationType.Warning);
    sleep(1000).then(() => signIn());
    return null!;
  }

  return response.status !== 204 ? await response.json() : null;
}

async function send(method: string, path: string, body?: any): Promise<any> {
  const result = await trySend(method, path, body);
  if (result === null) {
    return null;
  }

  if (result.errors === null) {
    return result.data;
  }

  if (result.errors[0].type === ErrorType.InternalError) {
    notify(result.errors![0].title!, NotificationType.Error);
    throw new Error('Server error');
  } else {
    notify(result.errors![0].title!, NotificationType.Warning);
    throw new Error('Bad request');
  }
}

export function tryGet(path: string): Promise<Result<any>> {
  return trySend('GET', path);
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
