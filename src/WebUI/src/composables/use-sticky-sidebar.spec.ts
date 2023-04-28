// https://github.com/verdie-g/crpg/issues/873
// https://user-images.githubusercontent.com/33551334/234285036-e96bd6a2-26c8-4ddc-9310-b2528f3ab70c.png

import { mount } from '@vue/test-utils';
import { useStickySidebar } from './use-sticky-sidebar';

const spyAddEventListener = vi.spyOn(window, 'addEventListener');
const spyRemoveEventListener = vi.spyOn(window, 'removeEventListener');

// @ts-ignore
vi.spyOn(window, 'requestAnimationFrame').mockImplementation(cb => cb());

const mockedObserve = vi.fn();
const mockedUnObserve = vi.fn();
const ResizeObserverMock = vi.fn(() => ({
  observe: mockedObserve,
  unobserve: mockedUnObserve,
}));
vi.stubGlobal('ResizeObserver', ResizeObserverMock);

const WINDOW_VIEWPORT_HEIGHT = 768;
const mockWindowHeight = vi
  .spyOn(window, 'innerHeight', 'get')
  .mockImplementation(() => WINDOW_VIEWPORT_HEIGHT);

const triggerScroll = async (y: number = 0) => {
  Object.defineProperty(global.window, 'scrollY', { value: y });
  window.dispatchEvent(new Event('scroll'));
  return nextTick();
};

const triggerResize = (height: number) => {
  mockWindowHeight.mockImplementationOnce(() => height);
  window.dispatchEvent(new Event('resize'));
};

const stickySidebarHeight = 1080;
const maxStickyTop = stickySidebarHeight - WINDOW_VIEWPORT_HEIGHT; // 312

const getComponent = (height: number, offsetTop: number, offsetBottom: number) =>
  defineComponent({
    template: '<div :style="{ top: `${top}px` }" ></div>',

    setup() {
      // one way to test the template reference
      const el = document.createElement('div');
      vi.spyOn(el, 'offsetHeight', 'get').mockImplementation(() => height);

      const { top } = useStickySidebar(ref(el), offsetTop, offsetBottom);

      return {
        top,
      };
    },
  });

afterEach(async () => {
  await triggerScroll(0);
});

describe('sticky el h < viewport h', () => {
  it('don`t listen scroll event', async () => {
    const wrapper = mount(getComponent(700, 0, 0));

    expect(spyAddEventListener).toHaveBeenCalledTimes(1); // only resize evt
    expect(spyAddEventListener).toHaveBeenNthCalledWith(1, 'resize', expect.anything());

    wrapper.unmount();
  });

  it('start listen scroll event after resize', async () => {
    const wrapper = mount(getComponent(700, 0, 0));

    expect(spyAddEventListener).toHaveBeenCalledTimes(1);
    expect(spyAddEventListener).toHaveBeenNthCalledWith(1, 'resize', expect.anything());

    triggerResize(500); // resize window 768px->500px

    expect(spyAddEventListener).toHaveBeenCalledTimes(2);
    expect(spyAddEventListener).toHaveBeenNthCalledWith(
      2,
      'scroll',
      expect.anything(),
      expect.anything()
    );

    wrapper.unmount();
  });
});

describe('sticky el h >= viewport h', () => {
  it('listen scroll evt', async () => {
    const wrapper = mount(getComponent(stickySidebarHeight, 0, 0));

    expect(spyAddEventListener).toHaveBeenCalledTimes(2);
    expect(spyAddEventListener).toHaveBeenNthCalledWith(1, 'resize', expect.anything());
    expect(spyAddEventListener).toHaveBeenNthCalledWith(
      2,
      'scroll',
      expect.anything(),
      expect.anything()
    );

    wrapper.unmount();
  });

  it('remove scroll evt listener, after resize window', async () => {
    const wrapper = mount(getComponent(stickySidebarHeight, 0, 0));

    expect(spyAddEventListener).toHaveBeenCalledTimes(2);
    expect(spyAddEventListener).toHaveBeenNthCalledWith(1, 'resize', expect.anything());
    expect(spyAddEventListener).toHaveBeenNthCalledWith(
      2,
      'scroll',
      expect.anything(),
      expect.anything()
    );

    triggerResize(1080); // resize window 768px->1080px

    expect(spyRemoveEventListener).toHaveBeenCalledTimes(1);
    expect(spyRemoveEventListener).toHaveBeenNthCalledWith(1, 'scroll', expect.anything());

    wrapper.unmount();
  });
});

it('remove listeners when cmp is unmounted', () => {
  const wrapper = mount(getComponent(stickySidebarHeight, 0, 0));

  expect(spyAddEventListener).toHaveBeenCalledTimes(2);
  expect(spyAddEventListener).toHaveBeenNthCalledWith(1, 'resize', expect.anything());
  expect(spyAddEventListener).toHaveBeenNthCalledWith(
    2,
    'scroll',
    expect.anything(),
    expect.anything()
  );

  wrapper.unmount();

  expect(spyRemoveEventListener).toHaveBeenCalledTimes(2);
  expect(spyRemoveEventListener).toHaveBeenNthCalledWith(1, 'resize', expect.anything());
  expect(spyRemoveEventListener).toHaveBeenNthCalledWith(2, 'scroll', expect.anything());
});

describe('el ResizeObserver', () => {
  it('observe', () => {
    const wrapper = mount(getComponent(stickySidebarHeight, 0, 0));
    expect(mockedObserve).toHaveBeenCalledTimes(1);

    wrapper.unmount();
  });

  it('unobserve', () => {
    const wrapper = mount(getComponent(stickySidebarHeight, 0, 0));
    wrapper.unmount();

    expect(mockedUnObserve).toHaveBeenCalledTimes(1);
  });
});

describe('scroll DOWN', () => {
  it('scrollY:0px', async () => {
    const wrapper = mount(getComponent(stickySidebarHeight, 0, 0));
    expect(wrapper.attributes('style')).toEqual('top: 0px;');

    wrapper.unmount(); // fire onBeforeUnmount hook - removeEvtListeners
  });

  it('scrollY:50px', async () => {
    const wrapper = mount(getComponent(stickySidebarHeight, 0, 0));
    await triggerScroll(50);
    expect(wrapper.attributes('style')).toEqual('top: -50px;');

    wrapper.unmount();
  });
});

it('Scroll UP', async () => {
  const wrapper = mount(getComponent(stickySidebarHeight, 0, 0));

  // scrolling DOWN
  await triggerScroll(maxStickyTop);
  await triggerScroll(1400);

  // then UP
  await triggerScroll(1200);
  expect(wrapper.attributes('style')).toEqual(`top: -112px;`);

  await triggerScroll(1100);
  expect(wrapper.attributes('style')).toEqual(`top: -12px;`);

  await triggerScroll(maxStickyTop);
  await triggerScroll(maxStickyTop - 1);
  expect(wrapper.attributes('style')).toEqual(`top: 0px;`);

  wrapper.unmount();
});
