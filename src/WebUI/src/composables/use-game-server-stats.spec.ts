import { mount } from '@vue/test-utils';

const mockGetGameServerStats = vi.fn();
vi.mock('@/service/game-server-statistics-service', () => ({
  getGameServerStats: mockGetGameServerStats,
}));

const mockSubscribe = vi.fn();
const mockUnsubscribe = vi.fn();
const mockUsePollInterval = vi.fn().mockImplementation(() => ({
  subscribe: mockSubscribe,
  unsubscribe: mockUnsubscribe,
}));
vi.mock('@/composables/use-poll-interval', () => ({
  usePollInterval: mockUsePollInterval,
}));

import { useGameServerStats } from './use-game-server-stats';

it('useGameServerStats composable lifecycle', async () => {
  const TestComponent = defineComponent({
    template: '<div/>',
    setup() {
      return {
        ...useGameServerStats(),
      };
    },
  });

  const wrapper = mount(TestComponent);

  expect(mockSubscribe).toBeCalled();
  expect(mockGetGameServerStats).not.toBeCalled();

  wrapper.unmount();
  expect(mockUnsubscribe).toBeCalled();
});
