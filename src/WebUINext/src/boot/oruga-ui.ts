import { type BootModule } from '@/types/boot-module';

import {
  Config,
  OButton,
  OField,
  OCheckbox,
  OInput,
  ODropdown,
  ODropdownItem,
} from '@oruga-ui/oruga-next';

export const install: BootModule = app => {
  app.component('OButton', OButton);

  app.component('OField', OField);
  app.component('OCheckbox', OCheckbox);
  app.component('OInput', OInput);

  app.component('ODropdown', ODropdown);
  app.component('ODropdownItem', ODropdownItem);

  app.use(Config, {});
};
