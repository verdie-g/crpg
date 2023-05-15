<script setup lang="ts">
import { type Map, LeafletMouseEvent } from 'leaflet';
import { LMap, LTileLayer, LControlZoom, LImageOverlay } from '@vue-leaflet/vue-leaflet';
// @ts-ignore TODO:
import { LMarkerClusterGroup } from 'vue-leaflet-markercluster';
import { MovementType, MovementTargetType } from '@/models/strategus';
import { type PartyVisible, PartyStatus } from '@/models/strategus/party';
import { type SettlementPublic } from '@/models/strategus/settlement';
import { mainHeaderHeightKey } from '@/symbols/common';
import { inSettlementStatuses } from '@/services/strategus-service';
import { positionToLatLng } from '@/utils/geometry';
import { useMap } from '@/composables/strategus/use-map';
import { useParty } from '@/composables/strategus/use-party';
import { useSettlements } from '@/composables/strategus/use-settlements';
import { useMove } from '@/composables/strategus/use-move';

definePage({
  meta: {
    layout: 'default',
    roles: ['User', 'Moderator', 'Admin'],
    noFooter: true,
  },
});

const mainHeaderHeight = injectStrict(mainHeaderHeightKey);

// prettier-ignore
const {
  map,
  mapOptions,
  center,
  mapBounds,
  maxBounds,
  zoom,
  tileLayerOptions,
  onMapMoveEnd
} = useMap();

// prettier-ignore
const {
  isRegistered,
  party,
  startUpdatePartyInterval,
  moveParty,
  visibleParties,
  updateParty
} = useParty();

// prettier-ignore
const {
  visibleSettlements,
  loadSettlements
} = useSettlements(mapBounds, zoom);

// prettier-ignore
const {
  moveTarget,
  moveTargetType,

  moveDialogCoordinates,
  moveDialogMovementTypes,

  onMoveDialogShown,
  onMoveDialogCancel
} =
  useMove();

const partySpawn = async () => {
  await updateParty();

  if (map.value === null || party.value === null) return;

  (map.value.leafletObject as Map).flyTo(positionToLatLng(party.value.position.coordinates), 5, {
    animate: false,
  });

  startUpdatePartyInterval();
};

const onMapClick = (event: LeafletMouseEvent) => {
  const clickCoordinates = [event.latlng.lng, event.latlng.lat];

  const coordinates =
    event.originalEvent.shiftKey &&
    party.value !== null &&
    party.value.status === PartyStatus.MovingToPoint
      ? [...party.value.waypoints.coordinates, clickCoordinates]
      : [clickCoordinates];

  return moveParty({
    status: PartyStatus.MovingToPoint,
    waypoints: { type: 'MultiPoint', coordinates },
  });
};

const onPartyCLick = (_event: LeafletMouseEvent, targetParty: PartyVisible) => {
  if (party.value === null) return;

  onMoveDialogShown({
    target: targetParty,
    targetType: MovementTargetType.Party,
    movementTypes: [MovementType.Follow, MovementType.Attack],
  });
};

const onSettlementClick = (settlement: SettlementPublic) => {
  if (party.value === null) return;

  onMoveDialogShown({
    target: settlement,
    targetType: MovementTargetType.Settlement,
    movementTypes: [MovementType.Move, MovementType.Attack],
  });
};

const onMoveDialogConfirm = (mt: MovementType) => {
  if (moveTarget.value !== null) {
    switch (moveTargetType.value) {
      case MovementTargetType.Party:
        moveParty({
          status:
            mt === MovementType.Follow
              ? PartyStatus.FollowingParty
              : PartyStatus.MovingToAttackParty,
          targetedPartyId: moveTarget.value.id,
        });
        break;

      case MovementTargetType.Settlement:
        moveParty({
          status:
            mt === MovementType.Move
              ? PartyStatus.MovingToSettlement
              : PartyStatus.MovingToAttackSettlement,
          targetedSettlementId: moveTarget.value.id,
        });
        break;
    }
  }

  onMoveDialogCancel();
};

const onMapReady = async (map: Map) => {
  mapBounds.value = map.getBounds();
  await Promise.all([loadSettlements(), partySpawn()]);
};

const onRegistered = () => {
  isRegistered.value = true;
  partySpawn();
};
</script>

<template>
  <div :style="{ height: `calc(100vh - ${mainHeaderHeight}px)` }">
    <LMap
      v-model:zoom="zoom"
      ref="map"
      :center="center"
      :options="mapOptions"
      :maxBounds="maxBounds"
      @ready="onMapReady"
      @click="onMapClick"
      @moveEnd="onMapMoveEnd"
    >
      <!-- TODO: FIXME: low res map image -->
      <!-- TODO: FIXME: zIndex -->
      <!-- <LImageOverlay
        url="https://www.printablee.com/postpic/2011/06/blank-100-square-grid-paper_405041.jpg"
        :bounds="maxBounds"
      /> -->
      <LTileLayer v-bind="tileLayerOptions" />
      <LControlZoom position="bottomright" />
      <ControlMousePosition />
      <ControlLocateParty v-if="party !== null" :party="party" position="bottomright" />
      <MarkerParty v-if="party !== null" :party="party" isSelf />
      <PartyMovementLine v-if="party !== null" :party="party" />

      <LMarkerClusterGroup :showCoverageOnHover="false" chunkedLoading>
        <MarkerParty
          v-for="visibleParty in visibleParties"
          :party="visibleParty"
          :key="'party-' + visibleParty.id"
          @click="onPartyCLick($event, visibleParty)"
        />
      </LMarkerClusterGroup>

      <MarkerSettlement
        v-for="settlement in visibleSettlements"
        :settlement="settlement"
        :key="'settlement-' + settlement.id"
        @click="onSettlementClick(settlement)"
      />

      <DialogMove
        v-if="moveDialogCoordinates !== null"
        :latLng="moveDialogCoordinates"
        :movementTypes="moveDialogMovementTypes"
        @confirm="onMoveDialogConfirm"
        @cancel="onMoveDialogCancel"
      />
    </LMap>

    <!-- Dialogs -->
    <div class="absolute left-6 top-6 z-[1000]">
      <DialogRegistration v-if="!isRegistered" @registered="onRegistered" />

      <DialogSettlement
        v-if="
          party !== null &&
          party.targetedSettlement !== null &&
          inSettlementStatuses.has(party.status)
        "
      />
    </div>
  </div>
</template>

<style>
@import 'leaflet/dist/leaflet.css';
@import 'vue-leaflet-markercluster/dist/style.css';

.leaflet-right .leaflet-control {
  @apply mb-2;
}

.leaflet-container .leaflet-control-attribution {
  @apply mb-0;
}

/* TODO: colors */
.marker-cluster-small {
  @apply bg-primary;
}

.marker-cluster-small div {
  @apply bg-primary-hover text-content-100;
}

.marker-cluster-medium {
  @apply bg-primary;
}

.marker-cluster-medium div {
  @apply bg-primary-hover text-content-100;
}

.marker-cluster-large {
  @apply bg-primary;
}

.marker-cluster-large div {
  @apply bg-primary-hover text-content-100;
}
</style>
