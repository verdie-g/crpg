<template>
  <section class="section">
    <div class="container">
      <div class="columns">
        <aside class="column is-narrow shop-filters">
          <shop-filter-form v-model="filters" />
        </aside>

        <div class="column">
          <div class="columns is-multiline items" v-if="pageItems">
            <div
              class="column is-narrow"
              v-for="{ item, weaponIdx } in pageItems"
              v-bind:key="item.id"
            >
              <div class="card item-card">
                <div class="card-image">
                  <figure class="image">
                    <img
                      :src="`${publicPath}items/${item.templateMbId}.png`"
                      alt="item image"
                      loading="lazy"
                    />
                  </figure>
                </div>
                <div class="card-content content">
                  <h4>{{ item.name }}</h4>
                  <div class="content">
                    <item-properties :item="item" :weapon-idx="weaponIdx" />
                  </div>
                </div>
                <footer class="card-footer">
                  <b-button
                    icon-left="coins"
                    expanded
                    :disabled="item.value > gold || ownedItems[item.id]"
                    :loading="buyingItems[item.id]"
                    @click="buy(item)"
                    :title="buyButtonTitle(item)"
                  >
                    {{ item.value }}
                  </b-button>
                </footer>
              </div>
            </div>
          </div>

          <b-pagination
            :total="filteredItems.length"
            :current.sync="currentPage"
            :per-page="itemsPerPage"
            order="is-centered"
            range-before="2"
            range-after="2"
            icon-prev="chevron-left"
          >
            <b-pagination-button
              slot-scope="props"
              :page="props.page"
              :id="`page${props.page.number}`"
              tag="router-link"
              :to="{
                name: 'shop',
                query: { ...$route.query, page: props.page.number },
              }"
            >
              {{ props.page.number }}
            </b-pagination-button>

            <b-pagination-button
              slot="previous"
              slot-scope="props"
              :page="props.page"
              tag="router-link"
              :to="{
                name: 'shop',
                query: { ...$route.query, page: props.page.number },
              }"
            >
              <b-icon icon="chevron-left" size="is-small" />
            </b-pagination-button>

            <b-pagination-button
              slot="next"
              slot-scope="props"
              :page="props.page"
              tag="router-link"
              :to="{
                name: 'shop',
                query: { ...$route.query, page: props.page.number },
              }"
            >
              <b-icon icon="chevron-right" size="is-small" />
            </b-pagination-button>
          </b-pagination>
        </div>
      </div>
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
import ShopFiltersForm from '@/components/ShopFiltersForm.vue';
import ShopFilters from '@/models/shop-filters';
import ItemType from '@/models/item-type';
import { filterItemsByType } from '@/services/item-service';
import Culture from '@/models/culture';

@Component({
  components: { ShopFilterForm: ShopFiltersForm, ItemProperties },
})
export default class Shop extends Vue {
  publicPath = process.env.BASE_URL;

  // items for which buy request was sent
  buyingItems: Record<number, boolean> = {};

  itemsPerPage = 20;

  // items owned by the user
  get ownedItems(): Record<number, boolean> {
    return userModule.ownedItems.reduce((res: Record<number, boolean>, i: Item) => {
      res[i.id] = true;
      return res;
    }, {});
  }

  get gold(): number {
    return userModule.user == null ? 0 : userModule.user.gold;
  }

  get currentPage(): number {
    const pageQuery = this.$route.query.page
      ? parseInt(this.$route.query.page as string, 10)
      : undefined;
    if (
      !this.filteredItems ||
      !pageQuery ||
      pageQuery > Math.ceil(this.filteredItems.length / this.itemsPerPage)
    ) {
      return 1;
    }

    return pageQuery;
  }

  get filters(): ShopFilters {
    return {
      type: this.$route.query.type ? (this.$route.query.type as ItemType) : null,
      culture: this.$route.query.culture ? (this.$route.query.culture as Culture) : null,
      showOwned:
        this.$route.query.showOwned !== undefined ? this.$route.query.showOwned === 'true' : true,
    };
  }

  set filters({ type, culture, showOwned }: ShopFilters) {
    this.$router.push({
      query: {
        ...this.$route.query,
        type,
        culture,
        showOwned: showOwned === true ? undefined : false.toString(),
        page: '1',
      },
    });
  }

  get filteredItems(): { item: Item; weaponIdx: number | undefined }[] {
    const filteredItems = itemModule.items.filter(
      i =>
        (this.filters.showOwned || this.ownedItems[i.id] === undefined) &&
        // When the user filters by a culture, Neutral items are always added in the result.
        (this.filters.culture === null ||
          i.culture === this.filters.culture ||
          i.culture === Culture.Neutral)
    );
    return filterItemsByType(filteredItems, this.filters.type);
  }

  get pageItems(): { item: Item; weaponIdx: number | undefined }[] {
    if (!this.filteredItems) {
      return [];
    }

    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.filteredItems.slice(startIndex, endIndex);
  }

  created(): void {
    itemModule.getItems();
    userModule.getOwnedItems();
  }

  async buy(item: Item): Promise<void> {
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
