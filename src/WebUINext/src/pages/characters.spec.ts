import { createTestingPinia } from '@pinia/testing';
import { mountWithRouter } from '@/__test__/unit/utils';
import { type Character } from '@/models/character';
import { type User } from '@/models/user';

import { useUserStore } from '@/stores/user';

const mountOptions = {
  global: {
    plugins: [createTestingPinia()],
  },
};

const userStore = useUserStore();

import Page from './characters.vue';

const routes = [
  {
    path: '/characters/',
    component: Page,
    children: [
      {
        name: 'CharactersId',
        path: ':id',
        component: {
          template: `<div data-aq-characters-id-index-page>CharactersId Index page stub</div>`,
        },
      },
      {
        name: 'Characters',
        path: '',
        component: {
          template: `<div data-aq-characters-index-page>Characters Index page stub</div>`,
        },
      },
    ],
  },
];
const route = {
  path: '/characters/2',
};

beforeEach(() => {
  userStore.$reset();
});

it.todo('characters list', async () => {
  userStore.user = {
    activeCharacterId: 2,
  } as User;
  userStore.characters = [
    { id: 1, name: 'Applejack', level: 31 },
    { id: 2, name: 'Spike', level: 1 },
  ] as Character[];

  const { wrapper } = await mountWithRouter(mountOptions, routes, route);

  const charactersList = wrapper.findAllComponents({ name: 'DropdownItem' });

  // console.log(wrapper.html());
  console.log(charactersList.at(0)!.html());

  // expect(charactersList.at(0)!.props('to')).toEqual({ name: 'CharactersId', params: { id: 1 } });
  // expect(charactersList.at(0)!.find('[data-aq-characters-list-item-attr="level"]')!.text()).toEqual(
  //   '31'
  // );
  // expect(
  //   charactersList.at(0)!.find('[data-aq-characters-list-item-attr="active"]')!.exists()
  // ).toBeFalsy();

  // expect(charactersList.at(1)!.props('to')).toEqual({ name: 'CharactersId', params: { id: 2 } });
  // expect(
  //   charactersList.at(1)!.find('[data-aq-characters-list-item-attr="active"]')!.exists()
  // ).toBeTruthy();
});

// it('router-view - index page', async () => {
//   const { wrapper } = await mountWithRouter(mountOptions, routes, route);

//   expect(wrapper.find('[data-aq-characters-index-page]')!.exists()).toBeTruthy();
// });
