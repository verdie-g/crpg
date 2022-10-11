import { type BootModule } from '@/types/boot-module';

import {
  Config,
  OButton,
  OField,
  OCheckbox,
  ORadio,
  OSwitch,
  OInput,
  ODropdown,
  ODropdownItem,
  OTable,
  OTableColumn,
  OLoading,
  OTabs,
  OTabItem,
} from '@oruga-ui/oruga-next';

export const install: BootModule = app => {
  app.component('OButton', OButton);

  app.component('OField', OField);
  app.component('OCheckbox', OCheckbox);
  app.component('ORadio', ORadio);
  app.component('OSwitch', OSwitch);

  app.component('OInput', OInput);

  app.component('ODropdown', ODropdown);
  app.component('ODropdownItem', ODropdownItem);

  app.component('OTable', OTable);
  app.component('OTableColumn', OTableColumn);

  app.component('OTabs', OTabs);
  app.component('OTabItem', OTabItem);

  app.component('OLoading', OLoading);

  app.use(Config, {});
};
