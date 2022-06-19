import { Action, getModule, Module, Mutation, VuexModule } from 'vuex-module-decorators';
import store from '@/store';
import * as itemService from '@/services/item-service';
import Item from '@/models/item';

@Module({ store, dynamic: true, name: 'item' })
class ItemModule extends VuexModule {
  items: Item[] = [];

  @Mutation
  setItems(items: Item[]) {
    this.items = items;
  }

  @Action({ commit: 'setItems' })
  getItems() {
    return itemService.getItems();
  }
}

export default getModule(ItemModule);
