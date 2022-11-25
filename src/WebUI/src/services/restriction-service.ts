import { get, post } from '@/services/crpg-client';
import Restriction from '@/models/restriction';
import RestrictionCreation from '@/models/restriction-creation';
import { checkIsDateExpired } from '@/utils/date';

export function mapRestrictions(restrictions: Omit<Restriction, 'expired'>[]): Restriction[] {
  const isAborted = (currentRestricion: Omit<Restriction, 'expired'>): boolean => {
    return restrictions.some(
      restriction =>
        currentRestricion.restrictedUser!.id === restriction.restrictedUser!.id &&
        currentRestricion.id !== restriction.id &&
        currentRestricion.type === restriction.type &&
        new Date(currentRestricion.createdAt).getTime() < new Date(restriction.createdAt).getTime()
    );
  };

  return restrictions.map(restriction => {
    let expired = checkIsDateExpired(restriction.createdAt, restriction.duration);

    if (!expired) {
      expired = isAborted(restriction);
    }

    return {
      ...restriction,
      expired,
    };
  });
}

export async function getRestrictions(): Promise<Restriction[]> {
  const restrictions: Omit<Restriction, 'expired'>[] = await get('/restrictions');
  return mapRestrictions(restrictions);
}

export function restrictUser(payload: RestrictionCreation): Promise<Restriction> {
  return post('/restrictions', payload);
}
