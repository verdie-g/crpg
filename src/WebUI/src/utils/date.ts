// TODO: unit
export function timestampToTimeString(ts: number): string {
  const days = Math.floor(ts / 86400000);
  const hours = Math.floor((ts % 86400000) / 3600000);
  const minutes = Math.floor(((ts % 86400000) % 3600000) / 60000);
  const seconds = Math.floor((((ts % 86400000) % 3600000) % 60000) / 1000);

  let timeStr = '';

  if (days !== 0) {
    timeStr += `${days}d `;
  }

  if (hours !== 0) {
    timeStr += `${hours} h `;
  }

  if (minutes !== 0) {
    timeStr += `${minutes} min `;
  }

  if (seconds !== 0) {
    timeStr += `${seconds} s `;
  }

  if (timeStr.length > 1) {
    timeStr = timeStr.slice(0, -1); // remove extra space
  }

  return timeStr;
}

function daysToMs(days: number) {
  return days * 24 * 60 * 60 * 1000;
}

function hoursToMs(hours: number) {
  return hours * 60 * 60 * 1000;
}

function minutesToMs(minutes: number) {
  return minutes * 60 * 1000;
}

export interface HumanDuration {
  days: number;
  hours: number;
  minutes: number;
}

export function convertHumanDurationToMs(duration: HumanDuration): number {
  return daysToMs(duration.days) + hoursToMs(duration.hours) + minutesToMs(duration.minutes);
}

export function checkIsDateExpired(createdAt: Date, duration: number): boolean {
  return new Date().getTime() > new Date(createdAt).getTime() + duration;
}

export function computeLeftMs(createdAt: Date, duration: number): number {
  const result = new Date(createdAt).getTime() + duration - new Date().getTime();
  return result < 0 ? 0 : result;
}
