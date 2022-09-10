<template>
  <div class="strategus-main">
    <div class="strategus-html-layer columns p-6">
      <div class="column is-one-third box strategus-dialog" v-if="currentDialog">
        <component :is="currentDialog" v-on="dialogEventHandlers" />
      </div>
    </div>

    <l-map
      ref="map"
      class="map"
      :zoom="3"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @ready="onMapBoundsChange"
      @moveend="onMapBoundsChange"
      @click="onMapClick"
    >
      <l-control-zoom position="bottomright" />
      <locate-party-control position="bottomright" />
      <l-control-mouse-position />
      <l-tile-layer :url="url" :attribution="attribution" />
      <party v-if="party" :party="party" :self="true" />
      <l-polyline v-if="partyMovementLine !== null" v-bind="partyMovementLine" />
      <l-marker-cluster
        :options="{ removeOutsideVisibleBounds: true, maxClusterRadius: markerClusterRadius }"
      >
        <party
          v-for="vh in visibleParties"
          :key="'party-' + vh.id"
          :party="vh"
          :self="false"
          @click="onPartyClick(vh)"
        />
        <settlement
          v-for="settlement in settlements"
          :key="'settlement-' + settlement.id"
          :settlement="settlement"
          @click="onSettlementClick(settlement)"
        />
      </l-marker-cluster>
    </l-map>
  </div>
</template>

<script lang="ts">
import { Vue, Component } from 'vue-property-decorator';
import { LatLng, LatLngBounds, CRS, LeafletMouseEvent } from 'leaflet';
import { LMap, LControlZoom, LTileLayer, LCircleMarker, LPolyline } from 'vue2-leaflet';
import Vue2LeafletMarkerCluster from 'vue2-leaflet-markercluster';
import LControlMousePosition from '@/components/strategus/LControlMousePosition.vue';
import LocatePartyControl from '@/components/strategus/LocatePartyControl.vue';
import { promptMovementType } from '@/components/strategus/MoveDialog.vue';
import Settlement from '@/models/settlement-public';
import SettlementType from '@/models/settlement-type';
import strategusModule from '@/store/strategus-module';
import * as strategusService from '@/services/strategus-service';
import SettlementComponent from '@/components/strategus/SettlementComponent.vue';
import RegistrationDialog from '@/components/strategus/RegistrationDialog.vue';
import SettlementDialog from '@/components/strategus/SettlementDialog.vue';
import Constants from '../../../../data/constants.json';
import Party from '@/models/party';
import PartyComponent from '@/components/strategus/PartyComponent.vue';
import PartyVisible from '@/models/party-visible';
import PartyStatus from '@/models/party-status';
import PartyStatusUpdateRequest from '@/models/party-status-update-request';
import { positionToLatLng } from '@/utils/geometry';
import { Position } from 'geojson';
import MovementType from '@/models/movement-type';

// Register here all dialogs that can be used by the dynamic dialog component.
const dialogs = {
  RegistrationDialog,
  SettlementDialog,
};

@Component({
  components: {
    LMap,
    LControlZoom,
    LTileLayer,
    LCircleMarker,
    LControlMousePosition,
    LocatePartyControl,
    LPolyline,
    'l-marker-cluster': Vue2LeafletMarkerCluster,
    ...dialogs,
    settlement: SettlementComponent,
    party: PartyComponent,
  },
})
export default class Strategus extends Vue {
  center = new LatLng(-100, 125);
  url = 'http://pecores.fr/gigamap/{z}/{y}/{x}.png';
  attribution = '<a target="_blank" href="https://www.taleworlds.com">TaleWorlds Entertainment</a>';
  mapOptions = {
    zoomSnap: 0.5,
    minZoom: 3,
    maxZoom: 7,
    crs: CRS.Simple,
    maxBoundsViscosity: 0.8,
    inertiaDeceleration: 2000,
    zoomControl: false,
  };
  maxBounds = new LatLngBounds([
    [0, 0],
    [-Constants.strategusMapHeight, Constants.strategusMapWidth * 3],
  ]);
  mapBounds: LatLngBounds | null = null;
  updateIntervalId = -1;

  // Register here handlers for all events that can emitted from a dialog.
  dialogEventHandlers = {
    partySpawn: this.partySpawn,
  };

  get settlements(): Settlement[] {
    if (this.mapBounds === null) {
      return [];
    }

    const zoom = this.map.mapObject.getZoom();
    return strategusModule.settlements.filter(s =>
      this.shouldDisplaySettlement(s, this.mapBounds!, zoom)
    );
  }

  get party(): Party | null {
    return strategusModule.party;
  }

  // Returns the polyline props if the user is moving, else null.
  get partyMovementLine(): any {
    if (this.party === null) {
      return null;
    }

    const attackColor = '#f14668';
    const moveColor = '#485fc7';

    let color: string;
    let positions: Position[];
    switch (this.party.status) {
      case PartyStatus.MovingToPoint:
        positions = this.party.waypoints.coordinates;
        color = moveColor;
        break;
      case PartyStatus.FollowingParty:
        positions = [this.party.targetedParty.position.coordinates];
        color = moveColor;
        break;
      case PartyStatus.MovingToSettlement:
        positions = [this.party.targetedSettlement.position.coordinates];
        color = moveColor;
        break;
      case PartyStatus.MovingToAttackParty:
        positions = [this.party.targetedParty.position.coordinates];
        color = attackColor;
        break;
      case PartyStatus.MovingToAttackSettlement:
        positions = [this.party.targetedSettlement.position.coordinates];
        color = attackColor;
        break;
      default:
        return null;
    }

    return {
      latLngs: [this.party.position.coordinates, ...positions].map(positionToLatLng),
      color,
    };
  }

  get visibleParties(): PartyVisible[] {
    return strategusModule.visibleParties;
  }

  get map(): LMap {
    return this.$refs.map as LMap;
  }

  get currentDialog(): string | null {
    return strategusModule.currentDialog;
  }

  created() {
    strategusModule.getSettlements();
    strategusModule.getUpdate().then(res => {
      if (res.errors !== null) {
        // Not registered to strategus.
        strategusModule.pushDialog('RegistrationDialog');
      } else {
        this.partySpawn();
        if (strategusService.inSettlementStatuses.has(this.party!.status)) {
          strategusModule.pushDialog('SettlementDialog');
        }
      }
    });
  }

  beforeDestroy() {
    if (this.updateIntervalId !== -1) {
      clearInterval(this.updateIntervalId);
    }
  }

  onMapBoundsChange() {
    this.mapBounds = this.map.mapObject.getBounds();
  }

  shouldDisplaySettlement(settlement: Settlement, mapBounds: LatLngBounds, zoom: number): boolean {
    const [x, y] = settlement.position.coordinates;
    if (!mapBounds!.contains(new LatLng(y, x))) {
      return false;
    }

    return (
      zoom > 4 ||
      (zoom > 3 && settlement.type == SettlementType.Castle) ||
      settlement.type === SettlementType.Town
    );
  }

  // Returns the maximum radius that a marker cluster will cover in pixels.
  // Since we don't want this radius to depend on the zoom, distance in latlng
  // should be converted to pixels.
  markerClusterRadius(): number {
    const maximumRadius = 1;
    const a = new LatLng(0, 0);
    const b = new LatLng(0, maximumRadius);
    const map = this.map.mapObject;
    return map.latLngToLayerPoint(a).distanceTo(map.latLngToLayerPoint(b));
  }

  partySpawn() {
    strategusModule.getUpdate();
    this.updateIntervalId = setInterval(() => strategusModule.getUpdate(), 60 * 1000);
    this.map.mapObject.flyTo(positionToLatLng(this.party!.position.coordinates), 5, {
      animate: false,
    });
  }

  onMapClick(event: LeafletMouseEvent) {
    const clickCoordinates = [event.latlng.lng, event.latlng.lat];
    let coordinates =
      event.originalEvent.shiftKey &&
      this.party !== null &&
      this.party.status === PartyStatus.MovingToPoint
        ? [...this.party.waypoints.coordinates, clickCoordinates]
        : [clickCoordinates];

    this.moveParty({
      status: PartyStatus.MovingToPoint,
      waypoints: { type: 'MultiPoint', coordinates },
    });
  }

  async onPartyClick(party: Party) {
    const movement = await promptMovementType(
      this.$refs.map as Vue,
      positionToLatLng(party.position.coordinates),
      [MovementType.Follow, MovementType.Attack]
    );

    if (movement === null) {
      return;
    }

    this.moveParty({
      status:
        movement === MovementType.Follow
          ? PartyStatus.FollowingParty
          : PartyStatus.MovingToAttackParty,
      targetedPartyId: party.id,
    });
  }

  async onSettlementClick(settlement: Settlement) {
    if (this.party === null) {
      return;
    }

    if (
      strategusService.inSettlementStatuses.has(this.party.status) &&
      this.party.targetedSettlement.id === settlement.id
    ) {
      strategusModule.pushDialog('SettlementDialog');
      return;
    }

    const movement = await promptMovementType(
      this.$refs.map as Vue,
      positionToLatLng(settlement.position.coordinates),
      [MovementType.Move, MovementType.Attack]
    );

    if (movement === null) {
      return;
    }

    this.moveParty({
      status:
        movement === MovementType.Move
          ? PartyStatus.MovingToSettlement
          : PartyStatus.MovingToAttackSettlement,
      targetedSettlementId: settlement.id,
    });
  }

  moveParty(updateRequest: Partial<PartyStatusUpdateRequest>) {
    if (this.party === null) {
      return;
    }

    strategusModule
      .updatePartyStatus({
        status: PartyStatus.MovingToPoint,
        waypoints: { type: 'MultiPoint', coordinates: [] },
        targetedPartyId: 0,
        targetedSettlementId: 0,
        ...updateRequest,
      })
      .then(() => strategusModule.popDialog());
  }
}
</script>

<style lang="scss">
@import '~leaflet.markercluster/dist/MarkerCluster.css';
@import '~leaflet.markercluster/dist/MarkerCluster.Default.css';

// Hide vertical scrollbar
html {
  overflow-y: auto;
}
</style>

<style lang="scss">
.leaflet-right .leaflet-control {
  margin-bottom: 3px; // Default is 10px and it's too much.
}

.leaflet-container .leaflet-control-attribution {
  margin-bottom: 0px;
}
</style>

<style scoped lang="scss">
.strategus-main {
  position: relative;

  .strategus-html-layer {
    position: absolute;
    width: 100%;
  }

  .strategus-dialog {
    z-index: 500; // To be over the map.
  }

  .map {
    // calc(Screen height - navbar)
    height: calc(100vh - 4.25rem);
    background-color: #284745;
  }
}
</style>
