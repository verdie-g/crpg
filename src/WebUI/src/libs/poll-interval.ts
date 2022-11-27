declare module 'vue/types/vue' {
  interface Vue {
    $pollInterval: PollInterval;
  }
}

interface PluginOptions {
  period: number;
}

interface Subscription {
  id: symbol;
  fn: () => Promise<any> | any;
}

class PollInterval {
  private subscriptions: Array<Subscription>;
  private interval: number;

  constructor(period: number) {
    this.subscriptions = [];

    this.interval = setInterval(() => {
      this.subscriptions.forEach(({ fn }) => fn());
    }, period);
  }

  subscribe(subscription: Subscription) {
    this.subscriptions.push(subscription);
  }

  unsubscribe(id: symbol) {
    const index = this.subscriptions.findIndex(el => el.id === id);
    this.subscriptions.splice(index, 1);
  }
}

export default {
  install(Vue: any, options: PluginOptions) {
    Vue.prototype.$pollInterval = new PollInterval(options.period);
  },
};
