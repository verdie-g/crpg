<template>
  <div class="strategus-main">
    <l-map
      ref="map"
      class="map"
      :zoom.sync="zoom"
      :center="center"
      :options="mapOptions"
      :max-bounds="maxBounds"
      @ready="onMapBoundsChange"
      @moveend="onMapBoundsChange"
      @click="onMapClick"
    >
      <l-control-mouse-position />
      <l-control class="column is-one-third" position="topleft">
        <component
          class="box"
          v-if="currentDialog"
          :is="currentDialog"
          v-on="dialogEventHandlers"
        />
      </l-control>
      <l-tile-layer :url="url" :attribution="attribution" />
      <hero v-if="hero" :hero="hero" :self="true" />
      <l-polyline v-if="heroMovementLine !== null" v-bind="heroMovementLine" />
      <l-marker-cluster
        :options="{ removeOutsideVisibleBounds: true, maxClusterRadius: markerClusterRadius }"
      >
        <hero
          v-for="vh in visibleHeroes"
          :key="'hero-' + vh.id"
          :hero="vh"
          :self="false"
          @click="onHeroClick(vh)"
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
import { LMap, LTileLayer, LCircleMarker, LControl, LPolyline } from 'vue2-leaflet';
import Vue2LeafletMarkerCluster from 'vue2-leaflet-markercluster';
import LControlMousePosition from '@/components/strategus/LControlMousePosition.vue';
import { promptMovementType } from '@/components/strategus/MoveDialog.vue';
import Settlement from '@/models/settlement-public';
import SettlementType from '@/models/settlement-type';
import strategusModule from '@/store/strategus-module';
import SettlementComponent from '@/components/strategus/SettlementComponent.vue';
import RegistrationDialog from '@/components/strategus/RegistrationDialog.vue';
import Constants from '../../../../data/constants.json';
import Hero from '@/models/hero';
import HeroComponent from '@/components/strategus/HeroComponent.vue';
import HeroVisible from '@/models/hero-visible';
import HeroStatus from '@/models/hero-status';
import HeroStatusUpdateRequest from '@/models/hero-status-update-request';
import { positionToLatLng } from '@/utils/geometry';
import { Position } from 'geojson';
import MovementType from '@/models/movement-type';

// Register here all dialogs that can be used by the dynamic dialog component.
const dialogs = {
  RegistrationDialog,
};

@Component({
  components: {
    LMap,
    LTileLayer,
    LCircleMarker,
    LControlMousePosition,
    LControl,
    LPolyline,
    'l-marker-cluster': Vue2LeafletMarkerCluster,
    ...dialogs,
    settlement: SettlementComponent,
    hero: HeroComponent,
  },
})
export default class Strategus extends Vue {
  zoom = 3;
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
  };
  maxBounds = new LatLngBounds([
    [0, 0],
    [-Constants.strategusMapHeight, Constants.strategusMapWidth * 3],
  ]);
  mapBounds: LatLngBounds | null = null;
  updateIntervalId = -1;

  // Register here handlers for all events that can emitted from a dialog.
  dialogEventHandlers = {
    heroSpawn: this.heroSpawn,
  };

  get settlements(): Settlement[] {
    if (this.mapBounds === null) {
      return [];
    }

    return strategusModule.settlements.filter(this.shouldDisplaySettlement);
  }

  get hero(): Hero | null {
    return strategusModule.hero;
  }

  // Returns the polyline props if the user is moving, else null.
  get heroMovementLine(): any {
    if (this.hero === null) {
      return null;
    }

    const attackColor = '#f14668';
    const moveColor = '#485fc7';

    let color: string;
    let positions: Position[];
    switch (this.hero.status) {
      case HeroStatus.MovingToPoint:
        positions = this.hero.waypoints.coordinates;
        color = moveColor;
        break;
      case HeroStatus.FollowingHero:
        positions = [this.hero.targetedHero.position.coordinates];
        color = moveColor;
        break;
      case HeroStatus.MovingToSettlement:
        positions = [this.hero.targetedSettlement.position.coordinates];
        color = moveColor;
        break;
      case HeroStatus.MovingToAttackHero:
        positions = [this.hero.targetedHero.position.coordinates];
        color = attackColor;
        break;
      case HeroStatus.MovingToAttackSettlement:
        positions = [this.hero.targetedSettlement.position.coordinates];
        color = attackColor;
        break;
      default:
        return null;
    }

    return {
      latLngs: [this.hero.position.coordinates, ...positions].map(positionToLatLng),
      color,
    };
  }

  get visibleHeroes(): HeroVisible[] {
    return strategusModule.visibleHeroes;
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
        this.heroSpawn();
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

  shouldDisplaySettlement(settlement: Settlement): boolean {
    const [x, y] = settlement.position.coordinates;
    if (!this.mapBounds!.contains(new LatLng(y, x))) {
      return false;
    }

    return (
      this.zoom > 4 ||
      (this.zoom > 3 && settlement.type == SettlementType.Castle) ||
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

  heroSpawn() {
    strategusModule.getUpdate();
    this.updateIntervalId = setInterval(() => strategusModule.getUpdate(), 60 * 1000);
    this.map.mapObject.flyTo(positionToLatLng(this.hero!.position.coordinates), 5, {
      duration: 0.4,
    });
  }

  onMapClick(event: LeafletMouseEvent) {
    const clickCoordinates = [event.latlng.lng, event.latlng.lat];
    let coordinates =
      event.originalEvent.shiftKey &&
      this.hero !== null &&
      this.hero.status === HeroStatus.MovingToPoint
        ? [...this.hero.waypoints.coordinates, clickCoordinates]
        : [clickCoordinates];

    this.moveHero({
      status: HeroStatus.MovingToPoint,
      waypoints: { type: 'MultiPoint', coordinates },
    });
  }

  async onHeroClick(hero: Hero) {
    const movement = await promptMovementType(
      this.$refs.map as Vue,
      positionToLatLng(hero.position.coordinates),
      [MovementType.Follow, MovementType.Attack]
    );

    if (movement === null) {
      return;
    }

    this.moveHero({
      status:
        movement === MovementType.Follow ? HeroStatus.FollowingHero : HeroStatus.MovingToAttackHero,
      targetedHeroId: hero.id,
    });
  }

  async onSettlementClick(settlement: Settlement) {
    const movement = await promptMovementType(
      this.$refs.map as Vue,
      positionToLatLng(settlement.position.coordinates),
      [MovementType.Move, MovementType.Attack]
    );

    if (movement === null) {
      return;
    }

    this.moveHero({
      status:
        movement === MovementType.Move
          ? HeroStatus.MovingToSettlement
          : HeroStatus.MovingToAttackSettlement,
      targetedSettlementId: settlement.id,
    });
  }

  moveHero(updateRequest: Partial<HeroStatusUpdateRequest>) {
    if (this.hero === null) {
      return;
    }

    strategusModule.updateHeroStatus({
      status: HeroStatus.MovingToPoint,
      waypoints: { type: 'MultiPoint', coordinates: [] },
      targetedHeroId: 0,
      targetedSettlementId: 0,
      ...updateRequest,
    });
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

<style scoped lang="scss">
.strategus-main {
  .map {
    // calc(Screen height - navbar)
    height: calc(100vh - 4.25rem);
    background-color: #284745;
  }
}
</style>
