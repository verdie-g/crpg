import { type LocationQuery, type RouteLocationNormalizedLoaded } from 'vue-router/auto';
import { parseQuery, stringifyQuery, scrollBehavior } from './router';

it.each<[string, LocationQuery]>([
  [
    'search=Lion%20Imprinted%20Saber',
    {
      search: 'Lion Imprinted Saber',
    },
  ],
  [
    'type=Bow&filter[flags][]=bow&filter[flags][]=long_bow',
    {
      filter: {
        flags: ['bow', 'long_bow'],
      },
      type: 'Bow',
    },
  ],
  [
    'type=Bolts&filter[damageType][]=Cut&filter[stackWeight][]=0.2&filter[stackWeight][]=2&search=Bolts&hideOwnedItems=true',
    {
      filter: {
        damageType: ['Cut'],
        stackWeight: [0.2, 2], // custom decoder
      },
      search: 'Bolts',
      type: 'Bolts',
      hideOwnedItems: true, // custom decoder
    },
  ],
])('parseQuery - q: %s', (query, expectation) => {
  expect(parseQuery(query)).toEqual(expectation);
});

it.each<[LocationQuery, string]>([
  [
    {
      search: 'Lion Imprinted Saber',
    },
    'search=Lion%20Imprinted%20Saber',
  ],
])('stringifyQuery - q: %s', (query, expectation) => {
  expect(stringifyQuery(query)).toEqual(expectation);
});

describe('scrollBehavior', () => {
  it('savedPosition', () => {
    expect(
      scrollBehavior({} as RouteLocationNormalizedLoaded, {} as RouteLocationNormalizedLoaded, {
        top: 20,
        left: 0,
      })
    ).toEqual({ top: 20, left: 0 });
  });

  it('scrollToTop', () => {
    expect(
      scrollBehavior(
        {
          matched: [
            {
              meta: {
                scrollToTop: true,
              },
            },
          ],
        } as RouteLocationNormalizedLoaded,
        {} as RouteLocationNormalizedLoaded,
        null
      )
    ).toEqual({ top: 0, left: 0, behavior: 'smooth' });
  });
});
