import { DateTime } from 'luxon'; // TODO: someday try to do without this library ;)
import { isBetween } from '@/utils/date';
import { Region } from '@/models/region';

interface HHScheduleTime {
  hours: number;
  minutes: number;
}
interface HHScheduleConfig {
  start: HHScheduleTime;
  end: HHScheduleTime;
  tz: string;
}

export const getHHScheduleConfig = () => {
  return import.meta.env.VITE_HH.split(',').reduce((out, cur) => {
    // TODO: try/catch
    const [region, start, end, tz] = cur.split('|') as [Region, string, string, string];

    const [startHours, startMinutes] = start.split(':');
    const [endHours, endMinutes] = end.split(':');

    out[region] = {
      start: {
        hours: Number(startHours),
        minutes: Number(startMinutes),
      },
      end: {
        hours: Number(endHours),
        minutes: Number(endMinutes),
      },
      tz,
    };

    return out;
  }, {} as Record<Region, HHScheduleConfig>);
};

interface HHEvent {
  start: Date;
  end: Date;
}

export const getHHEventByRegion = (region: Region): HHEvent => {
  const cfg = getHHScheduleConfig()[region];

  const startDt = DateTime.fromObject(
    { hour: cfg.start.hours, minute: cfg.start.minutes, second: 0 },
    { zone: cfg.tz }
  );

  const endDt = DateTime.fromObject(
    { hour: cfg.end.hours, minute: cfg.end.minutes, second: 0 },
    { zone: cfg.tz }
  );

  return {
    start: startDt.setZone(DateTime.local().zoneName).toJSDate(),
    end: endDt.setZone(DateTime.local().zoneName).toJSDate(),
  };
};

export const getHHEventRemaining = (event: HHEvent) => {
  if (!isBetween(new Date(), event.start, event.end)) return 0;
  return event.end.getTime() - new Date().getTime();
};
