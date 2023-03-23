interface Subscription {
  id: symbol;
  fn: () => Promise<any> | any;
}

// global state
const subscriptions: Array<Subscription> = [];

setInterval(() => {
  subscriptions.forEach(({ fn }) => fn());
}, 1000 * 60 * 2);

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
