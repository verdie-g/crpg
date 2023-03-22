<script setup lang="ts">
import { type RouteLocationNormalized } from 'vue-router/auto';
import { type CharacterCharacteristics, type CharacterOverallItemsStats } from '@/models/character';
import { useUserStore } from '@/stores/user';
import {
  characterKey,
  characterCharacteristicsKey,
  characterHealthPointsKey,
  characterItemsKey,
  characterItemsStatsKey,
} from '@/symbols/character';
import {
  getCharacterCharacteristics,
  createDefaultCharacteristic,
  getCharacterItems,
  computeHealthPoints,
  computeOverallArmor,
  computeOverallWeight,
  computeOverallPrice,
  computeLongestWeaponLength,
  computeAverageRepairCostByHour,
} from '@/services/characters-service';

definePage({
  props: true,
  meta: {
    layout: 'default',
    middleware: 'characterValidate',
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const props = defineProps<{ id: string }>();
const userStore = useUserStore();
const character = computed(() => userStore.characters.find(c => c.id === Number(props.id))!);

const { state: characterItems, execute: loadCharacterItems } = useAsyncState(
  ({ id }: { id: number }) => getCharacterItems(id),
  [],
  {
    immediate: false,
    resetOnExecute: false,
  }
);

const { state: characterCharacteristics, execute: loadCharacterCharacteristics } = useAsyncState(
  ({ id }: { id: number }) => getCharacterCharacteristics(id),
  createDefaultCharacteristic(),
  {
    immediate: false,
    resetOnExecute: false,
  }
);

const setCharacterCharacteristics = (characteristic: CharacterCharacteristics) => {
  characterCharacteristics.value = characteristic;
};

const healthPoints = computed(() =>
  computeHealthPoints(
    characterCharacteristics.value.skills.ironFlesh,
    characterCharacteristics.value.attributes.strength
  )
);

const itemsStats = computed((): CharacterOverallItemsStats => {
  const items = characterItems.value.map(ei => ei.userItem.baseItem);
  return {
    averageRepairCostByHour: computeAverageRepairCostByHour(items),
    weight: computeOverallWeight(items),
    price: computeOverallPrice(items),
    longestWeaponLength: computeLongestWeaponLength(items),
    ...computeOverallArmor(items),
  };
});

provide(characterKey, character);
provide(characterCharacteristicsKey, {
  characterCharacteristics: readonly(characterCharacteristics),
  setCharacterCharacteristics,
});
provide(characterHealthPointsKey, healthPoints);
provide(characterItemsKey, {
  characterItems: readonly(characterItems),
  loadCharacterItems,
});
provide(characterItemsStatsKey, itemsStats);

const fetchPageData = async (characterId: number) =>
  Promise.all([
    loadCharacterCharacteristics(0, { id: characterId }),
    loadCharacterItems(0, { id: characterId }),
  ]);

onBeforeRouteUpdate(async (to, from) => {
  if (to.name === from.name) {
    // if character changed
    await fetchPageData(
      Number((to as RouteLocationNormalized<'CharactersId'>).params.id as string)
    );
  }

  return true;
});

await fetchPageData(character.value.id);
</script>

<template>
  <div>
    <div class="mb-16 flex items-center justify-center gap-2 place-self-center">
      <RouterLink :to="{ name: 'CharactersId', params: { id } }" v-slot="{ isExactActive }">
        <OButton
          :variant="isExactActive ? 'transparent-active' : 'transparent'"
          size="lg"
          :label="$t('character.nav.overview')"
        />
      </RouterLink>

      <RouterLink :to="{ name: 'CharactersIdInventory', params: { id } }" v-slot="{ isActive }">
        <OButton
          :variant="isActive ? 'transparent-active' : 'transparent'"
          size="lg"
          :label="$t('character.nav.inventory')"
        />
      </RouterLink>

      <RouterLink
        :to="{ name: 'CharactersIdCharacteristic', params: { id } }"
        v-slot="{ isActive }"
      >
        <OButton
          :variant="isActive ? 'transparent-active' : 'transparent'"
          size="lg"
          :label="$t('character.nav.characteristic')"
        />
      </RouterLink>
    </div>

    <RouterView />
  </div>
</template>
