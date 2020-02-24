<template>
  <section class="section">
    <div class="container">
      <div class="columns is-multiline items" v-if="items">
        <div class="column is-narrow" v-for="item in items" v-bind:key="item.id">
          <div class="card item-card">
            <div class="card-image">
              <figure class="image">
                <img :src="item.image" alt="item image" />
              </figure>
            </div>
            <div class="card-content content">
              <h4>{{item.name}}</h4>
              <b-button icon-left="coins" expanded :disabled="item.value > golds || ownedItems[item.id]"
                        :loading="buyingItems[item.id]" @click="buy(item)"
                        :title="buyButtonTitle(item)">
                {{item.value}}
              </b-button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';
import itemModule from '@/store/item-module';
import Item from '@/models/item';
import { notify } from '@/services/notifications-service';

@Component
export default class Shop extends Vue {
  // items for which buy request was sent
  buyingItems: Record<number, boolean> = {};

  // items owned by the user
  get ownedItems(): Record<number, boolean> {
    return userModule.ownedItems.reduce((res: Record<number, boolean>, i: Item) => { res[i.id] = true; return res; }, {});
  }

  get golds(): number {
    return userModule.user == null ? 0 : userModule.user.golds;
  }

  get items() {
    return itemModule.items;
  }

  created() {
    itemModule.getItems();
    userModule.getOwnedItems();
  }

  async buy(item: Item) {
    Vue.set(this.buyingItems, item.id, true);
    await userModule.buyItem(item);
    Vue.set(this.buyingItems, item.id, false);
    notify(`Bought ${item.name} for ${item.value} golds`);
  }

  buyButtonTitle(item: Item): string {
    if (this.ownedItems[item.id]) {
      return 'You already own this item';
    }

    if (item.value > this.golds) {
      return 'Not enough golds';
    }

    return '';
  }
}
</script>

<style scoped lang="scss">
.items {
  justify-content: center;
}

.item-card {
  width: 256px;
}
</style>
