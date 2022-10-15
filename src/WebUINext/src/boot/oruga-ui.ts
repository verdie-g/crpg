import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { library, type IconDefinition } from '@fortawesome/fontawesome-svg-core';
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
  OIcon,
  OPagination,
  OPaginationButton,
} from '@oruga-ui/oruga-next';

import { type BootModule } from '@/types/boot-module';

export const install: BootModule = app => {
  Object.values(
    import.meta.glob<IconDefinition>('../assets/themes/oruga-tailwind-favoras/icons/*.ts', {
      eager: true,
    })
  ).forEach(icon =>
    // ref https://dev.to/astagi/add-custom-icons-to-font-awesome-4m67
    library.add(icon)
  );

  app.component('OIcon', OIcon);

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

  app.component('OPagination', OPagination);
  app.component('OPaginationButton', OPaginationButton);

  // @ts-ignore
  app.component('FontAwesomeIcon', FontAwesomeIcon);

  app.use(Config, {
    // https://oruga.io/components/Icon.html
    iconComponent: 'FontAwesomeIcon',
    iconPack: 'crpg',
    customIconPacks: {
      crpg: {
        sizes: {
          default: 'size_md',
          small: 'size_sm',
          medium: 'size_md',
          large: 'size_lg',
        },
        iconPrefix: 'fa-',
        internalIcons: {
          // TODO: multiplt path, stacked, layer ... @fortawesome/fontawesome-svg-core
          // 'eye-off': 'eye-off',
          // times: 'close-outline',
          // 'close-circle': 'close-circle-outline',
        },
      },
    },
  });
};
