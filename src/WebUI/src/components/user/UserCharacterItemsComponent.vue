<template>
  <div class="columns container is-fluid">
    <div class="column character-items">
      <div class="columns item-boxes">
        <div class="column is-narrow gear-column">
          <b-tooltip
            class="box item-box"
            :active="getTooltip(userItemsBySlot[itemSlot.Head]) !== null"
            type="is-dark"
            multilined
            position="is-left"
          >
            <div @click="navigateToShopItem(userItemsBySlot[itemSlot.Head])">
              <img
                v-if="userItemsBySlot[itemSlot.Head]"
                :src="userItemImage(userItemsBySlot[itemSlot.Head])"
                alt="Head armor"
              />
              <img
                v-else
                src="../../assets/head-armor.png"
                alt="Head armor"
                class="item-placeholder"
              />
            </div>
            <template v-slot:content>
              <div v-html="getTooltip(userItemsBySlot[itemSlot.Head])" />
            </template>
          </b-tooltip>

          <b-tooltip
            :active="getTooltip(userItemsBySlot[itemSlot.Shoulder]) !== null"
            type="is-dark"
            class="box item-box"
            multilined
            position="is-left"
          >
            <div @click="navigateToShopItem(userItemsBySlot[itemSlot.Shoulder])">
              <img
                v-if="userItemsBySlot[itemSlot.Shoulder]"
                :src="userItemImage(userItemsBySlot[itemSlot.Shoulder])"
                alt="Shoulder"
              />
              <img v-else src="../../assets/cape.png" alt="Shoulder" class="item-placeholder" />
            </div>
            <template v-slot:content>
              <div v-html="getTooltip(userItemsBySlot[itemSlot.Shoulder])" />
            </template>
          </b-tooltip>
          <b-tooltip
            :active="getTooltip(userItemsBySlot[itemSlot.Body]) !== null"
            type="is-dark"
            class="box item-box"
            multilined
            position="is-left"
          >
            <div @click="navigateToShopItem(userItemsBySlot[itemSlot.Body])">
              <img
                v-if="userItemsBySlot[itemSlot.Body]"
                :src="userItemImage(userItemsBySlot[itemSlot.Body])"
                alt="Body armor"
              />
              <img
                v-else
                src="../../assets/body-armor.png"
                alt="Body armor"
                class="item-placeholder"
              />
            </div>
            <template v-slot:content>
              <div v-html="getTooltip(userItemsBySlot[itemSlot.Body])" />
            </template>
          </b-tooltip>
          <b-tooltip
            :active="getTooltip(userItemsBySlot[itemSlot.Hand]) !== null"
            type="is-dark"
            class="box item-box"
            multilined
            position="is-left"
          >
            <div @click="navigateToShopItem(userItemsBySlot[itemSlot.Hand])">
              <img
                v-if="userItemsBySlot[itemSlot.Hand]"
                :src="userItemImage(userItemsBySlot[itemSlot.Hand])"
                alt="Hand armor"
              />
              <img
                v-else
                src="../../assets/hand-armor.png"
                alt="Hand armor"
                class="item-placeholder"
              />
            </div>
            <template v-slot:content>
              <div v-html="getTooltip(userItemsBySlot[itemSlot.Hand])" />
            </template>
          </b-tooltip>
          <b-tooltip
            :active="getTooltip(userItemsBySlot[itemSlot.Leg]) !== null"
            type="is-dark"
            class="box item-box"
            multilined
            position="is-left"
          >
            <div @click="navigateToShopItem(userItemsBySlot[itemSlot.Leg])">
              <img
                v-if="userItemsBySlot[itemSlot.Leg]"
                :src="userItemImage(userItemsBySlot[itemSlot.Leg])"
                alt="Leg armor"
              />
              <img
                v-else
                src="../../assets/leg-armor.png"
                alt="Leg armor"
                class="item-placeholder"
              />
            </div>
            <template v-slot:content>
              <div v-html="getTooltip(userItemsBySlot[itemSlot.Leg])" />
            </template>
          </b-tooltip>
        </div>

        <div class="column is-narrow mount-column">
          <b-tooltip
            :active="getTooltip(userItemsBySlot[itemSlot.MountHarness]) !== null"
            type="is-dark"
            class="box item-box"
            multilined
            position="is-top"
          >
            <div @click="navigateToShopItem(userItemsBySlot[itemSlot.MountHarness])">
              <img
                v-if="userItemsBySlot[itemSlot.MountHarness]"
                :src="userItemImage(userItemsBySlot[itemSlot.MountHarness])"
                alt="Mount harness"
              />
              <img
                v-else
                src="../../assets/horse-harness.png"
                alt="Horse harness"
                class="item-placeholder"
              />
            </div>
            <template v-slot:content>
              <div v-html="getTooltip(userItemsBySlot[itemSlot.MountHarness])" />
            </template>
          </b-tooltip>
          <b-tooltip
            :active="getTooltip(userItemsBySlot[itemSlot.Mount]) !== null"
            type="is-dark"
            class="box item-box"
            multilined
            position="is-top"
          >
            <div @click="navigateToShopItem(userItemsBySlot[itemSlot.Mount])">
              <img
                v-if="userItemsBySlot[itemSlot.Mount]"
                :src="userItemImage(userItemsBySlot[itemSlot.Mount])"
                alt="Mount"
              />
            </div>
            <template v-slot:content>
              <div v-html="getTooltip(userItemsBySlot[itemSlot.Mount])" />
            </template>
          </b-tooltip>
        </div>

        <div class="column is-narrow weapon-column">
          <b-tooltip
            :active="getTooltip(userItemsBySlot[itemSlot.Weapon0]) !== null"
            type="is-dark"
            class="box item-box"
            multilined
            position="is-right"
          >
            <div @click="navigateToShopItem(userItemsBySlot[itemSlot.Weapon0])">
              <img
                v-if="userItemsBySlot[itemSlot.Weapon0]"
                :src="userItemImage(userItemsBySlot[itemSlot.Weapon0])"
                alt="First weapon"
              />
            </div>
            <template v-slot:content>
              <div v-html="getTooltip(userItemsBySlot[itemSlot.Weapon0])" />
            </template>
          </b-tooltip>
          <b-tooltip
            :active="getTooltip(userItemsBySlot[itemSlot.Weapon1]) !== null"
            type="is-dark"
            class="box item-box"
            multilined
            position="is-right"
          >
            <div @click="navigateToShopItem(userItemsBySlot[itemSlot.Weapon1])">
              <img
                v-if="userItemsBySlot[itemSlot.Weapon1]"
                :src="userItemImage(userItemsBySlot[itemSlot.Weapon1])"
                alt="Second weapon"
              />
            </div>
            <template v-slot:content>
              <div v-html="getTooltip(userItemsBySlot[itemSlot.Weapon1])" />
            </template>
          </b-tooltip>
          <b-tooltip
            :active="getTooltip(userItemsBySlot[itemSlot.Weapon2]) !== null"
            type="is-dark"
            class="box item-box"
            multilined
            position="is-right"
          >
            <div @click="navigateToShopItem(userItemsBySlot[itemSlot.Weapon2])">
              <img
                v-if="userItemsBySlot[itemSlot.Weapon2]"
                :src="userItemImage(userItemsBySlot[itemSlot.Weapon2])"
                alt="Third weapon"
              />
            </div>
            <template v-slot:content>
              <div v-html="getTooltip(userItemsBySlot[itemSlot.Weapon2])" />
            </template>
          </b-tooltip>
          <b-tooltip
            :active="getTooltip(userItemsBySlot[itemSlot.Weapon3]) !== null"
            type="is-dark"
            multilined
            position="is-right"
          >
            <div
              class="box item-box"
              @click="navigateToShopItem(userItemsBySlot[itemSlot.Weapon3])"
            >
              <img
                v-if="userItemsBySlot[itemSlot.Weapon3]"
                :src="userItemImage(userItemsBySlot[itemSlot.Weapon3])"
                alt="Fourth Weapon"
              />
            </div>
            <template v-slot:content>
              <div v-html="getTooltip(userItemsBySlot[itemSlot.Weapon3])" />
            </template>
          </b-tooltip>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import Character from '@/models/character';
import ItemSlot from '@/models/item-slot';
import EquippedItem from '@/models/equipped-item';
import UserItem from '@/models/user-item';
import router from '@/router';
import { getItemDescriptor } from '@/services/item-service';
import ItemType from '@/models/item-type';
import ItemWeaponComponent from '@/models/item-weapon-component';

@Component
export default class UserCharacterItemsComponent extends Vue {
  @Prop(Object) readonly character: Character;
  itemSlot = ItemSlot;

  get characterEquippedItems(): EquippedItem[] | null {
    // TODO
    const item1 = {
      slot: 'Head',
      userItem: {
        id: 12,
        rank: 0,
        baseItem: {
          type: ItemType.HeadArmor,
          weapons: [] as ItemWeaponComponent[],
          armor: {
            headArmor: 22,
            legArmor: 0,
            armArmor: 0,
            bodyArmor: 0,
          },
          mount: null,
          name: 'Practice Longbow',
          price: 255,
          weight: 0.55,
        },
      },
    } as EquippedItem;
    // return userModule.characterEquippedItems(this.character.id);
    return [item1];
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

  userItemImage(userItem: UserItem): string {
    return `${process.env.BASE_URL}items/${userItem.baseItem.id}.png`;
  }

  getTooltip(userItem: UserItem): string | null {
    if (!userItem || !userItem.id) return null;

    let result = "<table style='width:100%'>";

    result += '<tr><td>Price:</td><td>' + userItem.baseItem.price + '</td></tr>';
    const itemDesc = getItemDescriptor(userItem.baseItem, userItem.rank);
    itemDesc.fields.forEach(field => {
      if (field[0] !== 'Culture' && field[0] !== 'Type')
        result += '<tr><td>' + field[0] + ':</td><td>' + field[1] + '</td></tr>';
    });
    itemDesc.modes
      .map(mode => mode.fields)
      .forEach(field => {
        result += '<tr><td>' + field[0] + ':</td><td>' + field[1] + '</td></tr>';
      });

    return (result += '</table>');
  }

  navigateToShopItem(userItem: UserItem): void {
    if (userItem.baseItem && userItem.baseItem.name) {
      router.push({
        name: 'shop',
        query: {
          searchQuery: userItem.baseItem.name,
        },
      });
    }
  }
}
</script>

<style scoped lang="scss">
.item-boxes {
  background-image: url('../../assets/body-silhouette.svg');
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
</style>
