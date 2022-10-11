import { mount } from '@vue/test-utils';
import SvgIcon from '@/components/SvgIcon.vue';

it('should correct href attribute', () => {
  const wrapper = mount(SvgIcon, {
    props: {
      name: 'testIcon',
      viewBox: '0 0 20 20',
    },
  });

  expect(wrapper.find('use').attributes('href')).toEqual('#icon-testIcon');
  expect(wrapper.find('svg').attributes('viewBox')).toEqual('0 0 20 20');
});
