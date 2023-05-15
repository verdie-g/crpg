<script setup lang="ts">
import { LPolyline } from '@vue-leaflet/vue-leaflet';
import { type Position } from 'geojson';
import { type Party, PartyStatus } from '@/models/strategus/party';

import { positionToLatLng } from '@/utils/geometry';

const { party } = defineProps<{ party: Party }>();

const attackColor = '#f14668';
const moveColor = '#485fc7';

const partyMovementLine = computed(() => {
  let color: string;
  const positions: Position[] = [];

  switch (party.status) {
    case PartyStatus.MovingToPoint:
      positions.push(...party.waypoints.coordinates);
      color = moveColor;
      break;
    case PartyStatus.FollowingParty:
      positions.push(party.targetedParty!.position.coordinates);
      color = moveColor;
      break;
    case PartyStatus.MovingToSettlement:
      positions.push(party.targetedSettlement!.position.coordinates);
      color = moveColor;
      break;
    case PartyStatus.MovingToAttackParty:
      positions.push(party.targetedParty!.position.coordinates);
      color = attackColor;
      break;
    case PartyStatus.MovingToAttackSettlement:
      positions.push(party.targetedSettlement!.position.coordinates);
      color = attackColor;
      break;
    default:
      return null;
  }

  return {
    latLngs: [party.position.coordinates, ...positions].map(positionToLatLng),
    color,
  };
});
</script>

<template>
  <LPolyline v-if="partyMovementLine !== null" v-bind="partyMovementLine" />
</template>
