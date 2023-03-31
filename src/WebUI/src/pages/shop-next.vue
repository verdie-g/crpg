<script setup lang="ts">
import { getItems } from '@/services/item-service';
import { useUserStore } from '@/stores/user';

import { create, search, insertMultiple, stemmers } from '@orama/orama';

definePage({
  meta: {
    layout: 'default',
    showInNav: true,
    sortInNav: 90,
    noStickyHeader: true,
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const userStore = useUserStore();

const { state: items, execute: loadItems } = useAsyncState(() => getItems(), [], {
  immediate: false,
});

const stats = {
  price: {
    min: 0,
    max: 0,
  },
};

const db = await create({
  schema: {
    name: 'string',
    foo: 'string',
    price: 'number',
  },
  components: {
    tokenizer: {
      language: 'english',
      stemmer: stemmers.english,
    },
    afterInsert: (_orama, _id, doc) => {
      for (const key in doc) {
        if (key in stats) {
          stats[key].min = Math.min(stats[key].min, doc[key]);
          stats[key].max = Math.max(stats[key].max, doc[key]);
        }
      }
    },
  },
});

await Promise.all([loadItems(), userStore.fetchUserItems()]);

const docs = [
  {
    name: 'The prestige',
    foo: 'bar',
    price: 500,
    tags: ['tag1', 'tag2'],
  },
  {
    name: 'Big Fish',
    foo: 'bar',
    price: 1000,
    tags: ['tag1'],
  },
];

await insertMultiple(db, docs, 1200);

const results = await search(db, {
  term: 'bar',
  properties: ['foo'],
  facets: {
    name: {
      sort: 'DESC',
    },
    tags: {
      sort: 'DESC',
    },
    price: {
      ranges: [
        {
          from: stats.price.min,
          to: stats.price.max,
        },
      ],
    },
  },
});

console.log(results);
</script>

<template>
  <div class="relative space-y-2 px-6 pt-6 pb-6">
    <div>__hi__</div>
    <!-- {{ items }} -->
  </div>
</template>
