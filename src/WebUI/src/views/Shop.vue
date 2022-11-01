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
              <div class="card item-card is-flex is-flex-direction-column">
                <div class="card-image">
                  <figure class="image">
                    <img
                      :src="`${publicPath}items/${item.id}.png`"
                      alt="item image"
                      loading="lazy"
                      style="height: 120px"
                    />
                  </figure>
                </div>
                <div
                  class="card-content content is-flex-grow-1 is-flex is-flex-direction-column px-4"
                >
                  <h4 class="px-1">{{ item.name }}</h4>
                  <div class="content is-flex-grow-1 is-flex">
                    <item-properties :item="item" :rank="0" :weapon-idx="weaponIdx" />
                  </div>
                </div>
                <footer class="card-footer">
                  <b-button
                    icon-left="coins"
                    expanded
                    :disabled="!canBuyItem(item)"
                    :loading="buyingItems[item.id]"
                    @click="buy(item)"
                    :title="buyButtonTitle(item)"
                  >
                    {{ item.price }}
                  </b-button>
                </footer>
              </div>
            </div>
          </div>

          <b-pagination
            :total="filteredItems.length"
            :current.sync="currentPage"
            :per-page="this.filters.itemsPerPage"
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
import { filterItemsByType, itemTypeToStr } from '@/services/item-service';
import Culture from '@/models/culture';
import UserItem from '@/models/user-item';

@Component({
  components: { ShopFilterForm: ShopFiltersForm, ItemProperties },
})
export default class Shop extends Vue {
  publicPath = process.env.BASE_URL;

  // items for which buy request was sent
  buyingItems: Record<string, boolean> = {};

  // items owned by the user
  get ownedItems(): Record<string, boolean> {
    return userModule.userItems.reduce((res: Record<string, boolean>, ui: UserItem) => {
      res[ui.baseItem.id] = true;
      return res;
    }, {});
  }

  get gold(): number {
    return userModule.user == null ? 0 : userModule.user.gold;
  }

  get isUserDonor(): boolean {
    return userModule.user == null ? false : userModule.user.isDonor;
  }

  get currentPage(): number {
    const pageQuery = this.$route.query.page
      ? parseInt(this.$route.query.page as string, 10)
      : undefined;
    if (
      !this.filteredItems ||
      !pageQuery ||
      pageQuery > Math.ceil(this.filteredItems.length / this.filters.itemsPerPage)
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
      showAffordable:
        this.$route.query.showAffordable !== undefined
          ? this.$route.query.showAffordable === 'true'
          : false,
      searchQuery: this.$route.query.searchQuery ? (this.$route.query.searchQuery as string) : '',
      itemsPerPage: this.$route.query.itemsPerPage
        ? Number.parseInt(this.$route.query.itemsPerPage as string)
        : 20,
    };
  }

  set filters({
    type,
    culture,
    showOwned,
    showAffordable,
    searchQuery,
    itemsPerPage,
  }: ShopFilters) {
    this.$router.push({
      query: {
        ...this.$route.query,
        type,
        culture,
        showOwned: showOwned === true ? undefined : false.toString(),
        showAffordable: showAffordable === true ? true.toString() : undefined,
        page: '1',
        searchQuery: searchQuery,
        itemsPerPage: itemsPerPage.toString(),
      },
    });
  }

  get filteredItems(): { item: Item; weaponIdx: number | undefined }[] {
    const filteredItems = itemModule.items.filter(i => {
      if (!this.filters.showOwned && this.ownedItems[i.id] !== undefined) {
        return false;
      }
      if (this.filters.showAffordable && userModule.user?.gold && i.price > userModule.user.gold) {
        return false;
      }
      if (!this.matchesItem(i, this.filters.searchQuery)) return false;

      // When the user filters by a culture, Neutral items are always added in the result.
      return (
        this.filters.culture === null ||
        i.culture === this.filters.culture ||
        i.culture === Culture.Neutral
      );
    });
    return filterItemsByType(filteredItems, this.filters.type);
  }

  get pageItems(): { item: Item; weaponIdx: number | undefined }[] {
    if (!this.filteredItems) {
      return [];
    }

    const startIndex = (this.currentPage - 1) * this.filters.itemsPerPage;
    const endIndex = startIndex + this.filters.itemsPerPage;
    return this.filteredItems.slice(startIndex, endIndex);
  }

  created(): void {
    itemModule.getItems();
    userModule.getUserItems();
  }

  async buy(item: Item): Promise<void> {
    Vue.set(this.buyingItems, item.id, true);
    await userModule.buyItem(item);
    Vue.set(this.buyingItems, item.id, false);
    notify(`Bought ${item.name} for ${item.price} gold`);
  }

  canBuyItem(item: Item): boolean {
    return (
      this.gold >= item.price &&
      !this.ownedItems[item.id] &&
      (item.type !== ItemType.Banner || this.isUserDonor)
    );
  }

  buyButtonTitle(item: Item): string {
    if (this.ownedItems[item.id]) {
      return 'You already own this item';
    }

    if (item.type === ItemType.Banner && !this.isUserDonor) {
      return 'Only donors can buy this item';
    }

    if (item.price > this.gold) {
      return 'Not enough gold';
    }

    return '';
  }

  matchesItem(item: Item, searchQuery: string): boolean {
    if (searchQuery.length === 0) return true;

    const lowerCaseSearchQuery = searchQuery.toLowerCase();
    return (
      item.name.toLowerCase().includes(lowerCaseSearchQuery) ||
      item.culture.toLowerCase().includes(lowerCaseSearchQuery) ||
      itemTypeToStr[item.type].toLowerCase().includes(lowerCaseSearchQuery)
    );
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
