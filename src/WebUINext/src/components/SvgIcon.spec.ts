import { mount } from '@vue/test-utils';
import SvgIcon from '@/components/SvgIcon.vue';

it('should correct href attribute', () => {
  const wrapper = mount(SvgIcon, {
    props: {
      prefix: 'testPrefix',
      name: 'testIcon',
    },
  });

  expect(wrapper.find('use').attributes('href')).toEqual('#testPrefix-testIcon');
});
