<script setup lang="ts">
import { getUserRestrictions } from '@/services/users-service';

definePage({
  props: true,
  meta: {
    layout: 'default',
    roles: ['Moderator', 'Admin'],
  },
});

const props = defineProps<{ id: string }>();

const { state: restrictions, execute: loadRestrictions } = useAsyncState(
  () => getUserRestrictions(Number(props.id)),
  [],
  {
    immediate: false,
  }
);

await loadRestrictions();
</script>

<template>
  <div>
    <div class="mb-8 flex items-center gap-4">
      <h2 class="text-lg">{{ $t('restriction.user.history') }}</h2>

      <Modal closable :autoHide="false">
        <OButton
          native-type="submit"
          variant="primary"
          size="sm"
          :label="$t('restriction.create.form.title')"
        />
        <template #popper="{ hide }">
          <div class="space-y-6 p-6">
            <div class="pb-4 text-center text-xl text-content-100">
              {{ $t('restriction.create.form.title') }}
            </div>
            <CreateRestrictionForm
              :userId="Number(props.id)"
              @restrictionCreated="
                () => {
                  hide();
                  loadRestrictions();
                }
              "
            />
          </div>
        </template>
      </Modal>
    </div>

    <RestrictionsTable :restrictions="restrictions" :hiddenCols="['restrictedUser']" />
  </div>
</template>
