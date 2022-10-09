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

  app.use(Config, {
    // https://oruga.io/components/Button.html#class-props
    button: /*@tw*/ {
      override: true,
      rootClass: 'btn',
      sizeClass: 'btn_size-',
      variantClass: 'btn_variant-',
      outlinedClass: 'btn_outlined-',
      elementsWrapperClass: 'btn__wrapper',
      labelClass: 'btn__label',
      roundedClass: '',
      invertedClass: '',
      iconClass: '',
      iconLeftClass: '',
      iconRightClass: '',
      expandedClass: 'btn_expanded',
      disabledClass: 'btn_disabled',
    },

    // https://oruga.io/components/Field.html#class-props
    field: /*@tw*/ {
      override: true,
      addonsClass: '',
      bodyClass: 'field__body',
      bodyHorizontalClass: 'field__body field__body_horizontal',
      filledClass: '',
      focusedClass: '',
      groupMultilineClass: '',
      groupedClass: 'field_grouped',
      horizontalClass: 'field field_horizontal',
      labelClass: 'field__label',
      labelHorizontalClass: 'field__label_horizontal',
      labelSizeClass: '',
      messageClass: '',
      mobileClass: '',
      rootClass: 'field',
      variantLabelClass: '',
      variantMessageClass: '',
    },

    // https://oruga.io/components/Checkbox.html#class-props
    checkbox: /*@tw*/ {
      override: true,
      checkCheckedClass: 'checkbox__check_checked',
      checkClass: 'checkbox__check',
      checkIndeterminateClass: '',
      checkedClass: '',
      disabledClass: '',
      labelClass: 'checkbox__label',
      rootClass: 'checkbox',
      sizeClass: '',
      variantClass: '',
    },

    // https://oruga.io/components/Input.html#class-props
    input: /*@tw*/ {
      override: true,
      counterClass: '',
      expandedClass: 'input_expanded',
      iconLeftClass: '',
      iconLeftSpaceClass: '',
      iconRightClass: '',
      iconRightSpaceClass: '',
      inputClass: 'input__input',
      rootClass: 'input',
      roundedClass: '',
      sizeClass: 'input__input_size-',
      variantClass: 'input__input_variant-',
    },

    dropdown: /*@tw*/ {
      override: true,
      menuClass: 'dropdown-menu',
      itemClass: 'dropdown-item',
      itemActiveClass: 'dropdown-item-active',
    },
  });
};
