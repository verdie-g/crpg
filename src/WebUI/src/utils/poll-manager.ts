import { createNanoEvents, Emitter } from 'nanoevents';

interface Events {
  tick: () => void;
}

export default class PollManager {
  private static instance: PollManager; // Singleton pattern
  private emitter: Emitter;
  private interval? = 1000 * 60 * 2;

  private constructor(interval?: number) {
    this.emitter = createNanoEvents<Events>();

    if (interval !== undefined) {
      this.interval = interval;
    }
  }

  public static getInstance(interval?: number): PollManager {
    if (!PollManager.instance) {
      PollManager.instance = new PollManager(interval);
    }

    return PollManager.instance;
  }

  public start(): void {
    const poll = () => {
      setTimeout(() => {
        this.emitter.emit('tick');
        poll();
      }, this.interval);
    };

    poll();
  }

  public on<E extends keyof Events>(event: E, callback: Events[E]) {
    return this.emitter.on(event, callback);
  }
}
