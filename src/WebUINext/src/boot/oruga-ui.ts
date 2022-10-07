import { type BootModule } from '@/types/boot-module';

import { Config, OButton, ODropdown, ODropdownItem, OIcon } from '@oruga-ui/oruga-next';

export const install: BootModule = app => {
  app.component('ODropdown', ODropdown);
  app.component('ODropdownItem', ODropdownItem);
  app.component('OButton', OButton);
  app.component('OIcon', OIcon);

  app.use(Config, {
    button: {
      override: true,
      rootClass: 'btn',
      sizeClass: 'btn_size-',
      variantClass: 'btn_variant-',
      outlinedClass: 'btn_outlined-',
      elementsWrapperClass: 'btn__wrapper',
      labelClass: 'btn__label',
    },
    dropdown: {
      override: true,

      menuClass: 'dropdown-menu',
      itemClass: 'dropdown-item',
      itemActiveClass: 'dropdown-item-active',
    },
  });
};
