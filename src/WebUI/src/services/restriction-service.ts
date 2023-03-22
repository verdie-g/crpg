import { get, post } from '@/services/crpg-client';
import {
  type Restriction,
  type RestrictionWithActive,
  type RestrictionCreation,
  RestrictionType,
} from '@/models/restriction';
import { checkIsDateExpired } from '@/utils/date';

const checkIsRestrictionActive = (
  restrictions: Restriction[],
  { id, type, createdAt, restrictedUser }: Restriction
): boolean => {
  return !restrictions.some(
    r =>
      restrictedUser!.id === r.restrictedUser!.id && // groupBy user - there may be restrisctions for other users on the list (/admin page)
      type === r.type &&
      id !== r.id &&
      new Date(createdAt).getTime() < new Date(r.createdAt).getTime() // check whether the the current restriction is NOT the newest
  );
};

export const mapRestrictions = (restrictions: Restriction[]): RestrictionWithActive[] => {
  return restrictions.map(r => ({
    ...r,
    active:
      !checkIsDateExpired(r.createdAt, r.duration) && checkIsRestrictionActive(restrictions, r),
  }));
};

export const getRestrictions = async () =>
  mapRestrictions(await get<Restriction[]>('/restrictions'));

export const restrictUser = async (payload: RestrictionCreation) =>
  post<Restriction>('/restrictions', payload);

export const getActiveJoinRestriction = (restrictions: RestrictionWithActive[]) =>
  restrictions.find(r => r.type === RestrictionType.Join && r.active === true) || null;
