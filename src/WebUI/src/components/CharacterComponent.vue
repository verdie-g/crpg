<template>
  <div class="columns container is-fluid">

    <character-stats-component :character="character" class="column character-stats" />

    <div class="column character-items">
      <div class="columns item-boxes">
        <div class="column is-narrow gear-column">
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Head)">
            <img v-if="character.items.headItem" :src="itemImage(character.items.headItem)" alt="Head armor" />
            <img v-else src="../assets/head-armor.png" alt="Head armor" class="item-placeholder" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Cape)">
            <img v-if="character.items.capeItem" :src="itemImage(character.items.capeItem)" alt="Cape" />
            <img v-else src="../assets/cape.png" alt="Cape" class="item-placeholder" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Body)">
            <img v-if="character.items.bodyItem" :src="itemImage(character.items.bodyItem)" alt="Body armor" />
            <img v-else src="../assets/body-armor.png" alt="Body armor" class="item-placeholder" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Hand)">
            <img v-if="character.items.handItem" :src="itemImage(character.items.handItem)" alt="Hand armor" />
            <img v-else src="../assets/hand-armor.png" alt="Hand armor" class="item-placeholder" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Leg)">
            <img v-if="character.items.legItem" :src="itemImage(character.items.legItem)" alt="Leg armor" />
            <img v-else src="../assets/leg-armor.png" alt="Leg armor" class="item-placeholder" />
          </div>
        </div>

        <div class="column is-narrow horse-column">
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.HorseHarness)">
            <img v-if="character.items.horseHarnessItem" :src="itemImage(character.items.horseHarnessItem)" alt="Horse harness" />
            <img v-else src="../assets/horse-harness.png" alt="Horse harness" class="item-placeholder" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Horse)">
            <img v-if="character.items.horseItem" :src="itemImage(character.items.horseItem)" alt="Horse" />
          </div>
        </div>

        <div class="column is-narrow weapon-column">
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon1)">
            <img v-if="character.items.weapon1Item" :src="itemImage(character.items.weapon1Item)" alt="First weapon" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon2)">
            <img v-if="character.items.weapon2Item" :src="itemImage(character.items.weapon2Item)" alt="Second weapon" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon3)">
            <img v-if="character.items.weapon3Item" :src="itemImage(character.items.weapon3Item)" alt="Third weapon" />
          </div>
          <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon4)">
            <img v-if="character.items.weapon4Item" :src="itemImage(character.items.weapon4Item)" alt="Fourth Weapon" />
          </div>
        </div>
      </div>

      <b-button type="is-warning" icon-left="angle-double-down" expanded
                @click="openRespecializeCharacterDialog">Respecialize</b-button>
      <b-button type="is-warning" icon-left="baby"
                expanded :disabled="character.level < 31"
                @click="openRetireCharacterDialog">Retire</b-button>
      <b-button type="is-danger" icon-left="trash" expanded @click="openDeleteCharacterDialog">Delete</b-button>
    </div>

    <b-modal :active.sync="isReplaceItemModalActive" scroll="keep">
      <div class="columns is-marginless replace-item-modal">
        <div class="column" v-if="itemToReplace">
          <h3>Replace <strong>{{itemToReplace.name}}</strong></h3>
          <item-properties :item="itemToReplace" />
        </div>
        <div class="column owned-items">
          <div v-if="fittingOwnedItems.length" class="columns is-multiline">
            <div class="column is-narrow owned-item" v-for="ownedItem in fittingOwnedItems"
                 v-bind:key="ownedItem.id" @click="selectedItem = ownedItem">
              <figure class="image">
                <img :src="itemImage(ownedItem)" alt="item image" />
              </figure>
              <h4>{{ownedItem.name}}</h4>
              <item-properties :item="ownedItem" />
            </div>
          </div>
          <div v-else>You don't own any item for this type.</div>
        </div>
        <div class="column" v-if="selectedItem">
          <h3>Replace with <strong>{{selectedItem.name}}</strong></h3>
          <div class="content">
            <item-properties :item="selectedItem" />
            <b-button size="is-medium" icon-left="check" expanded @click="confirmItemSelection" />
          </div>
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
import Item from '@/models/item';
import { getCharacterItemFromSlot } from '@/services/characters-service';
import { filterItemsFittingInSlot } from '@/services/item-service';
import { notify } from '@/services/notifications-service';
import CharacterStatsComponent from '@/components/CharacterStatsComponent.vue';

@Component({
  components: { CharacterStatsComponent, ItemProperties },
})
export default class CharacterComponent extends Vue {
  @Prop(Object) readonly character: Character;

  // modal stuff
  itemSlot = ItemSlot;
  isReplaceItemModalActive = false;
  itemToReplace: Item | null = null;
  itemToReplaceSlot: ItemSlot | null = null;
  selectedItem: Item | null = null;

  get fittingOwnedItems(): Item[] {
    return this.itemToReplaceSlot === null
      ? []
      : filterItemsFittingInSlot(userModule.ownedItems, this.itemToReplaceSlot)
        .filter(i => this.itemToReplace === null || i.id !== this.itemToReplace.id);
  }

  itemImage(item: Item) {
    return `${process.env.BASE_URL}items/${item.mbId}.png`;
  }

  openRespecializeCharacterDialog() {
    this.$buefy.dialog.confirm({
      title: 'Respecialize character',
      message: `Are you sure you want to respecialize your character ${this.character.name} lvl. ${this.character.level}? This action cannot be undone.`,
      confirmText: 'Respecialize Character',
      type: 'is-danger',
      hasIcon: true,
      onConfirm: () => {
        userModule.respecializeCharacter(this.character);
        notify('Character respecialized');
      },
    });
  }

  openRetireCharacterDialog() {
    this.$buefy.dialog.confirm({
      title: 'Retiring character',
      message: `Are you sure you want to retire your character ${this.character.name} lvl. ${this.character.level}? This action cannot be undone.`,
      confirmText: 'Retire',
      type: 'is-warning',
      hasIcon: true,
      onConfirm: () => {
        userModule.retireCharacter(this.character);
        notify('Character retired');
      },
    });
  }

  openDeleteCharacterDialog() {
    this.$buefy.dialog.confirm({
      title: 'Deleting character',
      message: `Are you sure you want to delete your character ${this.character.name} lvl. ${this.character.level}? This action cannot be undone.`,
      confirmText: 'Delete Character',
      type: 'is-danger',
      hasIcon: true,
      onConfirm: () => {
        userModule.deleteCharacter(this.character);
        notify('Character deleted');
      },
    });
  }

  openReplaceItemModal(slot: ItemSlot) {
    this.itemToReplace = getCharacterItemFromSlot(this.character.items, slot);
    this.itemToReplaceSlot = slot;
    this.selectedItem = null;
    if (userModule.ownedItems.length === 0) {
      userModule.getOwnedItems();
    }

    this.isReplaceItemModalActive = true;
  }

  confirmItemSelection() {
    userModule.replaceItem({ character: this.character, slot: this.itemToReplaceSlot!, item: this.selectedItem! });
    this.isReplaceItemModalActive = false;
  }
}
</script>

<style scoped lang="scss">
  .item-boxes {
    background-image: url("../assets/body-silhouette.svg");
    background-repeat: no-repeat;
    background-size: 190px;
    background-position: 180px 0px;
    padding-bottom: 8px; // so the silhouette's feet ain't cropped
  }

  .horse-column {
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

  .owned-items {
    overflow: auto;

    & > .columns {
      justify-content: center;
    }
  }

  .owned-item {
    width: 256px;
    cursor: pointer;

    &:hover {
      background-color: #fafafa; // TODO: use bulma variable
    }
  }
</style>
