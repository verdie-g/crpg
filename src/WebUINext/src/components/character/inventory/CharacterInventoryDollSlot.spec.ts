it.todo('TODO:', () => {});
// import { mount } from '@vue/test-utils';
// import { UserItem } from '@/models/user';
// import { ItemSlot } from '@/models/item';

// vi.mock('@/services/item-service', () => ({
//   getItemImage: vi.fn().mockImplementation((name: string) => `PATH_TO_CDN/images/${name}.png`),
// }));

// import CharacterInventoryDoll from './CharacterInventoryDoll.vue';

// const userItem = {
//   id: 1,
//   baseItem: {
//     id: 'super-item',
//     name: 'Flax Seed',
//   },
// } as UserItem;

// describe('ItemCard', () => {
//   it('empty', async () => {
//     const wrapper = mount(CharacterInventoryDoll, {
//       props: {
//         slot: ItemSlot.Weapon1,
//         title: 'Weapon 1',
//         placeholder: 'weapon',
//       },
//       shallow: true,
//     });

//     const placeholder = wrapper.find('[data-aq-character-slot-placeholder]');

//     expect(wrapper.find('[data-aq-character-slot-item-thumb]').exists()).toBeFalsy();
//     expect(placeholder.exists()).toBeTruthy();

//     expect(placeholder.attributes('alt')).toEqual('Weapon 1');
//     expect(placeholder.attributes('title')).toBe('Weapon 1');
//     expect(placeholder.attributes('src')).toEqual(
//       'PATH_TO_ASSET/images/themes/oruga-tailwind/img/weapon.png' // TODO:
//     );
//   });

//   it('not empty', async () => {
//     const wrapper = mount(CharacterInventoryDoll, {
//       props: {
//         slot: ItemSlot.Weapon1,
//         title: 'Weapon 1',
//         placeholder: 'weapon',
//         item: userItem,
//       },
//       shallow: true,
//     });

//     const itemThumb = wrapper.find('[data-aq-character-slot-item-thumb]');

//     expect(itemThumb.exists()).toBeTruthy();
//     expect(wrapper.find('[data-aq-character-slot-placeholder]').exists()).toBeFalsy();

//     expect(itemThumb.attributes('alt')).toEqual(userItem.baseItem.name);
//     expect(itemThumb.attributes('title')).toEqual(userItem.baseItem.name);

//     expect(itemThumb.attributes('src')).toEqual(`PATH_TO_CDN/images/${userItem.baseItem.id}.png`);
//   });

//   it('emit', async () => {
//     const wrapper = mount(CharacterInventoryDoll, {
//       props: {
//         slot: ItemSlot.Weapon1,
//         title: 'Weapon 1',
//         placeholder: 'weapon',
//         item: userItem,
//       },
//       shallow: true,
//     });

//     await wrapper.find('[data-aq-character-slot-action="unEquip"]').trigger('click');
//     expect(wrapper.emitted('unEquip')).toHaveLength(1);
//   });

//   describe('statuses', async () => {
//     it('available', async () => {
//       const wrapper = mount(CharacterInventoryDoll, {
//         props: {
//           slot: ItemSlot.Weapon1,
//           title: 'Weapon 1',
//           placeholder: 'weapon',
//           available: true,
//         },
//         shallow: true,
//       });

//       expect(wrapper.classes('ring-2')).toBeTruthy();
//       expect(wrapper.classes('ring-secondary')).toBeFalsy();
//     });

//     it('available + focused', async () => {
//       const wrapper = mount(CharacterInventoryDoll, {
//         props: {
//           slot: ItemSlot.Weapon1,
//           title: 'Weapon 1',
//           placeholder: 'weapon',
//           available: true,
//           focused: true,
//         },
//         shallow: true,
//       });

//       expect(wrapper.classes('ring-2')).toBeTruthy();
//       expect(wrapper.classes('ring-secondary')).toBeTruthy();
//     });

//     it('invalid', async () => {
//       const wrapper = mount(CharacterInventoryDoll, {
//         props: {
//           slot: ItemSlot.Weapon1,
//           title: 'Weapon 1',
//           placeholder: 'weapon',
//           invalid: true,
//         },
//         shallow: true,
//       });

//       expect(wrapper.classes('ring-2')).toBeTruthy();
//       expect(wrapper.classes('ring-danger')).toBeTruthy();
//     });

//     it('remove', async () => {
//       const wrapper = mount(CharacterInventoryDoll, {
//         props: {
//           slot: ItemSlot.Weapon1,
//           title: 'Weapon 1',
//           placeholder: 'weapon',
//           remove: true,
//         },
//         shallow: true,
//       });

//       expect(wrapper.classes('ring-2')).toBeTruthy();
//       expect(wrapper.classes('ring-todo')).toBeTruthy(); // TODO:
//     });
//   });
// });
