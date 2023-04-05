<script setup lang="ts">
import { useUserStore } from '@/stores/user';
import {
  updateCharacter,
  activateCharacter,
  characterClassToIcon,
} from '@/services/characters-service';
import { notify } from '@/services/notification-service';
import { t } from '@/services/translate-service';

definePage({
  meta: {
    layout: 'default',
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const userStore = useUserStore();
const { characters, user } = toRefs(userStore);

const route = useRoute('CharactersId');

const currentCharacterId = computed(() =>
  route.params.id !== undefined ? Number(route.params.id) : undefined
);

const currentCharacter = computed(() =>
  characters.value.find(char => char.id === currentCharacterId.value)
);
const currentCharacterIsActive = computed(
  () => user.value?.activeCharacterId === currentCharacterId.value
);

const onUpdateCharacter = async ({ name }: { name: string }) => {
  if (currentCharacter.value === undefined) return;

  if (name !== currentCharacter.value.name) {
    userStore.replaceCharacter(await updateCharacter(currentCharacter.value!.id, { name }));
  }

  notify(t('character.settings.update.notify.success'));
};

const onActivateCharacter = async (id: number) => {
  await activateCharacter(id, true);
  await userStore.fetchUser();

  notify(t('character.settings.update.notify.success'));
};

const shownCreateCharacterGuideModal = ref<boolean>(false);
const onCreateNewCharacter = async () => {
  if (user.value?.activeCharacterId !== null && user.value?.activeCharacterId !== undefined) {
    await activateCharacter(user.value.activeCharacterId, false);
    await userStore.fetchUser();
  }

  shownCreateCharacterGuideModal.value = true;
};

if (userStore.characters.length === 0) {
  await userStore.fetchCharacters();
}
</script>

<template>
  <div class="container relative py-6">
    <div
      v-if="currentCharacter !== undefined"
      id="character-top-navbar"
      class="mb-16 grid grid-cols-3 items-center justify-between gap-4"
    >
      <div class="order-1 flex items-center gap-4">
        <VDropdown :triggers="['click']" placement="bottom-end">
          <template #default="{ shown }">
            <OButton
              variant="primary"
              outlined
              :label="`${currentCharacter.name} (${currentCharacter.level})`"
              size="lg"
            >
              <OIcon
                :icon="characterClassToIcon[currentCharacter.class]"
                size="lg"
                v-tooltip="$t(`character.class.${currentCharacter.class}`)"
              />

              <div class="flex items-center gap-1">
                <div class="max-w-[150px] overflow-x-hidden text-ellipsis whitespace-nowrap">
                  {{ currentCharacter.name }}
                </div>

                <div>({{ currentCharacter.level }})</div>
              </div>

              <Tag
                v-if="currentCharacter.id === user?.activeCharacterId"
                :label="$t('character.status.active.short')"
                v-tooltip="$t('character.status.active.title')"
                variant="success"
                size="sm"
              />

              <Tag
                v-if="currentCharacter.forTournament"
                :label="$t('character.status.forTournament.short')"
                v-tooltip="$t('character.status.forTournament.title')"
                variant="warning"
                size="sm"
              />

              <div class="h-4 w-px select-none bg-border-300"></div>

              <OIcon
                icon="chevron-down"
                size="lg"
                :rotation="shown ? 180 : 0"
                class="text-content-400"
              />
            </OButton>
          </template>

          <template #popper="{ hide }">
            <DropdownItem
              v-for="char in characters"
              :checked="char.id === currentCharacterId"
              tag="RouterLink"
              :to="{ name: route.name, params: { id: char.id } }"
              @click="hide"
            >
              <CharacterSelectItem
                :character="char"
                :isCharacterActive="user!.activeCharacterId === char.id"
                @activate="onActivateCharacter(char.id)"
              />
            </DropdownItem>

            <DropdownItem
              class="text-primary hover:text-primary-hover"
              @click="
                () => {
                  onCreateNewCharacter();
                  hide();
                }
              "
            >
              <OIcon icon="add" size="lg" />
              {{ $t('character.create.title') }}
            </DropdownItem>
          </template>
        </VDropdown>

        <Modal>
          <OButton
            size="xl"
            iconRight="edit"
            rounded
            variant="secondary"
            outlined
            v-tooltip="$t('character.settings.update.title')"
          />
          <template #popper="{ hide }">
            <CharacterEditForm
              :character="currentCharacter"
              :active="currentCharacterIsActive"
              @cancel="hide"
              @confirm="
                data => {
                  onUpdateCharacter(data);
                  hide();
                }
              "
            />
          </template>
        </Modal>
      </div>
    </div>

    <RouterView v-slot="{ Component }">
      <Suspense>
        <component :is="Component" />
        <template #fallback>
          <OLoading fullPage active iconSize="xl" />
        </template>
      </Suspense>
    </RouterView>

    <CharacterCreateModal
      :shown="shownCreateCharacterGuideModal"
      @apply-hide="shownCreateCharacterGuideModal = false"
    />
  </div>
</template>
