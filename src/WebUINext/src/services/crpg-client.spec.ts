const mockGetToken = vi.fn();
const mockLogin = vi.fn();

vi.mock('@/services/auth-service', () => ({
  getToken: mockGetToken,
  login: mockLogin,
}));

const mockNotify = vi.fn();
vi.mock('@/services/notification-service', async () => {
  return {
    ...(await vi.importActual<typeof import('@/services/notification-service')>(
      '@/services/notification-service'
    )),
    notify: mockNotify,
  };
});

import { mockGet, mockPost, mockPut, mockDelete } from 'vi-fetch';
import { get, post, put, del } from './crpg-client';
import { ErrorType, type Result } from '@/models/crpg-client-result';

describe('get', () => {
  const path = '/test-get';

  it('OK 200', async () => {
    const response: Result<any> = {
      data: {
        name: 'Rainbow Dash',
        description: 'The best',
      },
      errors: null,
    };

    mockGet(path).willResolve(response, 200);

    const result = await get(path);

    expect(result).toEqual(response.data);
  });

  it('with error - InternalError', async () => {
    const response: Result<any> = {
      data: null,
      errors: [
        {
          traceId: null,
          type: ErrorType.InternalError,
          code: '500',
          title: 'some error',
          detail: null,
          stackTrace: null,
        },
      ],
    };

    mockGet(path).willResolve(response, 200);

    // ref https://vitest.dev/api/#rejects
    await expect(get(path)).rejects.toThrow('Server error');

    expect(mockNotify).toBeCalledWith('some error', 'danger');
  });

  it('with error - other', async () => {
    const response: Result<any> = {
      data: null,
      errors: [
        {
          traceId: null,
          type: ErrorType.Forbidden,
          code: '500',
          title: 'some warning',
          detail: null,
          stackTrace: null,
        },
      ],
    };

    mockGet(path).willResolve(response, 200);

    await expect(get(path)).rejects.toThrow('Bad request');

    expect(mockNotify).toBeCalledWith('some warning', 'warning');
  });

  it('Unauthorized', async () => {
    mockGet(path).willFail({}, 401);

    const result = await get(path);

    expect(result).toEqual(null);

    expect(mockNotify).toBeCalledWith('Session expired', 'warning');
  });
});

describe('post', () => {
  const path = '/test-post';

  it('OK 204 NO_CONTENT', async () => {
    const response: Result<any> = {
      data: {},
      errors: null,
    };

    mockPost(path).willResolve(response, 204);

    const result = await post(path, {
      name: 'Rainbow Dash',
      description: 'The best',
    });

    expect(result).toEqual(null);
  });
});

describe('put', () => {
  const path = '/test-put/1';

  it('OK 204', async () => {
    const response: Result<any> = {
      data: {},
      errors: null,
    };

    mockPut(path).willResolve(response, 204);

    const result = await put(path, {
      name: 'Rainbow Dash',
      description: 'The best',
    });

    expect(result).toEqual(null);
  });

  describe('del', () => {
    const path = '/test-del/1';

    it('OK 204', async () => {
      const response: Result<any> = {
        data: {},
        errors: null,
      };

      mockDelete(path).willResolve(response, 204);

      const result = await del(path);

      expect(result).toEqual(null);
    });
  });
});
