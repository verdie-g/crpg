import type { Ref } from 'vue';
import { weaponProficiencyCostCoefs } from '@root/data/constants.json';
import type {
  CharacterCharacteristics,
  CharacteristicSectionKey,
  SkillKey,
  CharacteristicKey,
} from '@/models/character';
import { applyPolynomialFunction } from '@/utils/math';
import { mergeObjectWithSum } from '@/utils/object';
import {
  createEmptyCharacteristic,
  createDefaultCharacteristic,
  wppForAgility,
  wppForWeaponMaster,
} from '@/services/characters-service';

interface FormSchema {
  key: CharacteristicSectionKey;
  children: {
    key: CharacteristicKey;
  }[];
}

const formSchema: FormSchema[] = [
  {
    key: 'attributes',
    children: [
      {
        key: 'strength',
      },
      {
        key: 'agility',
      },
    ],
  },
  {
    key: 'skills',
    children: [
      {
        key: 'ironFlesh',
      },
      {
        key: 'powerStrike',
      },
      {
        key: 'powerDraw',
      },
      {
        key: 'powerThrow',
      },
      {
        key: 'athletics',
      },
      {
        key: 'riding',
      },
      {
        key: 'weaponMaster',
      },
      {
        key: 'mountedArchery',
      },
      {
        key: 'shield',
      },
    ],
  },
  {
    key: 'weaponProficiencies',
    children: [
      {
        key: 'oneHanded',
      },
      {
        key: 'twoHanded',
      },
      {
        key: 'polearm',
      },
      {
        key: 'bow',
      },
      {
        key: 'crossbow',
      },
      {
        key: 'throwing',
      },
    ],
  },
];

const characteristicCost = (
  characteristicSectionKey: CharacteristicSectionKey,
  characteristicKey: CharacteristicKey, // TODO:
  characteristic: number
): number => {
  if (characteristicSectionKey === 'weaponProficiencies') {
    return Math.floor(applyPolynomialFunction(characteristic, weaponProficiencyCostCoefs));
  }

  return characteristic;
};

const characteristicRequirementsSatisfied = (
  characteristicSectionKey: CharacteristicSectionKey,
  characteristicKey: CharacteristicKey,
  characteristicValue: number,
  characteristics: CharacterCharacteristics
): boolean => {
  switch (characteristicSectionKey) {
    case 'skills':
      return skillRequirementsSatisfied(
        characteristicKey as SkillKey,
        characteristicValue,
        characteristics
      );
    default:
      return true;
  }
};

const skillRequirementsSatisfied = (
  skillKey: SkillKey,
  skill: number,
  characteristics: CharacterCharacteristics
): boolean => {
  // console.log({ skillKey, skill, characteristics });

  switch (skillKey) {
    case 'ironFlesh':
    case 'powerStrike':
    case 'powerDraw':
    case 'powerThrow':
      return skill <= Math.floor(characteristics.attributes.strength / 3); // TODO: move "3" to constants.json

    case 'athletics':
    case 'riding':
    case 'weaponMaster':
      return skill <= Math.floor(characteristics.attributes.agility / 3);

    case 'mountedArchery':
    case 'shield':
      return skill <= Math.floor(characteristics.attributes.agility / 6);

    /* c8 ignore next 2 */
    default:
      return false;
  }
};

export const useCharacterCharacteristic = (
  characteristicsInitial: Ref<CharacterCharacteristics>
) => {
  const characteristicsDelta = ref<CharacterCharacteristics>(createEmptyCharacteristic());
  const characteristicDefault = ref<CharacterCharacteristics>(createDefaultCharacteristic());

  const characteristics = computed((): CharacterCharacteristics => {
    return Object.entries(characteristicsInitial.value).reduce(
      (
        obj,
        [key, values]: [string | CharacteristicSectionKey, Partial<CharacterCharacteristics>]
      ) => ({
        ...obj,
        [key]: mergeObjectWithSum(obj[key as CharacteristicSectionKey] as any, values as any),
      }),
      { ...characteristicsDelta.value }
    );
  });

  const allSkillsRequirementsSatisfied = computed((): boolean =>
    Object.keys(characteristics.value.skills)
      .filter(skillKey => skillKey !== 'points')
      .every(skillKey => currentSkillRequirementsSatisfied(skillKey as SkillKey))
  );

  const wasChangeMade = computed(
    () =>
      characteristicsDelta.value.attributes.points !== 0 ||
      characteristicsDelta.value.skills.points !== 0 ||
      characteristicsDelta.value.weaponProficiencies.points !== 0
  );

  const isChangeValid = computed(
    (): boolean =>
      characteristics.value.attributes.points >= 0 &&
      characteristics.value.skills.points >= 0 &&
      characteristics.value.weaponProficiencies.points >= 0 &&
      allSkillsRequirementsSatisfied.value
  );

  const canConvertAttributesToSkills = computed(
    (): boolean => characteristics.value.attributes.points >= 1
  );

  const canConvertSkillsToAttributes = computed(
    (): boolean => characteristics.value.skills.points >= 2
  );

  const onInput = (
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey,
    newCharacteristicValue: number
  ) => {
    // console.log('111');

    // const requirementsSatisfied = characteristicRequirementsSatisfied(
    //   characteristicSectionKey,
    //   characteristicKey,
    //   newCharacteristicValue,
    //   characteristics.value
    // );
    // if (!requirementsSatisfied) return;

    // console.log('222');

    const characteristicInitialSection = characteristicsInitial.value![
      characteristicSectionKey
    ] as any;
    const characteristicDeltaSection = characteristicsDelta.value[characteristicSectionKey] as any;
    const characteristicSection = characteristics.value![characteristicSectionKey] as any;

    const oldCharacteristicValue = characteristicSection[characteristicKey];
    // const oldPoints = characteristicSection.points;

    const costToIncrease =
      characteristicCost(characteristicSectionKey, characteristicKey, oldCharacteristicValue) -
      characteristicCost(characteristicSectionKey, characteristicKey, newCharacteristicValue);

    // TODO:
    // console.log('222', { oldPoints, costToIncrease });
    // if (oldPoints + costToIncrease < 0) {
    //   return;
    // }

    characteristicDeltaSection.points += costToIncrease;

    characteristicDeltaSection[characteristicKey] =
      newCharacteristicValue - characteristicInitialSection[characteristicKey];

    if (characteristicKey === 'agility') {
      characteristicsDelta.value.weaponProficiencies.points +=
        wppForAgility(newCharacteristicValue) - wppForAgility(oldCharacteristicValue);
    } else if (characteristicKey === 'weaponMaster') {
      characteristicsDelta.value.weaponProficiencies.points +=
        wppForWeaponMaster(newCharacteristicValue) - wppForWeaponMaster(oldCharacteristicValue);
    }
  };

  // TODO: FIXME iter count, unit
  const onResetField = (
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey
  ) => {
    for (var i = 1; i <= 300; i++) {
      const inputProps = getInputProps(characteristicSectionKey, characteristicKey);
      onInput(characteristicSectionKey, characteristicKey, inputProps.min!);
    }
  };

  const onFullFillField = (
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey
  ) => {
    for (var i = 1; i <= 300; i++) {
      const inputProps = getInputProps(characteristicSectionKey, characteristicKey);
      onInput(characteristicSectionKey, characteristicKey, inputProps.max);
    }
  };

  // TODO: FIXME iter count, unit
  const getInputProps = (
    characteristicSectionKey: CharacteristicSectionKey,
    characteristicKey: CharacteristicKey,
    noLimit = false // TODO: FIXME: need a name, for builder
  ): { modelValue: number; min: number; max: number } => {
    //
    const initialValue = noLimit
      ? (characteristicDefault.value[characteristicSectionKey] as any)[characteristicKey]
      : (characteristicsInitial.value[characteristicSectionKey] as any)[characteristicKey];

    const value = (characteristics.value[characteristicSectionKey] as any)[characteristicKey];
    const points = characteristics.value[characteristicSectionKey].points;

    const costToIncrease =
      characteristicCost(characteristicSectionKey, characteristicKey, value + 1) -
      characteristicCost(characteristicSectionKey, characteristicKey, value);

    const requirementsSatisfied = characteristicRequirementsSatisfied(
      characteristicSectionKey,
      characteristicKey,
      value + 1,
      characteristics.value
    );

    return {
      modelValue: value,
      min: initialValue, // TODO: can to default for builder
      max: value + (costToIncrease <= points && requirementsSatisfied ? 1 : 0),
      // controls: initialPoints !== 0, // hide controls (+/-) if there is no points to give // TODO:
    };
  };

  const currentSkillRequirementsSatisfied = (skillKey: SkillKey): boolean =>
    skillRequirementsSatisfied(
      skillKey,
      characteristics.value.skills[skillKey],
      characteristics.value
    );

  const convertAttributeToSkills = () => {
    characteristicsInitial.value.attributes.points -= 1;
    characteristicsInitial.value.skills.points += 2;
  };

  const convertSkillsToAttribute = () => {
    characteristicsInitial.value.attributes.points += 1;
    characteristicsInitial.value.skills.points -= 2;
  };

  const reset = () => {
    characteristicsDelta.value = createEmptyCharacteristic();
  };

  return {
    characteristics,
    formSchema,
    wasChangeMade,
    isChangeValid,
    canConvertSkillsToAttributes,
    canConvertAttributesToSkills,
    onInput,
    onFullFillField,
    onResetField,
    getInputProps,
    currentSkillRequirementsSatisfied,
    convertAttributeToSkills,
    convertSkillsToAttribute,
    reset,
  };
};
