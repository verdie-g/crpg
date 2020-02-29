<template>
  <div class="section">
    <div class="columns">
      <div class="column is-narrow is-paddingless" style="width: 200px;">
        <div class="list is-hoverable">
          <a class="list-item" v-for="character in characters" v-bind:key="character.id"
             v-bind:class="{ 'is-active': character === selectedCharacter }" @click="selectCharacter(character)">
            <p class="title is-5">{{character.name}}</p>
            <p class="subtitle is-6">Level {{character.level}}</p>
          </a>
        </div>
      </div>

      <div class="column">
        <div class="columns" v-if="selectedCharacter">

          <div class="column content character-stats">
            <h1>{{selectedCharacter.name}}</h1>
            <p>
              <strong>Level:</strong> {{selectedCharacter.level}}<br />
              <strong>Experience:</strong> {{selectedCharacter.experience}}<br />
              <strong>Next Level:</strong> {{selectedCharacter.nextLevelExperience - selectedCharacter.experience}}
            </p>

            <h2>Attributes (0)</h2>
            <p>
              <strong>Strength:</strong>
              <b-icon icon="minus-square" size="is-small" />
              <span class="stats-value">28</span>
              <b-icon icon="plus-square" size="is-small" /><br />

              <strong>Perception:</strong>
              <b-icon icon="minus-square" size="is-small" />
              <span class="stats-value">14</span>
              <b-icon icon="plus-square" size="is-small" /><br />

              <strong>Endurance:</strong>
              <b-icon icon="minus-square" size="is-small" />
              <span class="stats-value">12</span>
              <b-icon icon="plus-square" size="is-small" /><br />
            </p>

            <h2>Skills (0)</h2>
            <p>
              <!-- Mastery of fighting with one-handed weapons either with a shield or without. -->
              <strong>One Handed:</strong>
              <b-icon icon="minus-square" size="is-small" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" /><br />

              <!-- Mastery of fighting with two-handed weapons of average length such as bigger axes and swords. -->
              <strong>Two Handed:</strong>
              <b-icon icon="minus-square" size="is-small" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" /><br />

              <!-- Mastery of the spear, lance, staff and other polearms, both one-handed and two-handed. -->
              <strong>Polearm:</strong>
              <b-icon icon="minus-square" size="is-small" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" /><br />

              <!-- Familiarity with bows and physical conditioning to shoot with them effectively. -->
              <strong>Bow:</strong>
              <b-icon icon="minus-square" size="is-small" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" /><br />

              <!-- Mastery of throwing projectiles accurately and with power. -->
              <strong>Throwing:</strong>
              <b-icon icon="minus-square" size="is-small" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" /><br />

              <!-- Knowledge of operating and maintaining crossbows. -->
              <strong>Crossbow:</strong>
              <b-icon icon="minus-square" size="is-small" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" /><br />

              <!-- The ability to control a horse, to keep your balance when it moves suddenly or unexpectedly. -->
              <strong>Riding:</strong>
              <b-icon icon="minus-square" size="is-small" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" /><br />

              <!-- Physical fitness, speed and balance. -->
              <strong>Athletics:</strong>
              <b-icon icon="minus-square" size="is-small" />
              <span class="stats-value">0</span>
              <b-icon icon="plus-square" size="is-small" />
            </p>

            <b-button size="is-medium" icon-left="undo" title="Reset changes" />
            <b-button size="is-medium" icon-left="check" title="Commit changes" />
          </div>

          <div class="column character-items">
            <div class="columns item-boxes">
              <div class="column is-narrow gear-column">
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Head)">
                  <img src="../assets/head-armor.png" alt="Head armor" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Cape)">
                  <img src="../assets/cape.png" alt="Cape" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Body)">
                  <img src="../assets/body-armor.png" alt="Body armor" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Hand)">
                  <img src="../assets/hand-armor.png" alt="Hand armor" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Leg)">
                  <img src="../assets/leg-armor.png" alt="Leg armor" />
                </div>
              </div>

              <div class="column is-narrow horse-column">
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.HorseHarness)">
                  <img src="../assets/horse-harness.png" alt="Horse harness" />
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Horse)">
                </div>
              </div>

              <div class="column is-narrow weapon-column">
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon1)">
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon2)">
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon3)">
                </div>
                <div class="box item-box" @click="openReplaceItemModal(itemSlot.Weapon4)">
                </div>
              </div>
            </div>
          </div>

          <b-modal :active.sync="isReplaceItemModalActive" scroll="keep">
            <div class="columns is-marginless replace-item-modal">
              <div class="column" v-if="itemToReplace">
                <h3>Replace {{itemToReplace.name}}</h3>
              </div>
              <div class="column">
                <div class="columns is-multiline owned-items">
                  <div class="column is-narrow owned-item" v-for="ownedItem in fittingOwnedItems" v-bind:key="ownedItem.id"
                       @click="selectedItem = ownedItem">
                    <figure class="image">
                      <img :src="ownedItem.image" alt="item image" />
                    </figure>
                    <h4>{{ownedItem.name}}</h4>
                    <p>
                      Other<br />
                      important<br />
                      stuff
                    </p>
                  </div>
                </div>
              </div>
              <div class="column" v-if="selectedItem">
                <h3>Replace with <strong>{{selectedItem.name}}</strong></h3>
                <div class="content">
                  <div v-for="prop in getItemProperties(selectedItem).common">
                    {{prop[0]}}: {{prop[1]}}
                  </div>
                  <b-tabs v-if="getItemProperties(selectedItem).primary" class="weapon-tabs">
                    <b-tab-item label="Primary">
                      <div v-for="prop in getItemProperties(selectedItem).primary">
                        {{prop[0]}}: {{prop[1]}}<br />
                      </div>
                      <b-taglist class="flag-tags">
                        <b-tag v-for="flag in getItemProperties(selectedItem).primaryFlags" type="is-info">{{flag}}</b-tag>
                      </b-taglist>
                    </b-tab-item>

                    <b-tab-item label="Secondary" v-if="getItemProperties(selectedItem).secondary">
                      <div v-for="prop in getItemProperties(selectedItem).secondary">
                        {{prop[0]}}: {{prop[1]}}<br />
                      </div>
                      <b-taglist class="flag-tags">
                        <b-tag v-for="flag in getItemProperties(selectedItem).secondaryFlags" type="is-info">{{flag}}</b-tag>
                      </b-taglist>
                    </b-tab-item>
                  </b-tabs>

                  <b-button size="is-medium" icon-left="check" expanded @click="confirmItemSelection" />

                </div>
              </div>
            </div>
          </b-modal>

        </div>

        <div v-else> <!-- if no character -->
          Explain how to create a character
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';
import Character from '@/models/character';
import ItemSlot from '@/models/item-slot';
import Item from '@/models/item';
import { getCharacterItemFromSlot } from '@/services/characters-service';
import { getItemProperties, filerItemsFittingInSlot } from '@/services/item-service';

  @Component
export default class Characters extends Vue {
    selectedCharacter: Character | null = null;

    // modal stuff
    itemSlot = ItemSlot;
    isReplaceItemModalActive: boolean = false;
    itemToReplace: Item | null = null;
    itemToReplaceSlot: ItemSlot;
    selectedItem: Item | null = null;

    get characters() {
      return userModule.characters;
    }

    get fittingOwnedItems() : Item[] {
      return filerItemsFittingInSlot(userModule.ownedItems, this.itemToReplaceSlot);
    }

    created() {
      userModule.getCharacters().then(c => this.selectedCharacter = c.length > 0 ? c[0] : null);
    }

    selectCharacter(character: Character) {
      this.selectedCharacter = character;
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
      userModule.replaceItem({ character: this.selectedCharacter!, item: this.selectedItem!, slot: this.itemToReplaceSlot });
      this.isReplaceItemModalActive = false;
    }

    getItemProperties(item: Item) {
      return getItemProperties(item);
    }
}
</script>

<style scoped lang="scss">
  .character-stats {
    strong {
      display: inline-block;
      width: 150px;
    }
  }

  .stats-value {
    display: inline-block;
    width: 40px;
    text-align: center;
  }

  .item-boxes {
    background-image: url("../assets/body-silhouette.svg");
    background-repeat: no-repeat;
    background-size: 215px;
    background-position: 138px 15px;
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
    width: 100px;
    height: 100px;
    cursor: pointer;

    &:hover {
      box-shadow: 0 5px 8px rgba(10, 10, 10, 0.1), 0 0 0 1px rgba(10, 10, 10, 0.1);
    }
  }

  .replace-item-modal {
    background-color: #fff;
  }

  .owned-items {
    justify-content: center;
  }

  .owned-item {
    width: 256px;
    cursor: pointer;

    &:hover {
      background-color: #fafafa; // TODO: use bulma variable
    }
  }
</style>
