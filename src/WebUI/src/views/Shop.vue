<template>
  <section class="section">
    <div class="container">
      <div class="columns is-multiline items" v-if="pageItems">
        <div class="column is-narrow" v-for="item in pageItems" v-bind:key="item.id">
          <div class="card item-card">
            <div class="card-image">
              <figure class="image">
                <img :src="`${publicPath}items/${item.mbId}.png`" alt="item image" />
              </figure>
            </div>
            <div class="card-content content">
              <h4>{{item.name}}</h4>
              <div class="content">
                <item-properties :item="item" />
              </div>
            </div>
            <footer class="card-footer">
              <b-button icon-left="coins" expanded :disabled="item.value > gold || ownedItems[item.id]"
                        :loading="buyingItems[item.id]" @click="buy(item)"
                        :title="buyButtonTitle(item)">
                {{item.value}}
              </b-button>
            </footer>
          </div>
        </div>
      </div>

      <b-pagination :total="allItems.length" :current.sync="currentPage" :per-page="itemsPerPage" order="is-centered" />
    </div>
  </section>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import ItemProperties from '@/components/ItemProperties.vue';
import userModule from '@/store/user-module';
import itemModule from '@/store/item-module';
import Item from '@/models/item';
import { notify } from '@/services/notifications-service';

@Component({
  components: { ItemProperties },
})
export default class Shop extends Vue {
  publicPath = process.env.BASE_URL;

  // items for which buy request was sent
  buyingItems: Record<number, boolean> = {};

  // pagination
  itemsPerPage = 20;
  currentPage = 1;

  // items owned by the user
  get ownedItems(): Record<number, boolean> {
    return userModule.ownedItems.reduce((res: Record<number, boolean>, i: Item) => { res[i.id] = true; return res; }, {});
  }

  get gold(): number {
    return userModule.user == null ? 0 : userModule.user.gold;
  }

  get allItems() {
    return itemModule.items;
  }

  get pageItems() {
    if (!this.allItems) {
      return [];
    }

    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.allItems.slice(startIndex, endIndex);
  }

  created() {
    itemModule.getItems();
    userModule.getOwnedItems();
  }

  async buy(item: Item) {
    Vue.set(this.buyingItems, item.id, true);
    await userModule.buyItem(item);
    Vue.set(this.buyingItems, item.id, false);
    notify(`Bought ${item.name} for ${item.value} gold`);
  }

  buyButtonTitle(item: Item): string {
    if (this.ownedItems[item.id]) {
      return 'You already own this item';
    }

    if (item.value > this.gold) {
      return 'Not enough gold';
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
  display: flex;
  flex-direction: column;
  width: 256px;
  height: 100%;

  .card-content {
    margin-bottom: 0;
  }

  .card-footer {
    margin-top: auto;

    button {
      border: none;
      border-radius: unset;
    }
  }
}

.flag-tags {
  margin-top: 6px;
}
</style>

// not scoped
<style lang="scss">
.weapon-tabs {
  .tabs ul {
    margin-top: 0;
    margin-left: 0;
  }

  .tab-content {
    padding-left: 0;
    padding-right: 0;
  }
}
</style>
