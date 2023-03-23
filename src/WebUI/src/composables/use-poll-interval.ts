interface Subscription {
  id: symbol;
  fn: () => Promise<any> | any;
}

// global state
const subscriptions: Array<Subscription> = [];

const INTERVAL = 1000 * 60 * 1; // 1 min

setInterval(() => {
  subscriptions.forEach(({ fn }) => fn());
}, INTERVAL);

export default () => {
  const subscribe = (subscription: Subscription) => {
    subscriptions.push(subscription);
  };

  const unsubscribe = (id: symbol) => {
    const index = subscriptions.findIndex(el => el.id === id);
    subscriptions.splice(index, 1);
  };

  return {
    subscribe,
    unsubscribe,
  };
};
