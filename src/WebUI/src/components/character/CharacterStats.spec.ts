it.todo('TODO:', () => {});

// import { mount } from '@vue/test-utils';
// import { CharacterCharacteristics } from '@/models/character';

// vi.mock('@/services/characters-service', () => ({
//   computeSpeedStats: vi.fn().mockReturnValue({
//     weightReductionFactor: 1,
//     freeWeight: 2.5,
//     perceivedWeight: 5.26,
//     nakedSpeed: 0.6854600000000001,
//     currentSpeed: 0.5974795142685528,
//     timeToMaxSpeed: 2.025430347714705,
//   }),
// }));

// import CharacterStats from './CharacterStats.vue';

// describe('ItemCard', () => {
//   it('empty', async () => {
//     const wrapper = mount(CharacterStats, {
//       props: {
//         characteristics: {
//           attributes: { strength: 3, agility: 3 },
//           skills: { athletics: 1 },
//         } as CharacterCharacteristics,
//         weight: 7.76,
//       },
//       shallow: true,
//     });

//     expect(wrapper.get('[data-aq-character-speed-stats="free-weight"]').text()).toEqual(
//       '2.5 / 2.5'
//     );
//     expect(wrapper.get('[data-aq-character-speed-stats="weight-reduction"]').text()).toEqual('0 %');
//     expect(wrapper.get('[data-aq-character-speed-stats="perceived-weight"]').text()).toEqual(
//       '5.26'
//     );
//     expect(wrapper.get('[data-aq-character-speed-stats="time-to-max-speed"]').text()).toEqual(
//       '2.025 s'
//     );
//     expect(wrapper.get('[data-aq-character-speed-stats="naked-speed"]').text()).toEqual('0.685');
//     expect(wrapper.get('[data-aq-character-speed-stats="current-speed"]').text()).toEqual('0.597');
//   });
// });
