<template>
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
            <strong>Next Level:</strong> 4214
          </p>

          <h2>Attributes</h2>
          <p>
            <strong>Strength:</strong> 28<br />
            <strong>Perception:</strong> 14<br />
            <strong>Endurance:</strong> 12
          </p>

          <h2>Skills</h2>
          <p>
            <strong>One Handed:</strong> 0<br />  <!-- Mastery of fighting with one-handed weapons either with a shield or without. -->
            <strong>Two Handed:</strong> 0<br /> <!-- Mastery of fighting with two-handed weapons of average length such as bigger axes and swords. -->
            <strong>Polearm:</strong> 0<br /> <!-- Mastery of the spear, lance, staff and other polearms, both one-handed and two-handed. -->
            <strong>Bow:</strong> 0<br /> <!-- Familiarity with bows and physical conditioning to shoot with them effectively. -->
            <strong>Throwing:</strong> 0<br /> <!-- Mastery of throwing projectiles accurately and with power. -->
            <strong>Crossbow:</strong> 0<br /> <!-- Knowledge of operating and maintaining crossbows. -->
            <strong>Riding:</strong> 0<br /> <!-- The ability to control a horse, to keep your balance when it moves suddenly or unexpectedly. -->
            <strong>Athletics:</strong> 0 <!-- Physical fitness, speed and balance. -->
          </p>
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
          <div class="columns replace-item-modal">
            <div class="column" v-if="itemToReplace">
              <h3>Replace {{itemToReplace.name}}</h3>
            </div>
            <div class="column">
              <div class="columns">
                <div class="column" v-for="ownedItem in ownedItems" v-bind:key="ownedItem.id" @click="selectedItem = ownedItem">
                  <figure class="image is-128x128">
                    <img src="https://via.placeholder.com/128x128.png" alt="owned item image" />
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
              <h3>Replace {{selectedItem.name}}</h3>
            </div>
          </div>
        </b-modal>

      </div>

      <div v-else> <!-- if no character -->
        Explain how to create a character
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import userModule from '@/store/user-module';
import Character from '@/models/character';
import ItemSlot from "@/models/item-slot";
import Item from "@/models/item";

  @Component
export default class Characters extends Vue {
    selectedCharacter: Character | null = null;

    // modal stuff
    itemSlot = ItemSlot;
    isReplaceItemModalActive: boolean = false;
    itemToReplace: Item | null = null;
    selectedItem: Item | null = null;

    get characters() {
      return userModule.characters;
    }

    get ownedItems() {
      return userModule.ownedItems;
    }

    created() {
      userModule.getCharacters().then(c => this.selectedCharacter = c.length > 0 ? c[0] : null);
    }

    selectCharacter(character: Character) {
      this.selectedCharacter = character;
    }

    openReplaceItemModal(slot: ItemSlot) {
      if (userModule.ownedItems.length === 0) {
        userModule.getOwnedItems();
      }

      this.isReplaceItemModalActive = true;
    }
}
</script>

<style scoped lang="scss">
  .character-stats {
    strong {
      display: inline-block;
      width: 200px;
    }
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
</style>
