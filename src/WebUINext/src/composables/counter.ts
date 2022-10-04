export const useCounter = () => {
  const counter = ref(0);

  const increase = () => {
    counter.value += 1;
  };

  return { counter: readonly(counter), increase };
};
