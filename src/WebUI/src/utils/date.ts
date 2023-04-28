import { HumanDuration } from '@/models/datetime';

export const parseTimestamp = (ts: number): HumanDuration => {
  const days = Math.floor(ts / 86400000);
  const hours = Math.floor((ts % 86400000) / 3600000);
  const minutes = Math.floor(((ts % 86400000) % 3600000) / 60000);

  return {
    days,
    hours,
    minutes,
  };
};

const daysToMs = (days: number) => days * 24 * 60 * 60 * 1000;

const hoursToMs = (hours: number) => hours * 60 * 60 * 1000;

const minutesToMs = (minutes: number) => minutes * 60 * 1000;

export const msToHours = (ms: number) => Math.floor(ms / 60 / 60 / 1000);

export const convertHumanDurationToMs = (duration: HumanDuration) => {
  return daysToMs(duration.days) + hoursToMs(duration.hours) + minutesToMs(duration.minutes);
};

/**
 * @param {number} duration - ms
 */
export const checkIsDateExpired = (createdAt: Date, duration: number) => {
  return new Date().getTime() > new Date(createdAt).getTime() + duration;
};

/**
 * @param {number} duration - ms
 */
export const computeLeftMs = (createdAt: Date, duration: number) => {
  const result = new Date(createdAt).getTime() + duration - new Date().getTime();
  return result < 0 ? 0 : result;
};

export const isBetween = (date: Date, start: Date, end: Date) =>
  date.valueOf() >= start.valueOf() && date.valueOf() <= end.valueOf();
