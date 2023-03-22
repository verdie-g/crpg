const mockedGetClan = vi.fn().mockResolvedValue({ id: 2, tag: 'mlp' });
vi.mock('@/services/clan-service', () => ({
  getClan: mockedGetClan,
}));

import { useClan } from './use-clan';

it('initial state', () => {
  const { clan, clanId } = useClan('2');

  expect(clanId.value).toBe(2);
  expect(clan.value).toEqual(null);
});

it('load clan', async () => {
  const { clan, loadClan } = useClan('2');

  await loadClan();
  expect(clan.value).toEqual({ id: 2, tag: 'mlp' });
  expect(mockedGetClan).toBeCalledWith(2);
});
