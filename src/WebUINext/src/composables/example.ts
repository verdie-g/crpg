export const useCounter = () => {
  const counter = ref<number>(0);

  const increase = (): void => {
    counter.value += 1;
  };

  return { counter: readonly(counter), increase };
};
