import { parse, stringify } from 'qs';
import { type RouterScrollBehavior, type LocationQuery } from 'vue-router/auto';

const numberCandidate = (candidate: string) => /^[+-]?\d+(\.\d+)?$/.test(candidate);

const tryParseFloat = (str: string) => (numberCandidate(str) ? parseFloat(str) : str);

// ref: https://github.com/ljharb/qs/blob/main/lib/utils.js#L111
const decoder = (str: string): string | number | boolean | null | undefined => {
  const candidateToNumber = tryParseFloat(str);

  if (typeof candidateToNumber === 'number' && !isNaN(candidateToNumber)) {
    return candidateToNumber;
  }

  const keywords: Record<string, any> = {
    true: true,
    false: false,
    null: null,
    undefined: undefined,
  };

  if (str in keywords) {
    return keywords[str];
  }

  const strWithoutPlus = str.replace(/\+/g, ' ');

  try {
    return decodeURIComponent(strWithoutPlus);
  } catch (_e) {
    return strWithoutPlus;
  }
};

export const parseQuery = (query: string) =>
  parse(query, {
    ignoreQueryPrefix: true,
    strictNullHandling: true,
    decoder,
  }) as LocationQuery;

export const stringifyQuery = (query: Record<string, any>) =>
  stringify(query, {
    strictNullHandling: true,
    arrayFormat: 'brackets',
    skipNulls: true,
  });

export const scrollBehavior: RouterScrollBehavior = (to, _from, savedPosition) => {
  if (savedPosition) {
    return savedPosition;
  }

  // check if any matched route config has meta that requires scrolling to top
  if (to.matched.some(m => m.meta.scrollToTop)) {
    return { top: 0, left: 0, behavior: 'smooth' };
  }
};
