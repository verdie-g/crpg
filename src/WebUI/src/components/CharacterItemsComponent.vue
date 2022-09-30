<template>
  <div class="card" style="width: 500px">
    <header class="card-header">
      <b-dropdown v-model="itemsFilter" :triggers="['hover']" aria-role="list">
        <template #trigger>
          <b-button icon-right="caret-down">{{ itemsFilter }}</b-button>
        </template>
        <b-dropdown-item
          v-for="filter in itemsFilters"
          :value="filter"
          :key="filter"
          aria-role="listitem"
        >
          <span>{{ filter }}</span>
        </b-dropdown-item>
      </b-dropdown>

      <b-dropdown v-model="sortBy" :triggers="['hover']" aria-role="list">
        <template #trigger>
          <b-button icon-right="caret-down">{{ sortBy }}</b-button>
        </template>
        <b-dropdown-item
          v-for="(sortableProperty, i) in sortableProperties"
          :value="sortableProperty"
          :key="sortableProperty + i"
          aria-role="listitem"
        >
          <span>{{ sortableProperty }}</span>
        </b-dropdown-item>
      </b-dropdown>

      <p class="control">
        <b-button :icon-right="sortButtonIcon" @click="sortDesc = !sortDesc" />
      </p>
    </header>

    <div class="card-content">
      <div class="columns item-boxes">
        <div
          v-for="(n, index) in itemsPerPage"
          v-bind:key="index"
          class="box item-box"
          @mouseover="selectItem(index)"
          @mouseleave="deselectItem"
          @click="changeEquipState"
        >
          <b-tooltip type="is-dark" position="is-bottom" multilined append-to-body>
            <template v-slot:content>
              <item-tooltip-component :user-item="selectedItem" />
            </template>
            <img
              v-if="filteredItems[index * currentPage]"
              :src="userItemImage(filteredItems[index * currentPage])"
            />
            <b-icon
              v-if="isEquippedItem(index)"
              class="equipped-icon"
              type="is-success"
              icon="user-plus"
              size="is-small"
            ></b-icon>
            <b-button
              v-if="showLockedButton(index)"
              class="lock-button"
              type="is-info"
              :icon-right="getLockButtonIcon(index)"
              size="is-small"
              @click.stop="lockButtonClicked(index)"
            />
            <b-button
              v-if="selectedItem && selectedItem === filteredItems[index * currentPage]"
              class="sell-button"
              type="is-danger"
              icon-right="coins"
              size="is-small"
              @click.stop="sellItemModal = true"
            />
          </b-tooltip>
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
        <b-pagination-button slot-scope="props" :page="props.page" :id="`page${props.page.number}`">
          {{ props.page.number }}
        </b-pagination-button>

        <b-pagination-button slot="previous" slot-scope="props" :page="props.page">
          <b-icon icon="chevron-left" size="is-small" />
        </b-pagination-button>

        <b-pagination-button slot="next" slot-scope="props" :page="props.page">
          <b-icon icon="chevron-right" size="is-small" />
        </b-pagination-button>
      </b-pagination>

      <display-user-item
        v-if="selectedItem || lockedItem"
        :user-item="displayUserItem"
        :show-image="false"
      />
    </div>

    <b-modal :active.sync="sellItemModal" scroll="keep">
      <div
        v-if="selectedItem"
        class="columns is-flex-direction-column is-marginless is-align-items-center sell-item-modal has-background-white"
      >
        <div class="column">
          <div class="has-text-centered">
            <span class="is-size-4">
              Are you sure you want to sell
              <strong>
                {{ selectedItem.baseItem.name }}
              </strong>
              ?
            </span>
          </div>
        </div>

        <div class="column">
          <div class="user-item">
            <display-user-item :user-item="selectedItem" />
          </div>
        </div>

        <div class="column">
          <b-button
            size="is-medium"
            type="is-danger"
            icon-left="coins"
            expanded
            @click="confirmSellItem"
          >
            Sell for {{ userItemToReplaceSalePrice }} gold
          </b-button>
          <b-button
            size="is-medium"
            type="is-secondary"
            icon-left="xmark"
            expanded
            @click="sellItemModal = false"
          >
            Cancel
          </b-button>
        </div>
      </div>
    </b-modal>
  </div>
</template>

<script lang="ts">
import Character from '@/models/character';
import EquippedItem from '@/models/equipped-item';
import ItemSlot from '@/models/item-slot';
import UserItem from '@/models/user-item';
import {
  computeSalePrice,
  filterUserItemsFittingInSlot,
  getSortableProperties,
  itemFitsInFreeWeaponSlot,
  sortUserItems,
} from '@/services/item-service';
import userModule from '@/store/user-module';
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import CharacterOverallItemsStatsComponent from '@/components/CharacterOverallItemsStatsComponent.vue';
import ItemTooltipComponent from '@/components/ItemTooltipComponent.vue';
import DisplayUserItem from '@/components/user/DisplayUserItem.vue';
import { NotificationType, notify } from '@/services/notifications-service';

@Component({
  components: {
    DisplayUserItem,
    CharacterOverallItemsStatsComponent,
    ItemTooltipComponent,
  },
})
export default class CharacterItemsComponent extends Vue {
  @Prop(Object) readonly character: Character;
  @Prop(String) readonly itemFilter: string | null;

  sellItemModal = false;
  currentPage = 1;
  itemsPerPage = 12;
  itemsFilters = [
    'All Items',
    'All Armor',
    'Weapons',
    'Head Armor',
    'Shoulder Armor',
    'Body Armor',
    'Hand Armor',
    'Leg Armor',
    'Mount',
    'Mount Harness',
  ];
  itemsFilter = this.itemsFilters[0];
  selectedItem = null as UserItem | null;
  lockedItem = null as UserItem | null;
  sortBy = 'Price';
  sortDesc = true;
  selectedItemIndex = null as number | null;

  created() {
    return userModule.getUserItems();
  }

  get userItems(): UserItem[] {
    const clonedUserItems = [] as UserItem[];
    userModule.userItems.forEach(userItem => clonedUserItems.push(Object.assign({}, userItem)));
    return clonedUserItems;
  }

  get equippedItems(): EquippedItem[] {
    return userModule.characterEquippedItems(this.character.id) || [];
  }

  get filteredItems() {
    switch (this.itemsFilter) {
      case 'All Items':
        return this.sortUserItems(this.userItems, this.sortBy, this.sortDesc);
      case 'All Armor':
        let result = filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Head);
        result = result.concat(
          filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Shoulder)
        );
        result = result.concat(filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Body));
        result = result.concat(filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Hand));
        result = result.concat(filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Leg));
        return this.sortUserItems(result, this.sortBy, this.sortDesc);
      case 'Weapons':
        return this.sortUserItems(
          filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Weapon0),
          this.sortBy,
          this.sortDesc
        );
      case 'Head Armor':
        return this.sortUserItems(
          filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Head),
          this.sortBy,
          this.sortDesc
        );
      case 'Shoulder Armor':
        return this.sortUserItems(
          filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Shoulder),
          this.sortBy,
          this.sortDesc
        );
      case 'Body Armor':
        return this.sortUserItems(
          filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Body),
          this.sortBy,
          this.sortDesc
        );
      case 'Hand Armor':
        return this.sortUserItems(
          filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Hand),
          this.sortBy,
          this.sortDesc
        );
      case 'Leg Armor':
        return this.sortUserItems(
          filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Leg),
          this.sortBy,
          this.sortDesc
        );
      case 'Mount':
        return this.sortUserItems(
          filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.Mount),
          this.sortBy,
          this.sortDesc
        );
      case 'Mount Harness':
        return this.sortUserItems(
          filterUserItemsFittingInSlot(userModule.userItems, ItemSlot.MountHarness),
          this.sortBy,
          this.sortDesc
        );
      default:
        return this.sortUserItems(this.userItems, this.sortBy, this.sortDesc);
    }
  }

  get displayUserItem() {
    if (this.lockedItem) return this.lockedItem;
    return this.selectedItem;
  }

  get userItemToReplaceSalePrice(): number {
    if (this.selectedItem === null) return 0;
    return computeSalePrice(this.selectedItem);
  }

  get pageItems(): UserItem[] {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    return this.filteredItems.slice(startIndex, endIndex);
  }

  get sortableProperties(): string[] {
    if (this.itemsFilter === 'All Items') {
      return ['Price', 'Weight', 'Equipped'];
    }
    const sortableProperties = getSortableProperties(this.filteredItems.map(i => i.baseItem));
    sortableProperties.push('Equipped');
    return sortableProperties;
  }

  get sortButtonIcon() {
    if (this.sortDesc) return 'arrow-down';
    return 'arrow-up';
  }

  lockButtonClicked(index: number) {
    const clickedItem = this.filteredItems[index * this.currentPage];
    if (clickedItem === this.lockedItem) this.lockedItem = null;
    else this.lockedItem = clickedItem;
  }

  getLockButtonIcon(index: number): string {
    if (!this.lockedItem) return 'lock-open';
    if (this.filteredItems[index * this.currentPage] === this.lockedItem) return 'lock';
    return 'lock-open';
  }

  userItemImage(userItem: UserItem): string {
    return `${process.env.BASE_URL}items/${userItem.baseItem.id}.png`;
  }

  userItemRankClass(userItem: UserItem | null): string {
    return userItem === null ? '' : `item-rank${userItem.rank}`;
  }

  isEquippedItem(index: number): boolean {
    const inventoryItem = this.filteredItems[index * this.currentPage];
    if (!inventoryItem) return false;
    return this.equippedItems.some(equippedItem => equippedItem.userItem.id === inventoryItem.id);
  }

  isEquippedItemByItem(userItem: UserItem): boolean {
    return this.equippedItems.some(equippedItem => equippedItem.userItem.id === userItem.id);
  }

  changeEquipState() {
    if (!this.selectedItem) return;
    if (itemFitsInFreeWeaponSlot(this.equippedItems, this.selectedItem))
      this.$emit('equip', this.selectedItem);
    else if (this.isEquippedItemByItem(this.selectedItem)) this.$emit('unequip', this.selectedItem);
    else this.$emit('equip', this.selectedItem);
    if (this.selectedItemIndex !== null) this.selectItem(this.selectedItemIndex);
  }

  async confirmSellItem(): Promise<void> {
    if (!this.selectedItem) return;
    if (this.selectedItem === this.lockedItem) this.lockedItem = null;
    const salePrice = await userModule.sellUserItem(this.selectedItem);
    notify(`Sold ${this.selectedItem?.baseItem.name} for ${salePrice} gold`, NotificationType.Info);
    this.sellItemModal = false;
  }

  showLockedButton(index: number) {
    const inventoryItem = this.filteredItems[index * this.currentPage];
    return (
      (this.selectedItem && this.selectedItem === inventoryItem) ||
      (this.lockedItem && this.lockedItem === inventoryItem)
    );
  }

  selectItem(index: number) {
    this.selectedItemIndex = index;
    this.selectedItem = this.filteredItems[index * this.currentPage];
    this.emitSelectedItem();
  }

  deselectItem() {
    if (this.sellItemModal) return;
    this.selectedItem = null;
    this.emitSelectedItem();
  }

  emitSelectedItem() {
    if (!this.selectedItem) this.$emit('deselectItemSlot');
    else this.$emit('selectItemSlot', this.selectedItem);
  }

  sortUserItems(userItems: UserItem[], sortBy: string, sortDesc: boolean) {
    if (sortBy === 'Equipped') {
      return userItems.sort((i1, i2) => {
        const isEquippedItem1 = this.equippedItems.some(
          equippedItem => equippedItem.userItem.id === i1.id
        );
        const isEquippedItem2 = this.equippedItems.some(
          equippedItem => equippedItem.userItem.id === i2.id
        );
        if (sortDesc) return isEquippedItem1 === isEquippedItem2 ? 0 : isEquippedItem2 ? -1 : 1;
        return isEquippedItem1 === isEquippedItem2 ? 0 : isEquippedItem2 ? 1 : -1;
      });
    }

    return sortUserItems(userItems, sortBy, this.sortDesc);
  }

  @Watch('itemFilter')
  watchItemFilter(itemFilter: ItemSlot | null) {
    if (!itemFilter) return;
    switch (itemFilter) {
      case ItemSlot.Head:
        this.itemsFilter = 'Head Armor';
        break;
      case ItemSlot.Shoulder:
        this.itemsFilter = 'Shoulder Armor';
        break;
      case ItemSlot.Body:
        this.itemsFilter = 'Body Armor';
        break;
      case ItemSlot.Hand:
        this.itemsFilter = 'Hand Armor';
        break;
      case ItemSlot.Leg:
        this.itemsFilter = 'Leg Armor';
        break;
      case ItemSlot.MountHarness:
        this.itemsFilter = 'Mount Harness';
        break;
      case ItemSlot.Mount:
        this.itemsFilter = 'Mount';
        break;
      case ItemSlot.Weapon0:
      case ItemSlot.Weapon1:
      case ItemSlot.Weapon2:
      case ItemSlot.Weapon3:
        this.itemsFilter = 'Weapons';
        break;
    }
  }
}
</script>

<style scoped lang="scss">
.item-boxes {
  background-repeat: no-repeat;
  background-size: 190px;
  background-position: 180px 0px;
  padding-bottom: 8px; // so the silhouette's feet ain't cropped
  display: flex;
  flex-wrap: wrap;
}

.item-box {
  position: relative;
  // item image dimensions / 2
  width: 128px;
  height: 60px;
  padding: 0;
  cursor: pointer;
  text-align: center; // to align horizontally placeholder
  margin: 0.8em;

  &:hover {
    box-shadow: 0 5px 8px rgba(10, 10, 10, 0.1), 0 0 0 1px rgba(10, 10, 10, 0.1);
  }
}
.equipped-icon {
  position: absolute;
  top: 0;
  right: 0;
}
.sell-button {
  position: absolute;
  bottom: 0;
  left: 0;
}
.lock-button {
  position: absolute;
  top: 0;
  left: 0;
}
</style>
