export function timestampToTimeString(ts: number): string {
  const date = new Date(ts);
  const year = date.getFullYear() - 1970;
  const month = date.getMonth();
  const day = date.getDate() - 1;
  const hours = date.getHours() - 1;
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
