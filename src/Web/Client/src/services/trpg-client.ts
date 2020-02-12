import { getToken } from '@/utils/auth';

const API_BASE_URL = 'http://localhost:5000/api';

function send(method: string, path: string, body?: any): Promise<any> {
  return fetch(API_BASE_URL + path, {
    method,
    headers: { Authorization: `Bearer ${getToken()}` },
    body,
  }).then(res => res.json());
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
