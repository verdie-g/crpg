it.todo('TODO:', () => {});

// import { flushPromises } from '@vue/test-utils';
// import { createTestingPinia } from '@pinia/testing';
// import { type Character, CharacteristicConversion } from '@/models/character';
// import type { User } from '@/models/user';

// import mockCharacters from '@/__mocks__/characters.json';
// import mockCharacterStatistics from '@/__mocks__/character-statistics.json';
// import mockCharacterCharacteristics from '@/__mocks__/character-characteristics.json';

// const mockGetCharacterStatistics = vi.fn().mockResolvedValue(mockCharacterStatistics);
// const mockGetCharacterCharacteristics = vi.fn().mockResolvedValue(mockCharacterCharacteristics);
// const mockUpdateCharacterCharacteristics = vi.fn().mockResolvedValue(mockCharacterCharacteristics);
// const mockGetCharacterItems = vi.fn().mockResolvedValue([]); // TODO: mocks
// const mockUpdateCharacterItems = vi.fn().mockResolvedValue([]); // TODO: mocks

// const mockConvertCharacterCharacteristics = vi.fn();
// const mockActivateCharacter = vi.fn();
// const mockRespecializeCharacter = vi.fn().mockResolvedValue(mockCharacters[1]);
// const mockDeleteCharacter = vi.fn();

// const mockCanRetireValidate = vi.fn().mockReturnValue(true);
// const mockRetireCharacter = vi.fn().mockResolvedValue(mockCharacters[1]);

// const mockUpdateCharacter = vi.fn().mockResolvedValue(mockCharacters[1]);

// const mockCanSetCharacterForTournamentValidate = vi.fn().mockReturnValue(true);
// const mockSetCharacterForTournament = vi.fn().mockResolvedValue(mockCharacters[1]);

// vi.mock('@/services/characters-service', async () => ({
//   ...(await vi.importActual<typeof import('@/services/characters-service')>(
//     '@/services/characters-service'
//   )),
//   getCharacterItems: mockGetCharacterItems,
//   updateCharacterItems: mockUpdateCharacterItems,

//   getCharacterStatistics: mockGetCharacterStatistics,
//   getCharacterCharacteristics: mockGetCharacterCharacteristics,
//   updateCharacterCharacteristics: mockUpdateCharacterCharacteristics,
//   convertCharacterCharacteristics: mockConvertCharacterCharacteristics,

//   updateCharacter: mockUpdateCharacter,
//   activateCharacter: mockActivateCharacter,
//   respecializeCharacter: mockRespecializeCharacter,
//   deleteCharacter: mockDeleteCharacter,

//   canRetireValidate: mockCanRetireValidate,
//   retireCharacter: mockRetireCharacter,

//   canSetCharacterForTournamentValidate: mockCanSetCharacterForTournamentValidate,
//   setCharacterForTournament: mockSetCharacterForTournament,
// }));

// import { useUserStore } from '@/stores/user';
// import { mountWithRouter } from '@/__test__/unit/utils';
// import Page from './index.vued].vue';

// const userStore = useUserStore(createTestingPinia());

// const CHARACTER_ID = 5;

// const routes = [
//   {
//     name: 'characters-id',
//     path: '/characters/:id',
//     props: true,
//     component: Page,
//   },
//   {
//     name: 'characters',
//     path: '/characters',
//     props: true,
//     component: {
//       template: `<div></div>`,
//     },
//   },
// ];
// const route = {
//   name: 'characters-id',
//   params: { id: CHARACTER_ID },
// };
// const options = {
//   global: {
//     stubs: ['CharacterCharacteristic'],
//   },
// };

// describe('character-[id] page', () => {
//   beforeEach(() => {
//     userStore.user = {
//       activeCharacterId: CHARACTER_ID,
//     } as User;
//     userStore.characters = mockCharacters as Character[];
//   });

//   it('setup fn', async () => {
//     const { wrapper } = await mountWithRouter(options, routes, route);

//     expect(mockGetCharacterCharacteristics).toBeCalledWith(CHARACTER_ID);
//     expect(mockGetCharacterStatistics).toBeCalledWith(CHARACTER_ID);

//     expect(wrapper.find('[data-aq-character-info="name"]').text()).toEqual('Fluttershy');
//   });

//   it('change character page', async () => {
//     const { wrapper, router } = await mountWithRouter(options, routes, route);
//     const NEW_CHARACTER_ID = 6;
//     await router.push({ name: 'characters-id', params: { id: NEW_CHARACTER_ID } });

//     expect(mockGetCharacterCharacteristics).toBeCalledWith(NEW_CHARACTER_ID);
//     expect(mockGetCharacterStatistics).toBeCalledWith(NEW_CHARACTER_ID);

//     expect(wrapper.find('[data-aq-character-info="name"]').text()).toEqual('Rarity');
//   });

//   describe('character characteristic', () => {
//     it('on emit:commit', async () => {
//       const { wrapper } = await mountWithRouter(options, routes, route);

//       wrapper
//         .getComponent({ name: 'CharacterCharacteristic' })
//         .vm.$emit('commit', mockCharacterCharacteristics);

//       expect(mockUpdateCharacterCharacteristics).toBeCalledWith(
//         CHARACTER_ID,
//         mockCharacterCharacteristics
//       );
//     });

//     it('on emit:convertCharacterCharacteristics', async () => {
//       const { wrapper } = await mountWithRouter(options, routes, route);

//       wrapper
//         .getComponent({ name: 'CharacterCharacteristic' })
//         .vm.$emit('convertCharacterCharacteristics', CharacteristicConversion.AttributesToSkills);

//       expect(mockConvertCharacterCharacteristics).toBeCalledWith(
//         CHARACTER_ID,
//         CharacteristicConversion.AttributesToSkills
//       );
//     });
//   });

//   it('activate', async () => {
//     const { wrapper } = await mountWithRouter(options, routes, route);

//     await wrapper.find('[data-aq-control="character-active"]').setValue(true);
//     await flushPromises();

//     expect(mockActivateCharacter).toBeCalledWith(CHARACTER_ID, true);
//     expect(userStore.fetchUser).toHaveBeenCalled();
//   });

//   it('update', async () => {
//     const { wrapper } = await mountWithRouter(options, routes, route);

//     await wrapper.find('[data-aq-character-update-control="name"]').setValue('Pinkie Pie');
//     await wrapper.find('[data-aq-character-update-form]').trigger('submit');
//     await flushPromises();

//     expect(mockUpdateCharacter).toHaveBeenCalledWith(CHARACTER_ID, { name: 'Pinkie Pie' });
//     expect(userStore.replaceCharacter).toBeCalledWith(mockCharacters[1]);
//   });

//   it('respecialize', async () => {
//     const { wrapper } = await mountWithRouter(options, routes, route);
//     mockGetCharacterCharacteristics.mockReset();

//     await wrapper.find('[data-aq-character-action="respecialize"]').trigger('click');
//     await flushPromises();

//     expect(mockRespecializeCharacter).toBeCalledWith(CHARACTER_ID);
//     expect(userStore.replaceCharacter).toBeCalledWith(mockCharacters[1]);
//     expect(mockGetCharacterCharacteristics).toBeCalled();
//   });

//   it('delete Character', async () => {
//     const { wrapper, router } = await mountWithRouter(options, routes, route);
//     const spyRouterReplace = vi.spyOn(router, 'replace');

//     await wrapper.find('[data-aq-character-action="delete"]').trigger('click');
//     await flushPromises();

//     expect(mockDeleteCharacter).toBeCalledWith(CHARACTER_ID);
//     expect(spyRouterReplace).toBeCalledWith({ name: 'characters' });
//   });

//   describe('retire character', () => {
//     it('!canRetire', async () => {
//       mockCanRetireValidate.mockReturnValue(false);
//       const { wrapper } = await mountWithRouter(options, routes, route);

//       await wrapper.find('[data-aq-character-action="retire"]').trigger('click');

//       expect(mockRetireCharacter).not.toBeCalled();
//     });

//     it('can retire', async () => {
//       mockCanRetireValidate.mockReturnValue(true);
//       const { wrapper } = await mountWithRouter(options, routes, route);
//       mockGetCharacterCharacteristics.mockReset();

//       await wrapper.find('[data-aq-character-action="retire"]').trigger('click');
//       await flushPromises();

//       expect(mockRetireCharacter).toBeCalledWith(CHARACTER_ID);
//       expect(userStore.replaceCharacter).toBeCalledWith(mockCharacters[1]);
//       expect(mockGetCharacterCharacteristics).toBeCalledTimes(1);
//     });
//   });

//   describe('set character for tournament', () => {
//     it('!canSetCharacterForTournament', async () => {
//       mockCanSetCharacterForTournamentValidate.mockReturnValue(false);

//       const { wrapper } = await mountWithRouter(options, routes, route);

//       await wrapper.find('[data-aq-character-action="forTournament"]').trigger('click');
//       await flushPromises();

//       expect(mockSetCharacterForTournament).not.toBeCalled();
//     });

//     it('canSetCharacterForTournament', async () => {
//       mockCanSetCharacterForTournamentValidate.mockReturnValue(true);

//       const { wrapper } = await mountWithRouter(options, routes, route);
//       mockGetCharacterCharacteristics.mockReset();

//       await wrapper.find('[data-aq-character-action="forTournament"]').trigger('click');
//       await flushPromises();

//       expect(mockSetCharacterForTournament).toBeCalledWith(CHARACTER_ID);
//       expect(userStore.replaceCharacter).toBeCalledWith(mockCharacters[1]);
//       expect(mockGetCharacterCharacteristics).toBeCalledTimes(1);
//     });
//   });
// });
