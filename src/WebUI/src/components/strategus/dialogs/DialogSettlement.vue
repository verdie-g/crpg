<script setup lang="ts">
import { PartyStatus } from '@/models/strategus/party';
import { useParty } from '@/composables/strategus/use-party';

const { party, toggleRecruitTroops, isTogglingRecruitTroops } = useParty();

const settlement = computed(() => party.value!.targetedSettlement!);
</script>

<template>
  <DialogBase>
    <div class="space-y-8">
      <div class="prose prose-invert">
        <h2>{{ settlement.name }}</h2>
        <p>Culture: {{ settlement.culture }}</p>
        <p>Garrison: TODO: 2515</p>
      </div>

      <div class="flex gap-4">
        <OButton
          variant="primary"
          size="lg"
          :label="
            party!.status !== PartyStatus.RecruitingInSettlement
              ? 'Start recruiting troops'
              : 'Stop recruiting troops'
          "
          :loading="isTogglingRecruitTroops"
          @click="toggleRecruitTroops"
        />

        <OButton variant="primary" size="lg" label="TODO: Shop" />
      </div>
    </div>
  </DialogBase>
</template>
