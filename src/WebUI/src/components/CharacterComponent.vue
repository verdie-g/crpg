<template>
  <div class="columns container is-fluid">
    <character-stats-component
      v-if="character !== null"
      :character="character"
      class="column character-stats"
    />

    <div class="column character-items">
      <div class="columns item-boxes">
        <div class="column is-narrow gear-column">
          <div :class="getBoxClasses(itemSlot.Head)" @click="onItemBoxClicked(itemSlot.Head)">
            <img
              v-if="userItemBySlot[itemSlot.Head]"
              :src="userItemImage(userItemBySlot[itemSlot.Head])"
              alt="Head armor"
            />

            <img v-else src="../assets/head-armor.png" alt="Head armor" class="item-placeholder" />
          </div>
          <div
            :class="getBoxClasses(itemSlot.Shoulder)"
            @click="onItemBoxClicked(itemSlot.Shoulder)"
          >
            <img
              v-if="userItemBySlot[itemSlot.Shoulder]"
              :src="userItemImage(userItemBySlot[itemSlot.Shoulder])"
              alt="Shoulder"
            />
            <img v-else src="../assets/cape.png" alt="Shoulder" class="item-placeholder" />
          </div>
          <div :class="getBoxClasses(itemSlot.Body)" @click="onItemBoxClicked(itemSlot.Body)">
            <img
              v-if="userItemBySlot[itemSlot.Body]"
              :src="userItemImage(userItemBySlot[itemSlot.Body])"
              alt="Body armor"
            />
            <img v-else src="../assets/body-armor.png" alt="Body armor" class="item-placeholder" />
          </div>
          <div :class="getBoxClasses(itemSlot.Hand)" @click="onItemBoxClicked(itemSlot.Hand)">
            <img
              v-if="userItemBySlot[itemSlot.Hand]"
              :src="userItemImage(userItemBySlot[itemSlot.Hand])"
              alt="Hand armor"
            />
            <img v-else src="../assets/hand-armor.png" alt="Hand armor" class="item-placeholder" />
          </div>
          <div :class="getBoxClasses(itemSlot.Leg)" @click="onItemBoxClicked(itemSlot.Leg)">
            <img
              v-if="userItemBySlot[itemSlot.Leg]"
              :src="userItemImage(userItemBySlot[itemSlot.Leg])"
              alt="Leg armor"
            />
            <img v-else src="../assets/leg-armor.png" alt="Leg armor" class="item-placeholder" />
          </div>
        </div>

        <div class="column is-narrow mount-column">
          <div
            :class="getBoxClasses(itemSlot.MountHarness)"
            @click="onItemBoxClicked(itemSlot.MountHarness)"
          >
            <img
              v-if="userItemBySlot[itemSlot.MountHarness]"
              :src="userItemImage(userItemBySlot[itemSlot.MountHarness])"
              alt="Mount harness"
            />
            <img
              v-else
              src="../assets/horse-harness.png"
              alt="Horse harness"
              class="item-placeholder"
            />
          </div>
          <div :class="getBoxClasses(itemSlot.Mount)" @click="onItemBoxClicked(itemSlot.Mount)">
            <img
              v-if="userItemBySlot[itemSlot.Mount]"
              :src="userItemImage(userItemBySlot[itemSlot.Mount])"
              alt="Mount"
            />
          </div>
        </div>

        <div class="column is-narrow weapon-column">
          <div :class="getBoxClasses(itemSlot.Weapon0)" @click="onItemBoxClicked(itemSlot.Weapon0)">
            <img
              v-if="userItemBySlot[itemSlot.Weapon0]"
              :src="userItemImage(userItemBySlot[itemSlot.Weapon0])"
              alt="First weapon"
            />
          </div>
          <div :class="getBoxClasses(itemSlot.Weapon1)" @click="onItemBoxClicked(itemSlot.Weapon1)">
            <img
              v-if="userItemBySlot[itemSlot.Weapon1]"
              :src="userItemImage(userItemBySlot[itemSlot.Weapon1])"
              alt="Second weapon"
            />
          </div>
          <div :class="getBoxClasses(itemSlot.Weapon2)" @click="onItemBoxClicked(itemSlot.Weapon2)">
            <img
              v-if="userItemBySlot[itemSlot.Weapon2]"
              :src="userItemImage(userItemBySlot[itemSlot.Weapon2])"
              alt="Third weapon"
            />
          </div>
          <div :class="getBoxClasses(itemSlot.Weapon3)" @click="onItemBoxClicked(itemSlot.Weapon3)">
            <img
              v-if="userItemBySlot[itemSlot.Weapon3]"
              :src="userItemImage(userItemBySlot[itemSlot.Weapon3])"
              alt="Fourth Weapon"
            />
          </div>
        </div>
      </div>

      <b-tooltip
        label="Some of your items might break at the end of a round. Switch automatic repair on so you don't have to repair manually."
        multilined
      >
        <div class="field">
          <b-switch :value="character.autoRepair" @input="onAutoRepairSwitch" disabled>
            Automatically repair damaged items
          </b-switch>
        </div>
      </b-tooltip>

      <br />

      <b-tooltip label="Respecialize character." multilined>
        <b-button
          type="is-warning"
          icon-left="angle-double-down"
          expanded
          @click="openRespecializeCharacterDialog"
        >
          Respecialize
        </b-button>
      </b-tooltip>

      <b-tooltip
        label="Reset character to level 1 to grant a bonus multiplier and an heirloom point. (lvl > 30)"
        multilined
        class="mr-2"
      >
        <b-button
          type="is-warning"
          icon-left="baby"
          expanded
          :disabled="character.level < 31"
          @click="openRetireCharacterDialog"
        >
          Retire
        </b-button>
      </b-tooltip>

      <b-button type="is-danger" icon-left="trash" @click="openDeleteCharacterDialog">
        Delete
      </b-button>

      <character-overall-items-stats-component :items="characterEquippedUserItems" />
    </div>

    <character-items-component
      :character="character"
      :item-filter="selectedItemSlot"
      @equip="equip"
      @unequip="unequip"
      @selectItemSlot="selectItem"
      @deselectItemSlot="deselectItem"
    />
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import ItemProperties from '@/components/ItemProperties.vue';
import userModule from '@/store/user-module';
import Character from '@/models/character';
import ItemSlot from '@/models/item-slot';
import { notify } from '@/services/notifications-service';
import CharacterStatsComponent from '@/components/CharacterStatsComponent.vue';
import CharacterOverallItemsStatsComponent from '@/components/CharacterOverallItemsStatsComponent.vue';
import CharacterItemsComponent from '@/components/CharacterItemsComponent.vue';
import EquippedItem from '@/models/equipped-item';
import UserItem from '@/models/user-item';
import { getSlotsForUserItem, itemFitsInFreeWeaponSlot } from '@/services/item-service';

@Component({
  components: {
    CharacterStatsComponent,
    ItemProperties,
    CharacterOverallItemsStatsComponent,
    CharacterItemsComponent,
  },
})
export default class CharacterComponent extends Vue {
  @Prop(Object) readonly character: Character;

  itemSlot = ItemSlot;
  selectedItemSlot: string | null = null;
  userItemToReplace: UserItem | null = null;
  userItemToReplaceSlot: ItemSlot | null = null;
  userItemToSell: UserItem | null = null;
  selectedUserItem: UserItem | null = null;

  get characterEquippedItems(): EquippedItem[] {
    return userModule.characterEquippedItems(this.character.id) || [];
  }

  get characterEquippedUserItems(): UserItem[] {
    return this.characterEquippedItems.map(ei => ei.userItem);
  }

  get userItemBySlot(): Record<ItemSlot, UserItem | undefined> {
    if (this.characterEquippedItems === null) {
      return {} as Record<ItemSlot, UserItem>;
    }

    return this.characterEquippedItems.reduce((userItemBySlot, ei) => {
      userItemBySlot[ei.slot] = ei.userItem;
      return userItemBySlot;
    }, {} as Record<ItemSlot, UserItem>);
  }

  get itemToReplaceUpgradeInfo(): { upgradable: boolean; reason: string } {
    // const info = { upgradable: true, reason: '' };
    // if (this.userItemToReplace === null) {
    //   return info;
    // }

    // if (this.userItemToReplace.rank === 3) {
    //   info.upgradable = false;
    //   info.reason = 'Max rank reached (3)';
    // } else if (userModule.user!.heirloomPoints === 0) {
    //   info.upgradable = false;
    //   info.reason = 'Not enough heirloom points';
    // }

    // return info;
    return { upgradable: false, reason: 'Heirloom are disabled for now' };
  }

  getBoxClasses(itemSlot: ItemSlot): string {
    let classes = 'box item-box';
    const item = this.selectedUserItem;
    if (!item) return classes;

    const slotsForUserItem = getSlotsForUserItem(item);
    if (slotsForUserItem.length > 1) {
      const weaponSlots = [ItemSlot.Weapon0, ItemSlot.Weapon1, ItemSlot.Weapon2, ItemSlot.Weapon3];
      if (
        !itemFitsInFreeWeaponSlot(this.characterEquippedItems, item) &&
        this.characterEquippedItems.some(equippedItem => equippedItem.userItem.id === item.id)
      ) {
        for (let weaponSlot of weaponSlots) {
          if (this.userItemBySlot[weaponSlot] && this.userItemBySlot[weaponSlot]!.id === item.id) {
            if (itemSlot === weaponSlot) return (classes += ' circle-sketch-highlight');
            return classes;
          }
        }
        return classes;
      }

      for (let weaponSlot of weaponSlots) {
        if (!this.userItemBySlot[weaponSlot]) {
          if (itemSlot === weaponSlot) return (classes += ' circle-sketch-highlight');
          return classes;
        }
      }
      if (itemSlot == ItemSlot.Weapon0) return (classes += ' circle-sketch-highlight');
      return classes;
    } else if (slotsForUserItem[0] === itemSlot) return (classes += ' circle-sketch-highlight');
    return classes;
  }

  created() {
    userModule.getCharacterItems(this.character.id);
  }

  userItemImage(userItem: UserItem): string {
    return `${process.env.BASE_URL}items/${userItem.baseItem.id}.png`;
  }

  userItemRankClass(userItem: UserItem | null): string {
    return userItem === null ? '' : `item-rank${userItem.rank}`;
  }

  onAutoRepairSwitch(autoRepair: boolean): void {
    userModule.switchAutoRepair({ character: this.character, autoRepair });
  }

  openRespecializeCharacterDialog(): void {
    this.$buefy.dialog.confirm({
      title: 'Respecialize character',
      message: `Are you sure you want to respecialize your character ${this.character.name} lvl. ${this.character.level}?
        This action cannot be undone.`,
      confirmText: 'Respecialize Character',
      type: 'is-danger',
      hasIcon: true,
      onConfirm: () => {
        userModule.respecializeCharacter(this.character);
        notify('Character respecialized');
      },
    });
  }

  openRetireCharacterDialog(): void {
    this.$buefy.dialog.confirm({
      title: 'Retiring character',
      message: `Are you sure you want to retire your character ${this.character.name} lvl. ${this.character.level}?
        This action cannot be undone.`,
      confirmText: 'Retire',
      type: 'is-warning',
      hasIcon: true,
      onConfirm: () => {
        userModule.retireCharacter(this.character);
        notify('Character retired');
      },
    });
  }

  openDeleteCharacterDialog(): void {
    this.$buefy.dialog.confirm({
      title: 'Deleting character',
      message: `Are you sure you want to delete your character ${this.character.name} lvl. ${this.character.level}?
        This action cannot be undone.`,
      confirmText: 'Delete Character',
      type: 'is-danger',
      hasIcon: true,
      onConfirm: () => {
        userModule.deleteCharacter(this.character);
        notify('Character deleted');
      },
    });
  }

  upgradeItem(): void {
    userModule.upgradeUserItem(this.userItemToReplace!);
    (this.$refs.replaceItemModal as any).close();
  }

  onItemBoxClicked(slot: ItemSlot) {
    const userItem = this.userItemBySlot[slot];
    if (userItem) this.unequip(userItem, slot);
    else this.selectedItemSlot = slot;
  }

  selectItem(userItem: UserItem) {
    this.selectedUserItem = userItem;
  }

  deselectItem() {
    this.selectedUserItem = null;
  }

  equip(userItem: UserItem) {
    const result = getSlotsForUserItem(userItem);
    let slot = result[0];
    if (result.length > 1) {
      if (!this.userItemBySlot[ItemSlot.Weapon0]) slot = ItemSlot.Weapon0;
      else if (!this.userItemBySlot[ItemSlot.Weapon1]) slot = ItemSlot.Weapon1;
      else if (!this.userItemBySlot[ItemSlot.Weapon2]) slot = ItemSlot.Weapon2;
      else if (!this.userItemBySlot[ItemSlot.Weapon3]) slot = ItemSlot.Weapon3;
      else slot = ItemSlot.Weapon0;
    }

    userModule.replaceItem({
      character: this.character,
      slot: slot,
      userItem: userItem,
    });
  }

  unequip(userItem: UserItem, itemSlot: ItemSlot | null) {
    const result = getSlotsForUserItem(userItem);
    if (!itemSlot) {
      itemSlot = result[0];
      if (result.length > 1) {
        if (this.userItemBySlot[ItemSlot.Weapon0]?.id === userItem.id) itemSlot = ItemSlot.Weapon0;
        else if (this.userItemBySlot[ItemSlot.Weapon1]?.id === userItem.id)
          itemSlot = ItemSlot.Weapon1;
        else if (this.userItemBySlot[ItemSlot.Weapon2]?.id === userItem.id)
          itemSlot = ItemSlot.Weapon2;
        else if (this.userItemBySlot[ItemSlot.Weapon3]?.id === userItem.id)
          itemSlot = ItemSlot.Weapon3;
      }
    }

    userModule.replaceItem({
      character: this.character,
      slot: itemSlot,
      userItem: null,
    });
  }
}
</script>

<style scoped lang="scss">
.item-boxes {
  background-image: url('../assets/body-silhouette.svg');
  background-repeat: no-repeat;
  background-size: 190px;
  background-position: 180px 0px;
  padding-bottom: 8px; // so the silhouette's feet ain't cropped
}

.mount-column {
  width: 250px;
  display: flex;
  justify-content: flex-end;
  flex-direction: column;
  align-items: center;
}

.item-box {
  // item image dimensions / 2
  width: 128px;
  height: 60px;
  padding: 0;
  cursor: pointer;
  text-align: center; // to align horizontally placeholder

  &:hover {
    box-shadow: 0 5px 8px rgba(10, 10, 10, 0.1), 0 0 0 1px rgba(10, 10, 10, 0.1);
  }

  .item-placeholder {
    margin-top: -3px; // because placeholder is 66 px in a 60px box
    opacity: 0.3;
  }
}

.user-item {
  width: 256px;

  // Only apply hover styles to items with a click action
  &__action {
    cursor: pointer;
    // Apply hover effect to any user-items with the hover-action class
    &:hover {
      background-color: #fafafa; // TODO: use bulma variable
    }
  }
}

.item-rank-3 {
  color: #ff3860;
}
.item-rank-2 {
  color: #ff3860;
}
.item-rank-1 {
  color: #ff3860;
}
.item-rank0 {
  color: #0a0a0a;
}
.item-rank1 {
  color: #48c774;
}
.item-rank2 {
  color: #3273dc;
}
.item-rank3 {
  color: #774fc2;
}

.circle-sketch-highlight {
  position: relative;
  font-size: 32px;
  font-weight: 500;
}
.circle-sketch-highlight:before {
  content: '';
  z-index: 1;
  left: 0.04em;
  top: -0.1em;
  border-width: 4px;
  border-style: solid;
  border-color: #ef8c22;
  position: absolute;
  border-right-color: transparent;
  width: 100%;
  height: 1em;
  transform: rotate(1deg);
  opacity: 0.7;
  border-radius: 50%;
  padding: 1em 1.5em;
}
.circle-sketch-highlight:after {
  content: '';
  z-index: 1;
  left: 0em;
  top: 0em;
  padding: 0.8em 0.8em;
  border-width: 4px;
  border-style: solid;
  border-color: #ef8c22;
  border-left-color: transparent;
  border-top-color: transparent;
  position: absolute;
  width: 100%;
  height: 1em;
  transform: rotate(-1deg);
  opacity: 0.7;
  border-radius: 50%;
}
</style>
