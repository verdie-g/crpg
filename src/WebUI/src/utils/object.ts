import { type Entries } from 'type-fest';

export const mergeObjectWithSum = (obj1: Record<string, number>, obj2: Record<string, number>) =>
  Object.keys(obj1).reduce(
    (obj, key) => {
      obj[key] += obj1[key];
      return obj;
    },
    { ...obj2 }
  );

// TODO: unit
const pickFunc = <T extends {}, K extends keyof T>(
  obj: T,
  predicate: (...args: any[]) => boolean
): Pick<T, K> =>
  Object.keys(obj)
    .filter(predicate)
    .reduce(
      (filteredObj: Pick<T, K>, key) => ({
        ...filteredObj,
        [key]: obj[key as keyof T],
      }),
      {} as Pick<T, K>
    );

// filters an object based on an array of keys
export const pick = <T extends {}, K extends keyof T>(obj: T, keys: Array<K>): Pick<T, K> =>
  pickFunc(obj, k => keys.includes(k as K));

export const omit = <T extends {}, K extends keyof T>(obj: T, keys: Array<K>): Pick<T, K> =>
  pickFunc(obj, k => !keys.includes(k as K));

export const omitPredicate = <T extends {}, K extends keyof T>(
  obj: T,
  predicate: (...args: any[]) => boolean
): Pick<T, K> => pickFunc(obj, predicate);

export const getEntries = <T extends object>(obj: T) => Object.entries(obj) as Entries<T>;
