// TODO: SPEC
export default (el: Ref<HTMLDivElement | null>, offsetTop = 0, offsetBottom = 0) => {
  const elHeight = ref<number>(0);

  const screenHeight = ref<number>(window.innerHeight);
  const endScroll = computed(() => screenHeight.value - offsetTop - elHeight.value);
  const currPos = ref<number>(window.scrollY);

  const active = ref<boolean>(false);
  const top = ref<number>(offsetTop);

  const setTop = (val: number) => {
    top.value = val;
  };

  const onScroll = () => {
    const newPos = window.scrollY;

    if (!active.value) {
      window.requestAnimationFrame(() => {
        if (elHeight.value > screenHeight.value - offsetTop) {
          if (newPos < currPos.value) {
            setTop(top.value < offsetTop ? top.value + currPos.value - newPos : offsetTop);
          } else {
            setTop(
              top.value > endScroll.value
                ? top.value + currPos.value - newPos
                : endScroll.value - offsetBottom
            );
          }
        } else {
          setTop(offsetTop);
        }

        currPos.value = window.scrollY;
        active.value = false;
      });

      active.value = true;
    }
  };

  const onResize = () => {
    screenHeight.value = window.innerHeight;
    elHeight.value = el.value?.offsetHeight || 0;
  };

  onMounted(() => {
    elHeight.value = el.value?.offsetHeight || 0;
    window.addEventListener('scroll', onScroll);
    window.addEventListener('resize', onResize);
  });

  onBeforeUnmount(() => {
    window.removeEventListener('scroll', onScroll);
    window.removeEventListener('resize', onResize);
  });

  return {
    top: readonly(top),
  };
};
