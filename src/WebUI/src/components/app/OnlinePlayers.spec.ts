import { mount } from '@vue/test-utils';
import { type GameServerStats } from '@/models/game-server-stats';
import OnlinePlayersVue from './OnlinePlayers.vue';
import { Region } from '@/models/region';

const getWrapper = (props: { gameServerStats: GameServerStats | null; showLabel: boolean }) =>
  mount(OnlinePlayersVue, {
    shallow: true,
    props,
    global: {
      renderStubDefaultSlot: true,
      stubs: {
        VTooltip: {
          template: `<div>
                      <slot/>
                      <slot name="popper"/>
                    </div>`,
        },
      },
    },
  });

it('testing api returning gameStats valid', async () => {
  const wrapper = getWrapper({
    gameServerStats: {
      total: { playingCount: 0 },
      regions: {
        [Region.Eu]: {
          playingCount: 0,
        },
      },
    },
    showLabel: false,
  });

  const onlinePlayersDiv = wrapper.find('[data-aq-online-players-count]');
  const regionsPopperContent = wrapper.find('[data-aq-region-stats]');

  expect(onlinePlayersDiv.text()).toEqual('0');
  expect(regionsPopperContent.exists()).toBeTruthy();
});

it('testing api returning gameStats null/Error', async () => {
  vi.mocked('@/services/translate-service');
  console.log(vi.mocked('@/services/translate-service'));
  const wrapper = getWrapper({
    gameServerStats: null,
    showLabel: false,
  });

  const onlinePlayersDiv = wrapper.find('[data-aq-online-players-count]');
  const regionsPopperContent = wrapper.find('[data-aq-region-stats]');

  expect(onlinePlayersDiv.text()).toEqual('?');
  expect(regionsPopperContent.exists()).toBeFalsy();
});
