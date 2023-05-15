<script setup lang="ts">
import { registerUser } from '@/services/strategus-service';
import { useUserStore } from '@/stores/user';

const { user } = toRefs(useUserStore());

const emit = defineEmits<{
  registered: [];
}>();

const start = async () => {
  await registerUser(user.value!.region);
  emit('registered');
};
</script>

<template>
  <DialogBase>
    <div class="prose prose-invert">
      <p>TODO: i18n</p>

      <h2>Welcome to Strategus</h2>

      <Divider />

      <p>
        Strategus is a multiplayer campaign for cRPG where players can aquire fiefs and land and
        gather armies on a real-time map. Strategus expands the world of cRPG to a browser-based map
        of Calradia, where players take their armies into battle against other players or serve as a
        mercenary in scheduled battles on the cRPG servers.
      </p>

      <p>
        You will join the campaign in region
        <strong>{{ $t(`region.${user!.region}`) }}.</strong>
      </p>

      <OButton nativeType="submit" variant="primary" size="xl" :label="'Start'" @click="start" />

      <Divider />

      <p>If this doesn't seem right you can try re-login or contact an administrator</p>
    </div>
  </DialogBase>
</template>
