import { useCounter } from './counter';

it('increase counter on call', () => {
  const { counter, increase } = useCounter();

  expect(counter.value).toBe(0);

  increase();

  expect(counter.value).toBe(1);
});
