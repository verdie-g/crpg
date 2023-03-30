import {
  FontAwesomeIcon,
  FontAwesomeLayers,
  FontAwesomeLayersText,
} from '@fortawesome/vue-fontawesome';
import { library, type IconDefinition } from '@fortawesome/fontawesome-svg-core';
import {
  Config,
  OButton,
  OField,
  OCheckbox,
  ORadio,
  OSwitch,
  OInput,
  OTable,
  OTableColumn,
  OLoading,
  OTabs,
  OTabItem,
  OIcon,
  OPagination,
  OPaginationButton,
  ONotification,
  OCollapse,
} from '@oruga-ui/oruga-next';
import VueSlider from 'vue-slider-component';

import { type BootModule } from '@/types/boot-module';

export const install: BootModule = app => {
  Object.values(
    import.meta.glob<IconDefinition>('../assets/themes/oruga-tailwind/icons/**/*.ts', {
      eager: true,
      import: 'default',
    })
  ).forEach(ic => {
    library.add(ic);
  });

  app
    .component('OIcon', OIcon)
    .component('OButton', OButton)
    .component('OField', OField)
    .component('OCheckbox', OCheckbox)
    .component('ORadio', ORadio)
    .component('OSwitch', OSwitch)
    .component('OInput', OInput)
    .component('OTable', OTable)
    .component('OTableColumn', OTableColumn)
    .component('OTabs', OTabs)
    .component('OTabItem', OTabItem)
    .component('OLoading', OLoading)
    .component('OPagination', OPagination)
    .component('OPaginationButton', OPaginationButton)
    .component('FontAwesomeIcon', FontAwesomeIcon)
    .component('FontAwesomeLayers', FontAwesomeLayers)
    .component('FontAwesomeLayersText', FontAwesomeLayersText)
    .component('ONotification', ONotification)
    .component('OCollapse', OCollapse)
    .component('VueSlider', VueSlider)
    .use(Config, {
      // https://oruga.io/components/Icon.html
      iconComponent: 'FontAwesomeIcon',
      iconPack: 'crpg',
      customIconPacks: {
        crpg: {
          sizes: {
            default: 'sm',
            '2xs': '2xs',
            xs: 'xs',
            sm: 'sm',
            lg: 'lg',
            xl: 'xl',
            '2xl': '2xl',
            '3x': '3x',
            '4x': '4x',
            '5x': '5x',
          },
          iconPrefix: 'fa-',
          internalIcons: {
            'close-circle': 'close',
          },
        },
      },
      useHtml5Validation: false,
    });
};
