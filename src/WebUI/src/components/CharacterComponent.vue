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
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Head)">
            <img
              v-if="userItemsBySlot[itemSlot.Head]"
              :src="userItemImage(userItemsBySlot[itemSlot.Head])"
              alt="Head armor"
            />
            <img v-else src="../assets/head-armor.png" alt="Head armor" class="item-placeholder" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Shoulder)">
            <img
              v-if="userItemsBySlot[itemSlot.Shoulder]"
              :src="userItemImage(userItemsBySlot[itemSlot.Shoulder])"
              alt="Shoulder"
            />
            <img v-else src="../assets/cape.png" alt="Shoulder" class="item-placeholder" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Body)">
            <img
              v-if="userItemsBySlot[itemSlot.Body]"
              :src="userItemImage(userItemsBySlot[itemSlot.Body])"
              alt="Body armor"
            />
            <img v-else src="../assets/body-armor.png" alt="Body armor" class="item-placeholder" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Hand)">
            <img
              v-if="userItemsBySlot[itemSlot.Hand]"
              :src="userItemImage(userItemsBySlot[itemSlot.Hand])"
              alt="Hand armor"
            />
            <img v-else src="../assets/hand-armor.png" alt="Hand armor" class="item-placeholder" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Leg)">
            <img
              v-if="userItemsBySlot[itemSlot.Leg]"
              :src="userItemImage(userItemsBySlot[itemSlot.Leg])"
              alt="Leg armor"
            />
            <img v-else src="../assets/leg-armor.png" alt="Leg armor" class="item-placeholder" />
          </div>
        </div>

        <div class="column is-narrow mount-column">
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.MountHarness)">
            <img
              v-if="userItemsBySlot[itemSlot.MountHarness]"
              :src="userItemImage(userItemsBySlot[itemSlot.MountHarness])"
              alt="Mount harness"
            />
            <img
              v-else
              src="../assets/horse-harness.png"
              alt="Horse harness"
              class="item-placeholder"
            />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Mount)">
            <img
              v-if="userItemsBySlot[itemSlot.Mount]"
              :src="userItemImage(userItemsBySlot[itemSlot.Mount])"
              alt="Mount"
            />
          </div>
        </div>

        <div class="column is-narrow weapon-column">
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon0)">
            <img
              v-if="userItemsBySlot[itemSlot.Weapon0]"
              :src="userItemImage(userItemsBySlot[itemSlot.Weapon0])"
              alt="First weapon"
            />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon1)">
            <img
              v-if="userItemsBySlot[itemSlot.Weapon1]"
              :src="userItemImage(userItemsBySlot[itemSlot.Weapon1])"
              alt="Second weapon"
            />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon2)">
            <img
              v-if="userItemsBySlot[itemSlot.Weapon2]"
              :src="userItemImage(userItemsBySlot[itemSlot.Weapon2])"
              alt="Third weapon"
            />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon3)">
            <img
              v-if="userItemsBySlot[itemSlot.Weapon3]"
              :src="userItemImage(userItemsBySlot[itemSlot.Weapon3])"
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
            Automatically repair damaged items (average repair cost
            {{ averageRepairCost }} gold)
          </b-switch>
        </div>
      </b-tooltip>

      <br />

      <b-tooltip label="Respecialize character for a third of its experience." multilined>
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
    </div>

    <b-modal :active.sync="isReplaceItemModalActive" scroll="keep" ref="replaceItemModal">
      <div class="replace-item-modal is-flex is-flex-direction-column px-4 py-4">
        <div class="is-flex-shrink-1">
          <h3 class="is-size-4 mb-2">
            Replace {{ userItemToReplaceSlot }}
            <span v-if="userItemToReplace" :class="userItemRankClass(userItemToReplace)">
              (
              <strong>{{ userItemToReplace.baseItem.name }}</strong>
              )
            </span>
          </h3>
        </div>
        <div class="columns">
          <div
            class="column is-flex is-flex-direction-column is-align-items-center"
            v-if="userItemToReplace"
          >
            <div class="is-flex-grow-1 is-align-self-center is-flex is-align-items-center">
              <div class="user-item">
                <display-user-item :user-item="userItemToReplace" />
              </div>
            </div>
            <div class="is-flex-shrink-1 columns mt-3">
              <div class="column">
                <b-button size="is-medium" expanded @click="unequipItem">Unequip</b-button>
              </div>
              <div class="column">
                <b-button
                  size="is-medium"
                  type="is-warning"
                  icon-left="angle-double-up"
                  expanded
                  :disabled="!itemToReplaceUpgradeInfo.upgradable"
                  :title="itemToReplaceUpgradeInfo.reason"
                  @click="upgradeItem"
                >
                  Upgrade
                </b-button>
              </div>
              <div class="column">
                <b-button
                  size="is-medium"
                  type="is-danger"
                  icon-left="coins"
                  expanded
                  @click="showSellItemConfirmation(userItemToReplace)"
                >
                  Sell
                </b-button>
              </div>
            </div>
          </div>
          <div class="column">
            <div
              v-if="fittingUserItems.length"
              class="user-items columns is-multiline is-justify-content-end"
            >
              <div
                class="column is-narrow user-item user-item__action is-relative"
                v-for="userItem in fittingUserItems"
                v-bind:key="userItem.id"
                @click="selectedUserItem = userItem"
              >
                <display-user-item :user-item="userItem" />
                <div
                  v-if="selectedUserItem && selectedUserItem.id === userItem.id"
                  class="confirm-selection__overlay px-2 py-3 is-flex is-flex-direction-column is-align-items-center is-justify-content-center has-text-centered"
                  @click="confirmItemSelection"
                >
                  <span class="is-size-6">Replace with:</span>
                  <div>
                    <strong :class="userItemRankClass(selectedUserItem)" class="is-size-5">
                      {{ selectedUserItem.baseItem.name }}
                    </strong>
                  </div>
                  <b-icon icon="check" size="is-large" />
                </div>
              </div>
            </div>
            <div v-else class="py-3 has-text-centered">
              <template v-if="userItemToReplace">
                You don't own any other items of this type.
              </template>
              <template v-else>You don't own any items of this type.</template>
            </div>
          </div>
        </div>
      </div>
    </b-modal>

    <b-modal :active.sync="isConfirmSellItemModalActive" scroll="keep" ref="confirmSellItemModal">
      <div
        v-if="userItemToSell"
        class="columns is-flex-direction-column is-marginless is-align-items-center sell-item-modal has-background-white"
      >
        <div class="column">
          <div class="has-text-centered">
            <span class="is-size-4">
              Are you sure you want to sell
              <strong :class="userItemRankClass(userItemToSell)">
                {{ userItemToSell.baseItem.name }}
              </strong>
              ?
            </span>
          </div>
        </div>

        <div class="column">
          <div class="user-item">
            <display-user-item :user-item="userItemToSell" />
          </div>
        </div>

        <div class="column">
          <item-properties :item="userItemToReplace.baseItem" :rank="userItemToReplace.rank" />
          <b-button size="is-medium" expanded @click="unequipItem">Unequip</b-button>
          <b-button
            size="is-medium"
            type="is-warning"
            icon-left="angle-double-up"
            expanded
            :disabled="!itemToReplaceUpgradeInfo.upgradable"
            :title="itemToReplaceUpgradeInfo.reason"
            @click="upgradeItem"
          >
            Upgrade
          </b-button>
          <b-button
            size="is-medium"
            type="is-danger"
            icon-left="coins"
            expanded
            @click="confirmSellItem"
          >
            Sell
          </b-button>
          <b-button
            size="is-medium"
            type="is-secondary"
            icon-left="xmark"
            expanded
            @click="cancelSellItem"
          >
            Cancel
          </b-button>
        </div>
      </div>
    </b-modal>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import ItemProperties from '@/components/ItemProperties.vue';
import userModule from '@/store/user-module';
import Character from '@/models/character';
import ItemSlot from '@/models/item-slot';
import { computeAverageRepairCost } from '@/services/characters-service';
import { filterUserItemsFittingInSlot } from '@/services/item-service';
import { NotificationType, notify } from '@/services/notifications-service';
import CharacterStatsComponent from '@/components/CharacterStatsComponent.vue';
import EquippedItem from '@/models/equipped-item';
import UserItem from '@/models/user-item';
import DisplayUserItem from '@/components/user/DisplayUserItem.vue';

@Component({
  components: { DisplayUserItem, CharacterStatsComponent, ItemProperties },
})
export default class CharacterComponent extends Vue {
  @Prop(Object) readonly character: Character;

  // modal stuff
  itemSlot = ItemSlot;
  isReplaceItemModalActive = false;
  isConfirmSellItemModalActive = false;
  userItemToReplace: UserItem | null = null;
  userItemToReplaceSlot: ItemSlot | null = null;
  userItemToSell: UserItem | null = null;
  selectedUserItem: UserItem | null = null;

  get characterEquippedItems(): EquippedItem[] | null {
    return userModule.characterEquippedItems(this.character.id);
  }

  get userItemsBySlot(): Record<ItemSlot, UserItem> {
    if (this.characterEquippedItems === null) {
      return {} as Record<ItemSlot, UserItem>;
    }

    return this.characterEquippedItems.reduce((userItemsBySlot, ei) => {
      userItemsBySlot[ei.slot] = ei.userItem;
      return userItemsBySlot;
    }, {} as Record<ItemSlot, UserItem>);
  }

  get averageRepairCost(): number {
    if (this.characterEquippedItems === null) {
      return 0;
    }

    return Math.floor(computeAverageRepairCost(this.characterEquippedItems));
  }

  get fittingUserItems(): UserItem[] {
    return this.userItemToReplaceSlot === null
      ? []
      : filterUserItemsFittingInSlot(userModule.userItems, this.userItemToReplaceSlot).filter(
          ui => this.userItemToReplace === null || ui.id !== this.userItemToReplace.id
        );
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

  openReplaceItemModal(slot: ItemSlot): void {
    this.userItemToReplace = this.userItemsBySlot[slot] ? this.userItemsBySlot[slot] : null;
    this.userItemToReplaceSlot = slot;
    this.selectedUserItem = null;
    if (userModule.userItems.length === 0) {
      userModule.getUserItems();
    }

    this.isReplaceItemModalActive = true;
  }

  unequipItem(): void {
    userModule.replaceItem({
      character: this.character,
      slot: this.userItemToReplaceSlot!,
      userItem: null,
    });
    (this.$refs.replaceItemModal as any).close();
  }

  upgradeItem(): void {
    userModule.upgradeUserItem(this.userItemToReplace!);
    (this.$refs.replaceItemModal as any).close();
  }

  async confirmSellItem(): Promise<void> {
    const salePrice = await userModule.sellUserItem(this.userItemToReplace!);
    notify(
      `Sold ${this.userItemToReplace?.baseItem.name} for ${salePrice} gold`,
      NotificationType.Info
    );
    (this.$refs.replaceItemModal as any).close();
    (this.$refs.confirmSellItemModal as any).close();
  }

  confirmItemSelection(): void {
    userModule.replaceItem({
      character: this.character,
      slot: this.userItemToReplaceSlot!,
      userItem: this.selectedUserItem!,
    });
    this.isReplaceItemModalActive = false;
  }

  showSellItemConfirmation(userItem: UserItem): void {
    this.isConfirmSellItemModalActive = true;
    this.userItemToSell = userItem;
  }

  cancelSellItem(): void {
    this.isConfirmSellItemModalActive = false;
    this.userItemToSell = null;
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

.replace-item-modal {
  background-color: #fff; // TODO: replace with bulma variable
  // inherit modal height
  max-height: inherit;
}

.user-items {
  overflow: auto;

  & > .columns {
    justify-content: center;
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

.confirm-selection__overlay {
  border-radius: 50%;
  opacity: 0.85;
  position: absolute;
  left: 20%;
  right: 20%;
  top: 20%;
  bottom: 20%;
  background-color: #bdbdbd;
  transition: background-color 200ms;
  &:hover {
    background-color: #c8e1c8;
  }
}
</style>
