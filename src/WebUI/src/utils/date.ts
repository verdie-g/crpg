// TODO: unit
export function timestampToTimeString(ts: number): string {
  const date = new Date(ts);
  const year = date.getFullYear() - 1970;
  const month = date.getMonth();
  const day = date.getUTCDate() - 1;
  const hours = date.getUTCHours();
  const minutes = date.getMinutes();

  let timeStr = '';
  if (year !== 0) {
    timeStr += `${year} year${year > 1 ? 's' : ''} `;
  }

  if (month !== 0) {
    timeStr += `${month} month${month > 1 ? 's' : ''} `;
  }

  if (day !== 0) {
    timeStr += `${day} day${day > 1 ? 's' : ''} `;
  }

  if (hours !== 0) {
    timeStr += `${hours} hour${hours > 1 ? 's' : ''} `;
  }

  if (minutes !== 0) {
    timeStr += `${minutes} minute${minutes > 1 ? 's' : ''} `;
  }

  if (timeStr.length > 1) {
    timeStr = timeStr.slice(0, -1); // remove extra space
  }

  return timeStr;
}

function daysToMs(days = 0) {
  return days * 24 * 60 * 60 * 1000;
}

function hoursToMs(hours = 0) {
  return hours * 60 * 60 * 1000;
}

function minutesToMs(minutes = 0) {
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
