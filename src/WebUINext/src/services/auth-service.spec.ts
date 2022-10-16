const mockSigninRedirect = vi.fn();
const mockGetUser = vi.fn();
const mockSignoutRedirect = vi.fn();
const mockSigninSilent = vi.fn();

import {
  extractToken,
  parseJwt,
  getUser,
  login,
  signInSilent,
  logout,
  getToken,
  getDecodedToken,
} from './auth-service';

const JWT_TOKEN =
  'eyJhbGciOiJSUzI1NiIsImtpZCI6IkUyMUZDMEIzMUYwRkUxOEQwM0I2ODUzNzAxQkRFQUIyIiwidHlwIjoiYXQrand0In0.eyJuYmYiOjE2NjU5NDk4NjUsImV4cCI6MTY2NTk1MDQ2NSwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6ODAwMCIsImNsaWVudF9pZCI6ImNycGctd2ViLXVpIiwic3ViIjoiMiIsImF1dGhfdGltZSI6MTY2NTk0OTg1MiwiaWRwIjoibG9jYWwiLCJyb2xlIjoiVXNlciIsImp0aSI6IjY2N0RFMzZDOTg0RUFDRENBMDZBNUVDQkVGNTBDRDBEIiwic2lkIjoiOTAxRTFBNUNEQUVBRjMxMjRBMjZCODJDRDUzNjM2RTYiLCJpYXQiOjE2NjU5NDk4NTMsInNjb3BlIjpbIm9wZW5pZCIsInVzZXJfYXBpIiwib2ZmbGluZV9hY2Nlc3MiXSwiYW1yIjpbInB3ZCJdfQ.Js5ze2JNSew0m5UD86HyA62cMvPxnFJACa8IhQW59vzTABticqtdo8070soza-11JJyT_zHAI97SWSQRUoQ4w1pCdPmMsh5HyMueMx-OO_cFpkZg6PkpQlaSYB_Z_916k5nhCRVkNK7X4H2MByhkMd1rh0yFFGvYKVAnWKNYZFYL2y9VPv510b8RV0JyfjZvwLRN-cU2n7xVHkSzSE7WYz6X3D9l4MS-cOZRt3Y62EqjuHbBY3sJk-VIJCAuc0puGmms69_-9KV5cMLNfwoHOihYctSB7JVh-oSlXxuvkZHz_23eQnvsW5DLCgrOKnxMQBj44TRBwqHmrLl89UV3GQ';

const DECODED_TOKEN = {
  nbf: 1665949865,
  exp: 1665950465,
  iss: 'https://localhost:8000',
  client_id: 'crpg-web-ui',
  sub: '2',
  auth_time: 1665949852,
  idp: 'local',
  role: 'User',
  jti: '667DE36C984EACDCA06A5ECBEF50CD0D',
  sid: '901E1A5CDAEAF3124A26B82CD53636E6',
  iat: 1665949853,
  scope: ['openid', 'user_api', 'offline_access'],
  amr: ['pwd'],
};

vi.mock('oidc-client-ts', () => ({
  UserManager: vi.fn().mockImplementation(() => ({
    signinRedirect: mockSigninRedirect,
    getUser: mockGetUser,
    signoutRedirect: mockSignoutRedirect,
    signinSilent: mockSigninSilent,
  })),
  WebStorageStateStore: vi.fn(),
}));

describe('utils', () => {
  it('extractToken', () => {
    const payload = {
      access_token: JWT_TOKEN,
    };

    expect(extractToken(payload)).toEqual(JWT_TOKEN);
  });

  it('parseJwt', () => {
    expect(parseJwt(JWT_TOKEN)).toEqual(DECODED_TOKEN);
  });
});

it('getUser', async () => {
  mockGetUser.mockResolvedValueOnce({ foo: 'bar' });
  expect(await getUser()).toEqual({ foo: 'bar' });
  expect(mockGetUser).toHaveBeenCalled();
});

it('login', async () => {
  mockSigninRedirect.mockResolvedValue(true);
  await login();
  expect(mockSigninRedirect).toHaveBeenCalled();
});

it('logout', async () => {
  await logout();
  expect(mockSignoutRedirect).toHaveBeenCalled();
});

describe('signInSilent', () => {
  it('signInSilent - user logged in', async () => {
    mockSigninSilent.mockResolvedValue({ access_token: 'access_token' });
    const token = await signInSilent();

    expect(token).toEqual('access_token');
    expect(mockSigninSilent).toHaveBeenCalled();
  });

  it('signInSilent - user not logged in', async () => {
    mockSigninSilent.mockRejectedValue(false);
    const token = await signInSilent();

    expect(token).toEqual(null);
    expect(mockSigninSilent).toHaveBeenCalled();
  });
});

it('getToken', async () => {
  mockGetUser.mockResolvedValueOnce({ access_token: 'access_token' });

  const token = await getToken();

  expect(token).toEqual('access_token');
  expect(mockGetUser).toHaveBeenCalled();
});

it('getDecodedToken', async () => {
  mockGetUser.mockResolvedValueOnce({ access_token: JWT_TOKEN });

  const decodedToken = await getDecodedToken();

  expect(decodedToken).toEqual({
    userId: parseInt(DECODED_TOKEN.sub, 10),
    userRole: DECODED_TOKEN.role,
    expiration: new Date(DECODED_TOKEN.exp * 1000),
    issuedAt: new Date(DECODED_TOKEN.iat * 1000),
  });
  expect(mockGetUser).toHaveBeenCalled();
});
