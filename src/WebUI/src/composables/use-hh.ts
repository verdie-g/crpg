import { computedWithControl } from '@vueuse/core';

import { Region } from '@/models/region';
import { getHHEventByRegion, getHHEventRemaining } from '@/services/hh-service';
import { useUserStore } from '@/stores/user';
import { notify } from '@/services/notification-service';
import { t } from '@/services/translate-service';

export const useHappyHours = () => {
  const userStore = useUserStore();
  let interval: number;

  const source = ref();
  const HHEvent = computedWithControl(
    () => source.value,
    () => getHHEventByRegion(userStore.user!.region || Region.Eu)
  );

  const HHEventRemaining = computed(() => getHHEventRemaining(HHEvent.value));

  const isHHCountdownEnded = ref<boolean>(true);

  onMounted(() => {
    // @ts-ignore
    interval = setInterval(HHEvent.trigger, 5000);
  });

  onBeforeUnmount(() => {
    clearInterval(interval);
  });

  const onEndHHCountdown = () => {
    isHHCountdownEnded.value = false;
    notify(t('hh.notify.ended'));
  };

  const onStartHHCountdown = () => {
    // TODO:
    // notify(t('hh.notify.started'));
  };

  // https://fengyuanchen.github.io/vue-countdown/
  const transformSlotProps = (props: Record<string, number>) => {
    return Object.entries(props).reduce((out, [key, value]) => {
      out[key] = value < 10 ? `0${value}` : String(value);
      return out;
    }, {} as Record<string, string>);
  };

  return {
    HHEvent,
    HHEventRemaining,
    isHHCountdownEnded,
    onStartHHCountdown,
    onEndHHCountdown,
    transformSlotProps,
  };
};
