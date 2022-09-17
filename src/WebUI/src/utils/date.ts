export function timestampToTimeString(ts: number): string {
  const date = new Date(ts);
  const year = date.getFullYear() - 1970;
  const month = date.getMonth();
  const day = date.getDate() - 1;
  const hours = date.getHours() - 1;
  const minutes = date.getMinutes();

  const timeParts: string[] = [];

  if (year !== 0) {
    timeParts.push(`${year} year${year > 1 ? 's' : ''}`);
  }

  if (month !== 0) {
    timeParts.push(`${month} month${month > 1 ? 's' : ''}`);
  }

  if (day !== 0) {
    timeParts.push(`${day} day${day > 1 ? 's' : ''}`);
  }

  if (hours !== 0) {
    timeParts.push(`${hours} hour${hours > 1 ? 's' : ''}`);
  }

  if (minutes !== 0) {
    timeParts.push(`${minutes} minute${minutes > 1 ? 's' : ''}`);
  }

  return timeParts.join(' ');
}
