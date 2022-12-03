import { get, post } from '@/services/crpg-client';
import Restriction, { RestrictionWithActive } from '@/models/restriction';
import RestrictionCreation from '@/models/restriction-creation';
import { checkIsDateExpired } from '@/utils/date';

export function mapRestrictions(restrictions: Restriction[]): RestrictionWithActive[] {
  const checkIsRestrictionActive = (currentRestricion: Restriction): boolean => {
    const { id, type, createdAt, restrictedUser } = currentRestricion;
    return !restrictions.some(
      r =>
        restrictedUser!.id === r.restrictedUser!.id && // groupBy user - there may be restrisctions for other users on the list (/admin page)
        type === r.type &&
        id !== r.id &&
        new Date(createdAt).getTime() < new Date(r.createdAt).getTime() // check whether the the current restriction is NOT the newest
    );
  };

  return restrictions.map(r => {
    return {
      ...r,
      active: !checkIsDateExpired(r.createdAt, r.duration) && checkIsRestrictionActive(r),
    };
  });
}

export async function getRestrictions(): Promise<RestrictionWithActive[]> {
  const restrictions: Restriction[] = await get('/restrictions');
  return mapRestrictions(restrictions);
}

export function restrictUser(payload: RestrictionCreation): Promise<Restriction> {
  return post('/restrictions', payload);
}
