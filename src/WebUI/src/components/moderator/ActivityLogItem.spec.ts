import { mount } from '@vue/test-utils';
import { type ActivityLog, ActivityLogType } from '@/models/activity-logs';
import ActivityLogItem from './ActivityLogItem.vue';

const getWrapper = (activityLog: ActivityLog, isSelfUser = true) =>
  mount(ActivityLogItem, {
    shallow: true,
    props: {
      activityLog,
      user: {
        id: 2,
      },
      users: {
        '1': {
          id: 1,
        },
      },
      isSelfUser,
    },
    global: {
      renderStubDefaultSlot: true,
      stubs: {
        VTooltip: {
          template: `<div>
                    <slot />
                    <slot name="popper"  />
                  </div>`,
        },
      },
    },
  });

it('gold, heirloomPoints, itemId, experience', () => {
  const wrapper = getWrapper({
    id: 1,
    type: ActivityLogType.ItemUpgraded,
    userId: 2,
    metadata: {
      gold: '10',
      price: '10',
      heirloomPoints: '100',
      experience: '1000',
      itemId: 'mlp_armor',
    },
    createdAt: new Date('2023-03-30T18:00:00.0000000Z'),
  });

  expect(wrapper.find('[data-aq-i18n-t-stub]').attributes('keypath')).toEqual(
    'activityLog.tpl.ItemUpgraded'
  );

  expect(wrapper.find('[data-aq-addLogItem-tpl-goldPrice]')).toBeDefined();
  expect(wrapper.findComponent({ name: 'Coin' }).props('value')).toEqual(10);
  expect(wrapper.find('[data-aq-addLogItem-tpl-heirloomPoints]')).toBeDefined();

  expect(wrapper.find('[data-aq-addLogItem-tpl-itemId]')).toBeDefined();
  expect(wrapper.find('[data-aq-addLogItem-tpl-experience]')).toBeDefined();
});

it('Team hit', async () => {
  const wrapper = getWrapper({
    id: 1,
    type: ActivityLogType.TeamHit,
    userId: 2,
    metadata: {
      damage: '10',
      targetUserId: '1',
    },
    createdAt: new Date('2023-03-30T18:00:00.0000000Z'),
  });

  expect(wrapper.classes('self-start')).toBeDefined();

  expect(wrapper.find('[data-aq-i18n-t-stub]').attributes('keypath')).toEqual(
    'activityLog.tpl.TeamHit'
  );

  const userMediaComponents = wrapper.findAllComponents({ name: 'UserMedia' });
  expect(userMediaComponents.length).toEqual(2);
  expect(userMediaComponents.at(0)!.props('user')).toEqual({ id: 2 });
  expect(userMediaComponents.at(1)!.props('user')).toEqual({ id: 1 });

  expect(wrapper.find('[data-aq-addLogItem-tpl-damage]')).toBeDefined();

  const addUserBtn = wrapper.find('[data-aq-addLogItem-addUser-btn]');
  await addUserBtn.trigger('click');
  expect(wrapper.emitted()['addUser'][0]).toEqual([1]);
});

it('Add type', async () => {
  const wrapper = getWrapper({
    id: 1,
    type: ActivityLogType.ChatMessageSent,
    userId: 2,
    metadata: {},
    createdAt: new Date('2023-03-30T18:00:00.0000000Z'),
  });

  const addTagTrigger = wrapper.find('[data-aq-addLogItem-type]');
  await addTagTrigger.trigger('click');

  expect(wrapper.emitted()['addType'][0]).toEqual([ActivityLogType.ChatMessageSent]);
});
