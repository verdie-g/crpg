function useTimeoutFn(cb: (...args: unknown[]) => any, interval: number) {
  let timer: number | null = null;

  function clear() {
    if (timer) {
      clearTimeout(timer);
      timer = null;
    }
  }

  function stop() {
    clear();
  }

  function start(...args: unknown[]) {
    clear();
    timer = setTimeout(() => {
      timer = null;

      cb(...args);
    }, interval) as unknown as number;
  }

  start();

  return {
    start,
    stop,
  };
}

export function useTimeoutPoll(fn: () => Promise<any>, interval: number) {
  const { start, stop: timeoutStop } = useTimeoutFn(loop, interval);

  let isActive = false;

  async function loop() {
    if (!isActive) return;

    await fn();
    start();
  }

  function resume() {
    if (!isActive) {
      isActive = true;
      loop();
    }
  }

  function pause() {
    isActive = false;
  }

  function stop() {
    isActive = false;
    timeoutStop();
  }

  resume();

  return {
    isActive,
    pause,
    stop,
    resume,
  };
}
