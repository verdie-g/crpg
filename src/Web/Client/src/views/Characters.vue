<template>
  <div class="section">
    <div class="columns">
      <div class="column is-narrow is-paddingless" style="width: 200px;">
        <div class="list is-hoverable" v-if="characters.length">
          <a class="list-item" v-for="character in characters" v-bind:key="character.id"
             v-bind:class="{ 'is-active': selectedCharacter && character.id === selectedCharacter.id }" @click="selectCharacter(character)">
            <p class="title is-5">{{character.name}}</p>
            <p class="subtitle is-6">Level {{character.level}}</p>
          </a>
        </div>
      </div>

      <div class="column">
        <div class="columns container is-fluid" v-if="selectedCharacter">

          <div class="column content character-stats">
            <div class="character-name">
              <h1>{{selectedCharacter.name}}</h1>
              <b-icon icon="pencil-alt" class="character-name-edit" @click.native="openEditCharacterDialog" />
            </div>

            <p>
              <strong>Level:</strong> {{selectedCharacter.level}}<br />
              <strong>Experience:</strong> {{selectedCharacter.experience}}<br />
              <strong>Next Level:</strong> {{selectedCharacter.nextLevelExperience - selectedCharacter.experience}}
            </p>

            <h2>Attributes (0)</h2>
            <p>
              <strong>Strength:</strong>
              <b-icon icon="minus-square" size="is-small" class="stats-decrease-button" />
              <span class="stats-value">28</span>
              <b-icon icon="plus-square" size="is-small" class="stats-increase-button" /><br />

              <strong>Perception:</strong>
              <b-icon icon="minus-square" size="is-small" class="stats-decrease-button" />
              <span class="stats-value">14</span>
              <b-icon icon="plus-square" size="is-small" class="stats-increase-button" /><br />

              <strong>Endurance:</strong>
              <b-icon icon="minus-square" size="is-small" class="stats-decrease-button" />
              <span class="stats-value">12</span>
              <b-icon icon="plus-square" size="is-small" class="stats-increase-button" /><br />
            </p>

            <h2>Skills (0)</h2>
            <p>
              <!-- Mastery of fighting with one-handed weapons either with a shield or without. -->
              <strong>One Handed:</strong>
              <b-icon icon="minus-square" size="is-small" class="stats-decrease-button" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" class="stats-increase-button" /><br />

              <!-- Mastery of fighting with two-handed weapons of average length such as bigger axes and swords. -->
              <strong>Two Handed:</strong>
              <b-icon icon="minus-square" size="is-small" class="stats-decrease-button" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" class="stats-increase-button" /><br />

              <!-- Mastery of the spear, lance, staff and other polearms, both one-handed and two-handed. -->
              <strong>Polearm:</strong>
              <b-icon icon="minus-square" size="is-small" class="stats-decrease-button" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" class="stats-increase-button" /><br />

              <!-- Familiarity with bows and physical conditioning to shoot with them effectively. -->
              <strong>Bow:</strong>
              <b-icon icon="minus-square" size="is-small" class="stats-decrease-button" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" class="stats-increase-button" /><br />

              <!-- Mastery of throwing projectiles accurately and with power. -->
              <strong>Throwing:</strong>
              <b-icon icon="minus-square" size="is-small" class="stats-decrease-button" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" class="stats-increase-button" /><br />

              <!-- Knowledge of operating and maintaining crossbows. -->
              <strong>Crossbow:</strong>
              <b-icon icon="minus-square" size="is-small" class="stats-decrease-button" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" class="stats-increase-button" /><br />

              <!-- The ability to control a horse, to keep your balance when it moves suddenly or unexpectedly. -->
              <strong>Riding:</strong>
              <b-icon icon="minus-square" size="is-small" class="stats-decrease-button" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" class="stats-increase-button" /><br />

              <!-- Physical fitness, speed and balance. -->
              <strong>Athletics:</strong>
              <b-icon icon="minus-square" size="is-small" class="stats-decrease-button" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" class="stats-increase-button" />
            </p>

            <div class="stats-buttons">
              <b-button size="is-medium" icon-left="undo" title="Reset changes" />
              <b-button size="is-medium" icon-left="check" title="Commit changes" />
            </div>
          </div>

          <div class="column character-items">
            <div class="columns item-boxes">
              <div class="column is-narrow gear-column">
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Head)">
                  <img v-if="selectedCharacter.headItem" :src="selectedCharacter.headItem.image" alt="Head armor" />
                  <img v-else src="../assets/head-armor.png" alt="Head armor" class="item-placeholder" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Cape)">
                  <img v-if="selectedCharacter.capeItem" :src="selectedCharacter.capeItem.image" alt="Cape" />
                  <img v-else src="../assets/cape.png" alt="Cape" class="item-placeholder" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Body)">
                  <img v-if="selectedCharacter.bodyItem" :src="selectedCharacter.bodyItem.image" alt="Body armor" />
                  <img v-else src="../assets/body-armor.png" alt="Body armor" class="item-placeholder" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Hand)">
                  <img v-if="selectedCharacter.handItem" :src="selectedCharacter.handItem.image" alt="Hand armor" />
                  <img v-else src="../assets/hand-armor.png" alt="Hand armor" class="item-placeholder" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Leg)">
                  <img v-if="selectedCharacter.legItem" :src="selectedCharacter.legItem.image" alt="Leg armor" />
                  <img v-else src="../assets/leg-armor.png" alt="Leg armor" class="item-placeholder" />
                </div>
              </div>

              <div class="column is-narrow horse-column">
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.HorseHarness)">
                  <img v-if="selectedCharacter.horseHarnessItem" :src="selectedCharacter.horseHarnessItem.image" alt="Horse harness" />
                  <img v-else src="../assets/horse-harness.png" alt="Horse harness" class="item-placeholder" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Horse)">
                  <img v-if="selectedCharacter.horseItem" :src="selectedCharacter.horseItem.image" alt="Horse" />
                </div>
              </div>

              <div class="column is-narrow weapon-column">
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon1)">
                  <img v-if="selectedCharacter.weapon1Item" :src="selectedCharacter.weapon1Item.image" alt="First weapon" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon2)">
                  <img v-if="selectedCharacter.weapon2Item" :src="selectedCharacter.weapon2Item.image" alt="Second weapon" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon3)">
                  <img v-if="selectedCharacter.weapon3Item" :src="selectedCharacter.weapon3Item.image" alt="Third weapon" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon4)">
                  <img v-if="selectedCharacter.weapon4Item" :src="selectedCharacter.weapon4Item.image" alt="Fourth Weapon" />
                </div>
              </div>
            </div>

            <b-button type="is-warning" icon-left="angle-double-down" expanded disabled>Respecialize</b-button>
            <b-button type="is-warning" icon-left="baby" expanded disabled>Retire</b-button>
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
                      <img :src="ownedItem.image" alt="item image" />
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

        <div v-else> <!-- if no character -->
          To create a character, simply connect to one of the cRPG servers.
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import ItemProperties from '@/components/ItemProperties.vue';
import userModule from '@/store/user-module';
import Character from '@/models/character';
import ItemSlot from '@/models/item-slot';
import Item from '@/models/item';
import { getCharacterItemFromSlot } from '@/services/characters-service';
import { filterItemsFittingInSlot } from '@/services/item-service';
import { notify } from '@/services/notifications-service';

@Component({
  components: { ItemProperties },
})
export default class Characters extends Vue {
    selectedCharacterId: number = -1;

    // modal stuff
    itemSlot = ItemSlot;
    isReplaceItemModalActive: boolean = false;
    itemToReplace: Item | null = null;
    itemToReplaceSlot: ItemSlot | null = null;
    selectedItem: Item | null = null;

    get characters() {
      return userModule.characters;
    }

    get selectedCharacter(): Character | null {
      return this.selectedCharacterId === -1
        ? null
        : this.characters.find(c => c.id === this.selectedCharacterId)!;
    }

    get fittingOwnedItems() : Item[] {
      return this.itemToReplaceSlot === null
        ? []
        : filterItemsFittingInSlot(userModule.ownedItems, this.itemToReplaceSlot)
          .filter(i => this.itemToReplace === null || i.id !== this.itemToReplace.id);
    }

    created() {
      userModule.getCharacters().then(c => this.selectedCharacterId = c.length > 0 ? c[0].id : -1);
    }

    selectCharacter(character: Character) {
      this.selectedCharacterId = character.id;
    }

    openEditCharacterDialog() {
      this.$buefy.dialog.prompt({
        message: 'New name',
        inputAttrs: {
          value: this.selectedCharacter!.name,
          minlength: 2,
          maxlength: 32,
        },
        trapFocus: true,
        onConfirm: (newName) => {
          userModule.renameCharacter({ character: this.selectedCharacter!, newName });
          notify('Character renamed');
        },
      });
    }

    openDeleteCharacterDialog() {
      this.$buefy.dialog.confirm({
        title: 'Deleting character',
        message: `Are you sure you want to delete your character ${this.selectedCharacter!.name} lvl. ${this.selectedCharacter!.level}? This action cannot be undone.`,
        confirmText: 'Delete Character',
        type: 'is-danger',
        hasIcon: true,
        onConfirm: () => {
          userModule.deleteCharacter(this.selectedCharacter!);
          notify('Character deleted');
        },
      })
    }

    openReplaceItemModal(slot: ItemSlot) {
      this.itemToReplace = getCharacterItemFromSlot(this.selectedCharacter!, slot);
      this.itemToReplaceSlot = slot;
      this.selectedItem = null;
      if (userModule.ownedItems.length === 0) {
        userModule.getOwnedItems();
      }

      this.isReplaceItemModalActive = true;
    }

    confirmItemSelection() {
      userModule.replaceItem({ character: this.selectedCharacter!, slot: this.itemToReplaceSlot!, item: this.selectedItem! });
      this.isReplaceItemModalActive = false;
    }
}
</script>

<style scoped lang="scss">
  $stats-name-width: 150px;
  $stats-value-width: 40px;

  .character-name {
    display: flex;

    .character-name-edit {
      margin-left: 8px;
      display: none;
      cursor: pointer;
    }

    &:hover {
      .character-name-edit {
        display: inline;
      }
    }
  }

  .character-stats {
    strong {
      display: inline-block;
      width: $stats-name-width;
    }
  }

  .stats-value {
    display: inline-block;
    width: $stats-value-width;
    text-align: center;
  }

  .stats-decrease-button, .stats-increase-button {
    cursor: pointer;
  }

  .stats-buttons {
    margin-left: $stats-name-width + 16 + ($stats-value-width / 2) - 45;
  }

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
