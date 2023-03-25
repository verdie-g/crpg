<script setup lang="ts">
import { type RouteLocationNormalized } from 'vue-router/auto';
import {
  experienceMultiplierByGeneration,
  maxExperienceMultiplierForGeneration,
  minimumRetirementLevel,
  tournamentLevel,
  maximumLevel,
  freeRespecializeIntervalDays,
} from '@root/data/constants.json';
import { useUserStore } from '@/stores/user';
import { characterKey, characterCharacteristicsKey } from '@/symbols/character';
import { parseTimestamp } from '@/utils/date';
import { notify } from '@/services/notification-service';
import { t } from '@/services/translate-service';
import {
  getExperienceForLevel,
  getCharacterStatistics,
  getCharacterLimitations,
  deleteCharacter,
  activateCharacter,
  respecializeCharacter,
  canRetireValidate,
  retireCharacter,
  canSetCharacterForTournamentValidate,
  setCharacterForTournament,
  getHeirloomPointByLevel,
  getExperienceMultiplierBonus,
  getCharacterKDARatio,
  getRespecCapability,
} from '@/services/characters-service';
import usePollInterval from '@/composables/use-poll-interval';

definePage({
  meta: {
    layout: 'default',
    roles: ['User', 'Moderator', 'Admin'],
  },
});

const router = useRouter();
const userStore = useUserStore();

const character = injectStrict(characterKey);
const { loadCharacterCharacteristics } = injectStrict(characterCharacteristicsKey);

const currentLevelExperience = computed(() => getExperienceForLevel(character.value.level));
const nextLevelExperience = computed(() => getExperienceForLevel(character.value.level + 1));

const onDeleteCharacter = async () => {
  if (character.value.id === userStore.user!.activeCharacterId) {
    await activateCharacter(character.value.id, false);
    await userStore.fetchUser();
  }

  await deleteCharacter(character.value.id);
  notify(t('character.settings.delete.notify.success'));

  const characters = userStore.characters.filter(char => char.id !== character.value.id);

  if (characters.length === 0) {
    await router.replace({ name: 'Root' });
  } else {
    await router.replace({
      name: 'CharactersId',
      params: { id: userStore.user!.activeCharacterId || characters[0].id },
    });
  }

  userStore.fetchCharacters();
};

const respecCapability = computed(() =>
  getRespecCapability(character.value, characterLimitations.value, userStore.user!.gold)
);

const onRespecializeCharacter = async () => {
  userStore.replaceCharacter(await respecializeCharacter(character.value.id));
  userStore.subtractGold(respecCapability.value.price);
  await Promise.all([
    loadCharacterLimitations(0, { id: character.value.id }),
    loadCharacterCharacteristics(0, { id: character.value.id }),
  ]);
  notify(t('character.settings.respecialize.notify.success'));
};

const canRetire = computed(() => canRetireValidate(character.value.level));
const onRetireCharacter = async () => {
  userStore.replaceCharacter(await retireCharacter(character.value.id));
  await userStore.fetchUser();
  notify(t('character.settings.retire.notify.success'));
};

const canSetCharacterForTournament = computed(() =>
  canSetCharacterForTournamentValidate(character.value)
);

const onSetCharacterForTournament = async () => {
  userStore.replaceCharacter(await setCharacterForTournament(character.value.id));
  notify(t('character.settings.tournament.notify.success'));
};

const { state: characterStatistics, execute: loadCharacterStatistics } = useAsyncState(
  ({ id }: { id: number }) => getCharacterStatistics(id),
  { kills: 0, deaths: 0, assists: 0, playTime: 0 },
  {
    immediate: false,
    resetOnExecute: false,
  }
);

const { state: characterLimitations, execute: loadCharacterLimitations } = useAsyncState(
  ({ id }: { id: number }) => getCharacterLimitations(id),
  { lastFreeRespecializeAt: new Date() },
  {
    immediate: false,
    resetOnExecute: false,
  }
);

const kdaRatio = computed(() =>
  characterStatistics.value.deaths === 0 ? '∞' : getCharacterKDARatio(characterStatistics.value)
);

const experienceMultiplierBonus = computed(() =>
  getExperienceMultiplierBonus(userStore.user!.experienceMultiplier)
);

const heirloomPointByLevel = computed(() => getHeirloomPointByLevel(character.value.level));

const { subscribe, unsubscribe } = usePollInterval();
const id = Symbol('loadCharacterStatistics');
onMounted(() => {
  subscribe({ id, fn: userStore.fetchCharacters });
});
onBeforeUnmount(() => {
  unsubscribe(id);
});

const fetchPageData = async (characterId: number) =>
  Promise.all([
    loadCharacterStatistics(0, { id: characterId }),
    loadCharacterLimitations(0, { id: characterId }),
  ]);

onBeforeRouteUpdate(async to => {
  await fetchPageData(Number((to as RouteLocationNormalized<'CharactersId'>).params.id as string));
  return true;
});

await fetchPageData(character.value.id);
</script>

<template>
  <div class="mx-auto max-w-2xl space-y-12 pb-12">
    <FormGroup :label="$t('character.settings.group.overview.title')" :collapsable="false">
      <div class="grid grid-cols-2 gap-2 text-2xs">
        <SimpleTableRow
          :label="$t('character.statistics.level.title')"
          :tooltip="
            character.forTournament
              ? {
                  title: $t('character.statistics.level.lockedTooltip.title', {
                    maxLevel: maximumLevel,
                  }),
                }
              : {
                  title: $t('character.statistics.level.tooltip.title', { maxLevel: maximumLevel }),
                }
          "
        >
          <div
            class="flex gap-1.5"
            :class="[character.forTournament ? 'text-status-warning' : 'text-content-100']"
          >
            {{ character.level }}
            <OIcon v-if="character.forTournament" icon="lock" size="sm" />
          </div>
        </SimpleTableRow>

        <template v-if="!character.forTournament">
          <SimpleTableRow
            :label="$t('character.statistics.generation.title')"
            :value="String(character.generation)"
            :tooltip="{
              title: $t('character.statistics.generation.tooltip.title'),
            }"
          />

          <SimpleTableRow
            :label="$t('character.statistics.expMultiplier.title')"
            :value="$t('character.format.expMultiplier', { multiplier: $n(userStore.user!.experienceMultiplier) })"
            :tooltip="{
              title: $t('character.statistics.expMultiplier.tooltip.title', {
                maxExpMulti: $t('character.format.expMultiplier', {
                  multiplier: $n(maxExperienceMultiplierForGeneration),
                }),
              }),
              description: $t('character.statistics.expMultiplier.tooltip.desc'),
            }"
          />

          <SimpleTableRow
            :label="$t('character.statistics.kda.title')"
            :value="$t('character.format.kda', {
                kills: characterStatistics!.kills,
                deaths: characterStatistics!.deaths,
                assists: characterStatistics!.assists,
                ratio: kdaRatio,
              })"
            :tooltip="{
              title: $t('character.statistics.kda.tooltip.title'),
            }"
          />

          <SimpleTableRow
            :label="$t('character.statistics.playTime.title')"
            :value="$t('dateTimeFormat.dd:hh:mm', {
                ...parseTimestamp(characterStatistics!.playTime),
              })"
          />

          <div class="col-span-2 mt-12 px-4 py-2.5">
            <VTooltip placement="bottom">
              <VueSlider
                :key="currentLevelExperience"
                class="!cursor-default !opacity-100"
                :modelValue="character.experience"
                disabled
                tooltip="always"
                :min="currentLevelExperience"
                :max="nextLevelExperience"
                :tooltipFormatter="$n"
                :marks="[currentLevelExperience, nextLevelExperience]"
              >
                <template #mark="{ pos, value, label }">
                  <div
                    class="absolute top-2.5 whitespace-nowrap"
                    :class="{
                      '-translate-x-full transform': value === nextLevelExperience,
                    }"
                    :style="{ left: `${pos}%` }"
                  >
                    {{ $n(label) }}
                  </div>
                </template>
              </VueSlider>
              <template #popper>
                <div class="prose prose-invert">
                  <h3>{{ $t('character.statistics.experience.tooltip.title') }}</h3>
                  <i18n-t
                    scope="global"
                    keypath="character.statistics.experience.tooltip.desc"
                    class="text-content-200"
                    tag="p"
                  >
                    <template #remainExpToUp>
                      <span class="text-content-100">
                        {{ $n(nextLevelExperience - character.experience) }}
                      </span>
                    </template>
                  </i18n-t>
                </div>
              </template>
            </VTooltip>
          </div>
        </template>
      </div>
    </FormGroup>

    <FormGroup
      class="mb-16"
      icon="settings"
      :label="$t('character.settings.group.actions.title')"
      :collapsable="false"
    >
      <div class="grid grid-cols-3 gap-4">
        <!--  -->
        <Modal :disabled="!respecCapability.enabled">
          <VTooltip placement="auto">
            <div>
              <OButton
                variant="secondary"
                size="xl"
                :disabled="!respecCapability.enabled"
                expanded
                iconLeft="chevron-down-double"
                data-aq-character-action="respecialize"
              >
                <div class="flex items-center gap-2">
                  <span class="max-w-[100px] overflow-x-hidden text-ellipsis whitespace-nowrap">
                    {{ $t('character.settings.respecialize.title') }}
                  </span>

                  <Tag
                    v-if="respecCapability.price === 0"
                    variant="success"
                    size="sm"
                    label="free"
                  />
                  <SvgSpriteImg v-else name="coin" viewBox="0 0 18 18" class="w-4" />
                </div>
              </OButton>
            </div>

            <template #popper>
              <div class="prose prose-invert">
                <h5 class="text-content-100">
                  {{ $t('character.settings.respecialize.tooltip.title') }}
                </h5>
                <i18n-t
                  scope="global"
                  keypath="character.settings.respecialize.tooltip.desc"
                  tag="p"
                >
                  <template #respecializationPrice>
                    <Coin :value="respecCapability.price" />
                  </template>
                </i18n-t>

                <i18n-t
                  v-if="!character.forTournament"
                  scope="global"
                  keypath="character.settings.respecialize.tooltip.free"
                  tag="p"
                >
                  <template #days>
                    <span class="font-bold text-content-100">
                      {{ freeRespecializeIntervalDays }}
                    </span>
                  </template>
                </i18n-t>

                <i18n-t
                  v-if="respecCapability.price !== 0"
                  scope="global"
                  keypath="character.settings.respecialize.tooltip.nextFreeAt"
                  tag="p"
                >
                  <template #nextFreeAt>
                    <span class="font-bold text-content-100">
                      {{
                        $t('dateTimeFormat.dd:hh:mm', {
                          ...respecCapability.nextFreeAt,
                        })
                      }}
                    </span>
                  </template>
                </i18n-t>
              </div>
            </template>
          </VTooltip>

          <template #popper="{ hide }">
            <ConfirmActionForm
              :title="$t('character.settings.respecialize.dialog.title')"
              :name="character.name"
              :confirmLabel="$t('action.apply')"
              @cancel="hide"
              @confirm="
                () => {
                  onRespecializeCharacter();
                  hide();
                }
              "
            >
              <template #description>
                <i18n-t
                  scope="global"
                  keypath="character.settings.respecialize.dialog.desc"
                  tag="p"
                >
                  <template #respecializationPrice>
                    <Coin :value="respecCapability.price" class="text-status-danger" />
                  </template>
                </i18n-t>
              </template>
            </ConfirmActionForm>
          </template>
        </Modal>

        <template v-if="!character.forTournament">
          <!--  -->
          <Modal>
            <VTooltip placement="auto">
              <div>
                <OButton
                  variant="primary"
                  outlined
                  :disabled="!canRetire"
                  size="xl"
                  expanded
                  iconLeft="child"
                  data-aq-character-action="retire"
                  :label="$t('character.settings.retire.title')"
                />
              </div>

              <template #popper>
                <div class="prose prose-invert">
                  <h5 class="text-content-100">
                    {{ $t('character.settings.retire.tooltip.title') }}
                  </h5>
                  <i18n-t scope="global" keypath="character.settings.retire.tooltip.desc" tag="p">
                    <template #resetLevel>
                      <span class="font-bold text-status-danger">1</span>
                    </template>
                    <template #multiplierBonus>
                      <span class="font-bold text-status-success">
                        +{{
                          $n(experienceMultiplierByGeneration, 'percent', {
                            minimumFractionDigits: 0,
                          })
                        }}
                      </span>
                    </template>
                    <template #maxMultiplierBonus>
                      <span class="text-content-100">
                        +{{
                          $n(maxExperienceMultiplierForGeneration - 1, 'percent', {
                            minimumFractionDigits: 0,
                          })
                        }}
                      </span>
                    </template>
                    <template #heirloom>
                      <OIcon icon="blacksmith" size="sm" class="align-middle text-primary" />
                    </template>
                  </i18n-t>
                  <i18n-t
                    v-if="!canRetire"
                    scope="global"
                    keypath="character.settings.retire.tooltip.requiredDesc"
                    class="text-status-danger"
                    tag="p"
                  >
                    {{ $t('character.settings.retire.tooltip.required') }}
                    <template #requiredLevel>
                      <span class="font-bold">{{ minimumRetirementLevel }}+</span>
                    </template>
                  </i18n-t>
                </div>
              </template>
            </VTooltip>

            <template #popper="{ hide }">
              <ConfirmActionForm
                :title="$t('character.settings.retire.dialog.title')"
                :name="character.name"
                :confirmLabel="$t('action.apply')"
                @cancel="hide"
                @confirm="
                  () => {
                    onRetireCharacter();
                    hide();
                  }
                "
              >
                <template #description>
                  <p>
                    {{ $t('character.settings.retire.dialog.desc') }}
                  </p>
                  <i18n-t scope="global" keypath="character.settings.retire.dialog.reward" tag="p">
                    <template #heirloom>
                      <span class="inline-flex items-center text-sm font-bold text-primary">
                        +{{ heirloomPointByLevel }}
                        <OIcon icon="blacksmith" size="sm" />
                      </span>
                    </template>
                    <template #multiplierBonus>
                      <span class="text-sm font-bold text-status-success">
                        +{{
                          $n(experienceMultiplierBonus, 'percent', {
                            minimumFractionDigits: 0,
                          })
                        }}
                      </span>
                    </template>
                    <template #resetLevel>
                      <span class="text-sm font-bold text-status-danger">1</span>
                    </template>
                  </i18n-t>
                </template>
              </ConfirmActionForm>
            </template>
          </Modal>

          <!--  -->
          <Modal :disabled="!canSetCharacterForTournament">
            <VTooltip placement="auto">
              <div>
                <OButton
                  variant="secondary"
                  size="xl"
                  expanded
                  iconLeft="member"
                  :disabled="!canSetCharacterForTournament"
                  data-aq-character-action="forTournament"
                  :label="$t('character.settings.tournament.title')"
                />
              </div>
              <template #popper>
                <div class="prose prose-invert">
                  <h5 class="text-content-100">
                    {{ $t('character.settings.tournament.tooltip.title') }}
                  </h5>

                  <i18n-t
                    scope="global"
                    keypath="character.settings.tournament.tooltip.desc"
                    tag="p"
                  >
                    <template #tournamentLevel>
                      <span class="text-sm font-bold text-content-100">
                        {{ tournamentLevel }}
                      </span>
                    </template>
                  </i18n-t>

                  <i18n-t
                    v-if="!canSetCharacterForTournament"
                    scope="global"
                    keypath="character.settings.tournament.tooltip.requiredDesc"
                    class="text-status-danger"
                    tag="p"
                  >
                    <template #requiredLevel>
                      <span class="text-xs font-bold">{{ `<${tournamentLevel}` }}</span>
                    </template>
                  </i18n-t>
                </div>
              </template>
            </VTooltip>

            <template #popper="{ hide }">
              <ConfirmActionForm
                :title="$t('character.settings.tournament.dialog.title')"
                :description="$t('character.settings.tournament.dialog.desc')"
                :name="character.name"
                :confirmLabel="$t('action.apply')"
                @cancel="hide"
                @confirm="
                  () => {
                    onSetCharacterForTournament();
                    hide();
                  }
                "
              />
            </template>
          </Modal>
        </template>
      </div>
    </FormGroup>

    <i18n-t scope="global" keypath="character.settings.delete.title" tag="div" class="text-center">
      <template #link>
        <Modal>
          <span class="cursor-pointer text-status-danger hover:text-opacity-80">
            {{ $t('character.settings.delete.link') }}
          </span>

          <template #popper="{ hide }">
            <ConfirmActionForm
              :title="$t('character.settings.delete.dialog.title')"
              :description="$t('character.settings.delete.dialog.desc')"
              :name="character.name"
              :confirmLabel="$t('action.delete')"
              @cancel="hide"
              @confirm="
                () => {
                  onDeleteCharacter();
                  hide();
                }
              "
            />
          </template>
        </Modal>
      </template>
    </i18n-t>
  </div>
</template>
