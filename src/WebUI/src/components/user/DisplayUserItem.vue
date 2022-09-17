<template>
  <div>
    <figure class="image mb-2">
      <img :src="userItemImage(userItem)" alt="item image" />
    </figure>
    <h4 :class="userItemRankClass(userItem)">{{ userItem.baseItem.name }}</h4>
    <item-properties :item="userItem.baseItem" :rank="userItem.rank" />
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import ItemProperties from '@/components/ItemProperties.vue';
import CharacterStatsComponent from '@/components/CharacterStatsComponent.vue';
import UserItem from '@/models/user-item';

@Component({
  components: { CharacterStatsComponent, ItemProperties },
})
export default class DisplayUserItem extends Vue {
  @Prop(Object) readonly userItem: UserItem;

  userItemImage(userItem: UserItem): string {
    return `${process.env.BASE_URL}items/${userItem.baseItem.id}.png`;
  }

  userItemRankClass(userItem: UserItem | null): string {
    return userItem === null ? '' : `item-rank${userItem.rank}`;
  }
}
</script>

<style scoped lang="scss"></style>
