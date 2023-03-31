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

const db = await create({
  schema: {
    name: 'string',
    foo: 'string',
    price: 'number',
    // tags: 'string',
  },
  components: {
    tokenizer: {
      language: 'english',
      stemmer: stemmers.english,
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
  {
    name: 'Harry Potter and the Philosopher',
    foo: 'bar',
    price: 1500,
    tags: ['tag3'],
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
          from: 0,
          to: 100000,
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
