import { StatusCodes } from 'http-status-codes';
import { ErrorType, type Result } from '@/models/crpg-client-result';
import { getToken, login } from '@/services/auth-service';
import { NotificationType, notify } from '@/services/notification-service';
import { sleep } from '@/utils/promise';
import { Platform } from '@/models/platform';

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

async function trySend<T = any>(method: string, path: string, body?: any): Promise<Result<T>> {
  const token = await getToken();

  const response = await fetch(API_BASE_URL + path, {
    method,
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: body != null ? JSON.stringify(body) : undefined,
  });

  if (response.status === StatusCodes.UNAUTHORIZED) {
    notify('Session expired', NotificationType.Warning);
    await sleep(1000);
    await login((localStorage.getItem('user-platform') as Platform) || Platform.Steam);
    return null!;
  }

  return response.status !== StatusCodes.NO_CONTENT ? await response.json() : null;
}

async function send(method: string, path: string, body?: any): Promise<any> {
  const result = await trySend(method, path, body);

  if (result === null) {
    return null;
  }

  if (result.errors === null) {
    return result.data;
  }

  const [error] = result.errors || [];

  if (error?.type === ErrorType.InternalError) {
    notify(error.title!, NotificationType.Danger);
    throw new Error('Server error');
  } else {
    notify(error.title!, NotificationType.Warning);
    throw new Error('Bad request');
  }
}

export function tryGet<T = any>(path: string): Promise<Result<T>> {
  return trySend<T>('GET', path);
}

export function get<T = any>(path: string): Promise<T> {
  return send('GET', path);
}

export function post<T = any>(path: string, body?: any): Promise<T> {
  return send('POST', path, body);
}

export function put<T = any>(path: string, body?: any): Promise<T> {
  return send('PUT', path, body);
}

export function del(path: string): Promise<any> {
  return send('DELETE', path);
}
