import { mockGet } from 'vi-fetch';
import { response } from '@/__mocks__/crpg-client';

import { getGameServerStats } from '@/services/game-server-statistics-service';

const mockGameServerStats = {
  total: {
    playerCount: 0,
  },
};

it('getGameServerStats', async () => {
  mockGet('/game-server-statistics').willResolve(
    response({
      total: {
        playerCount: 0,
      },
    })
  );

  expect(await getGameServerStats()).toEqual(mockGameServerStats);
});
