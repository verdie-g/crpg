const CLAN_INVINTATIONS = [{ id: 2 }, { id: 3 }];
const mockedGetClanInvitations = vi.fn().mockResolvedValue(CLAN_INVINTATIONS);
vi.mock('@/services/clan-service', () => ({
  getClanInvitations: mockedGetClanInvitations,
}));

import { useClanApplications } from './use-clan-applications';

it('initial state', () => {
  const { applications, applicationsCount } = useClanApplications(2);

  expect(applications.value).toEqual([]);
  expect(applicationsCount.value).toEqual(0);
});

it('initial state', async () => {
  const { applications, applicationsCount, loadClanApplications } = useClanApplications(2);

  await loadClanApplications();

  expect(applications.value).toEqual(CLAN_INVINTATIONS);
  expect(applicationsCount.value).toEqual(2);
  expect(mockedGetClanInvitations).toBeCalledWith(2, ['Request'], ['Pending']);
});
