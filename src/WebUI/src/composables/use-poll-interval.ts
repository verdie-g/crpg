type SubscriptionFn = () => Promise<any> | any;

const INTERVAL = 1000 * 60 * 2; // 2 min

// global state
const subscriptions = new Map<symbol, SubscriptionFn>();

setInterval(() => {
  for (let fn of subscriptions.values()) {
    fn();
  }
}, INTERVAL);

export default () => {
  const subscribe = (id: symbol, fn: SubscriptionFn) => {
    subscriptions.set(id, fn);
  };

  const unsubscribe = (id: symbol) => {
    subscriptions.delete(id);
  };

  return {
    subscribe,
    unsubscribe,
  };
};
