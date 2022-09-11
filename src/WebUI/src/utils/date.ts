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

const rtf = new Intl.RelativeTimeFormat('en', {
  numeric: 'auto'
});

export function deltaTimeToRelativeTimeString(ts: number): string {
  const now = new Date()

  const date = new Date(Date.now() - ts);
  const days = date.getDay();
  const daysParts = rtf.formatToParts(days, 'day');
  const daysIntPart = daysParts.find(p => p.type === 'integer');
  const daysUnit = daysIntPart?.unit;
  const daysUnitPart = daysParts.find(
    p => p.type === 'literal' && (!daysUnit || p.value.includes(daysUnit))
  );
  const daysStr = `${daysIntPart?.value} ${daysUnitPart?.value}`;

  if (days === 0) {
    return '< 1 day';
  }

  const months = date.getMonth();
  if (months !== now.getMonth()) {
    const monthsParts = rtf.formatToParts(months, 'month');
    const monthsIntPart = monthsParts.find(p => p.type === 'integer');
    const monthsIntStr = monthsIntPart?.value;
    const monthsUnit = monthsIntPart?.unit;
    const monthsUnitPart = monthsParts.find(
      p => p.type === 'literal' && (!monthsUnit || p.value.includes(monthsUnit))
    );
    const monthsUnitStr = monthsUnitPart?.value;
    const monthsStr = `${monthsIntStr} ${monthsUnitStr}`;

    console.info('daysStr', daysParts, daysStr);
    console.info('monthsParts', monthsParts, monthsStr, months, now.getMonth());
    console.info('dates', date, 'and:', now);
    return `${monthsStr} ${daysStr}`;
  }

  return `${daysStr}`;
}

