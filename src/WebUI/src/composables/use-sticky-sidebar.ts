// TODO: SPEC
export const useStickySidebar = (
  el: Ref<HTMLDivElement | null>,
  paddingTop = 0,
  paddingBottom = 0
) => {
  const elHeight = ref<number>(0);
  const screenHeight = ref<number>(window.innerHeight);
  const endScroll = computed(() => screenHeight.value - elHeight.value);
  const currPos = ref<number>(window.scrollY);
  const top = ref<number>(paddingTop);

  let timeout = 0; // for debounced scroll

  // observe stickyEl height change
  const resizeObserver = new ResizeObserver(entries => {
    const [entry] = entries;

    if (elHeight.value !== entry.contentRect.height) {
      _handleScrollEventListener();
    }
  });

  const _setTop = (val: number) => {
    top.value = val;
  };

  const _updateElHeight = () => {
    elHeight.value = el.value?.offsetHeight || 0;
  };

  const _onScroll = () => {
    if (timeout) {
      window.cancelAnimationFrame(timeout);
    }

    timeout = window.requestAnimationFrame(() => {
      const newPos = window.scrollY;
      // console.table([{ currPos: currPos.value, newPos, endScroll: endScroll.value, top: top.value }]);

      // UP
      if (newPos < currPos.value) {
        if (top.value < paddingTop) {
          _setTop(top.value + currPos.value - newPos);
          // console.log('[UP] 1', top.value);
        } else {
          _setTop(paddingTop);
          // console.log('[UP] 2', top.value);
        }
      }
      // DOWN
      else {
        if (top.value > endScroll.value) {
          _setTop(top.value + currPos.value - newPos);
          // console.log('[DOWN] 1', top.value);
        } else {
          _setTop(endScroll.value - paddingBottom);
          // console.log('[DOWN] 2', top.value);
        }
      }

      currPos.value = newPos;
    });
  };

  const _onResize = () => {
    screenHeight.value = window.innerHeight;

    _handleScrollEventListener();
  };

  const _handleScrollEventListener = () => {
    _updateElHeight();

    if (elHeight.value > screenHeight.value - paddingTop) {
      // events with the same parameters aren't duplicated, there's no point in checking
      window.addEventListener('scroll', _onScroll, {
        capture: false,
        passive: true,
      });
    } else {
      window.removeEventListener('scroll', _onScroll);
    }
  };

  onMounted(() => {
    if (el.value !== null) {
      window.addEventListener('resize', _onResize);
      resizeObserver.observe(el.value!);

      _handleScrollEventListener();
    }
  });

  onBeforeUnmount(() => {
    if (el.value !== null) {
      window.removeEventListener('resize', _onResize);
      window.removeEventListener('scroll', _onScroll);
      resizeObserver.unobserve(el.value);
    }
  });

  return {
    top: readonly(top),
  };
};
