<template>
  <div>
    <div v-for="prop in itemProperties.common">
      {{prop[0]}}: {{prop[1]}}
    </div>
    <b-tabs v-if="itemProperties.primary" class="weapon-tabs">
      <b-tab-item label="Primary">
        <div v-for="prop in itemProperties.primary">
          {{prop[0]}}: {{prop[1]}}<br />
        </div>
        <b-taglist class="flag-tags">
          <b-tag v-for="flag in itemProperties.primaryFlags" type="is-info">{{flag}}</b-tag>
        </b-taglist>
      </b-tab-item>

      <b-tab-item label="Secondary" v-if="itemProperties.secondary">
        <div v-for="prop in itemProperties.secondary">
          {{prop[0]}}: {{prop[1]}}<br />
        </div>
        <b-taglist class="flag-tags">
          <b-tag v-for="flag in itemProperties.secondaryFlags" type="is-info">{{flag}}</b-tag>
        </b-taglist>
      </b-tab-item>
    </b-tabs>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { getItemProperties } from '@/services/item-service';
import Item from '@/models/item';

@Component
export default class ItemProperties extends Vue {
  @Prop(Object) readonly item: Item;

  get itemProperties() {
    return getItemProperties(this.item);
  }
}
</script>

<style scoped lang="scss">
</style>
