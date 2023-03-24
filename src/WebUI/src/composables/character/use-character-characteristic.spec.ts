// TODO: FIXME: MOCK SERVICES!!
import { type PartialDeep } from 'type-fest';

import type {
  CharacterCharacteristics,
  CharacteristicSectionKey,
  SkillKey,
  CharacteristicKey,
  CharacterSkills,
} from '@/models/character';

import { useCharacterCharacteristic } from './use-character-characteristic';
import { createCharacteristics } from '@/services/characters-service';

it.each([
  [0, false],
  [1, true],
  [123, true],
])(
  'canConvertAttributesToSkills - freeAttributesPoints: %s',
  (freeAttributesPoints, expectation) => {
    const { canConvertAttributesToSkills } = useCharacterCharacteristic(
      ref(
        createCharacteristics({
          attributes: { points: freeAttributesPoints },
        })
      )
    );

    expect(canConvertAttributesToSkills.value).toStrictEqual(expectation);
  }
);

it.each([
  [0, false],
  [1, false],
  [2, true],
  [12, true],
])('canConvertSkillsToAttributes - freeSkillsPoints: %s', (freeSkillsPoints, expectation) => {
  const { canConvertSkillsToAttributes } = useCharacterCharacteristic(
    ref(
      createCharacteristics({
        skills: { points: freeSkillsPoints },
      })
    )
  );

  expect(canConvertSkillsToAttributes.value).toStrictEqual(expectation);
});

describe('wasChangeMade', () => {
  it.each<
    [
      PartialDeep<CharacterCharacteristics>,
      CharacteristicSectionKey,
      CharacteristicKey,
      number,
      boolean
    ]
  >([
    [{ attributes: { points: 1, strength: 1 } }, 'attributes', 'strength', 2, true],
    [{ attributes: { points: 1, strength: 2 } }, 'attributes', 'strength', 2, false],
    [{ attributes: { strength: 3 }, skills: { points: 1 } }, 'skills', 'ironFlesh', 1, true],
    // [{ attributes: { strength: 3 }, skills: { points: 1 } }, 'skills', 'ironFlesh', 2, false],
    [{ weaponProficiencies: { points: 3 } }, 'weaponProficiencies', 'oneHanded', 1, true],
    // [{ weaponProficiencies: { points: 3 } }, 'weaponProficiencies', 'oneHanded', 2, false],
    [{ weaponProficiencies: { points: 6 } }, 'weaponProficiencies', 'oneHanded', 2, true],
  ])(
    'wasChangeMade - %j, %s, %s, %s',
    (characteristics, sectionCharacteristicKey, characteristicKey, value, expectation) => {
      const { wasChangeMade, onInput } = useCharacterCharacteristic(
        ref(createCharacteristics(characteristics))
      );

      expect(wasChangeMade.value).toBeFalsy();
      onInput(sectionCharacteristicKey, characteristicKey, value);
      expect(wasChangeMade.value).toStrictEqual(expectation);
    }
  );

  it('wasChangeMade - all at once', () => {
    const { wasChangeMade, onInput } = useCharacterCharacteristic(
      ref(
        createCharacteristics({
          attributes: { points: 3 },
          skills: { points: 1 },
          weaponProficiencies: { points: 3 },
        })
      )
    );

    expect(wasChangeMade.value).toBeFalsy();

    onInput('attributes', 'strength', 3);
    onInput('skills', 'powerDraw', 1);
    onInput('weaponProficiencies', 'polearm', 3);
    expect(wasChangeMade.value).toBeTruthy();

    onInput('attributes', 'strength', 0);
    onInput('skills', 'powerDraw', 0);
    onInput('weaponProficiencies', 'polearm', 0);
    expect(wasChangeMade.value).toBeFalsy();
  });
});

it.each<[PartialDeep<CharacterCharacteristics>, boolean]>(
  // prettier-ignore
  [
    [{ attributes: { points: 0 }, skills: { points: 0 }, weaponProficiencies: { points: 0 } }, true],
    [{ attributes: { points: 0 }, skills: { points: 0 }, weaponProficiencies: { points: -10 } }, false],
    [{ attributes: { strength: 2 }, skills: { ironFlesh: 1 } }, false],
    [{ attributes: { strength: 5 }, skills: { powerStrike: 2 } }, false],
    [{ attributes: { strength: 6 }, skills: { powerStrike: 2 } }, true],
    [{ attributes: { strength: 12 }, skills: { powerDraw: 5 } }, false],
    [{ attributes: { agility: 5 }, skills: { mountedArchery: 1 } }, false],
    [{ attributes: { agility: 12 }, skills: { shield: 2 } }, true],
    // more cases -> currentSkillRequirementsSatisfied
]
)('isChangeValid - %j', (characteristics, expectation) => {
  const { isChangeValid } = useCharacterCharacteristic(ref(createCharacteristics(characteristics)));
  expect(isChangeValid.value).toStrictEqual(expectation);
});

it.each<[PartialDeep<CharacterCharacteristics>, boolean]>(
  // prettier-ignore
  [
    [{ attributes: { strength: 2 },  skills: { ironFlesh: 1 } },      false],
    [{ attributes: { strength: 3 },  skills: { ironFlesh: 1 } },      true],
    [{ attributes: { strength: 5 },  skills: { powerStrike: 2 } },    false],
    [{ attributes: { strength: 6 },  skills: { powerStrike: 2 } },    true],
    [{ attributes: { strength: 12 }, skills: { powerDraw: 5 } },      false],
    [{ attributes: { strength: 6 },  skills: { powerThrow: 2 } },     true],
    [{ attributes: { agility: 6 },   skills: { athletics: 3 } },      false],
    [{ attributes: { agility: 6 },   skills: { athletics: 2 } },      true],
    [{ attributes: { agility: 21 },  skills: { athletics: 7 } },      true],
    [{ attributes: { agility: 3 },   skills: { mountedArchery: 1 } }, false],
    [{ attributes: { agility: 5 },   skills: { mountedArchery: 1 } }, false],
    [{ attributes: { agility: 6 },   skills: { mountedArchery: 1 } }, true],
    [{ attributes: { agility: 6 },   skills: { shield: 1 } },         true],
    [{ attributes: { agility: 6 },   skills: { shield: 3 } },         false],
    [{ attributes: { agility: 6 },   skills: { shield: 2 } },         false],
    [{ attributes: { agility: 12 },  skills: { shield: 2 } },         true],
]
)('currentSkillRequirementsSatisfied - %j, %s', (characteristics, expectation) => {
  const { currentSkillRequirementsSatisfied } = useCharacterCharacteristic(
    ref(createCharacteristics(characteristics))
  );

  const [skillKey] = Object.keys(characteristics.skills as Partial<CharacterSkills>);
  expect(currentSkillRequirementsSatisfied(skillKey as SkillKey)).toStrictEqual(expectation);
});

it.each<
  [
    PartialDeep<CharacterCharacteristics>,
    CharacteristicSectionKey,
    CharacteristicKey,
    { modelValue?: number; min?: number; max: number }
  ]
>(
  // prettier-ignore
  [
    [{ attributes: { points: 0, strength: 0 } }, 'attributes', 'strength', { max: 0 }],
    [{ attributes: { points: 1, strength: 0 } }, 'attributes', 'strength', { max: 1, modelValue: 0 }],
    [{ attributes: { points: 2, strength: 0 } }, 'attributes', 'strength', { max: 1 }],
    [{ attributes: { points: 1, strength: 1 } }, 'attributes', 'strength', { max: 2, modelValue: 1, min: 1 }],
    [{ attributes: { points: 1, agility: 1 } }, 'attributes', 'agility', { max: 2, modelValue: 1, min: 1 }],
    //
    [{ attributes: { strength: 1 }, skills: { points: 1 } }, 'skills', 'ironFlesh', { max: 0, modelValue: 0, min: 0 }],
    [{ attributes: { strength: 3 }, skills: { points: 1 } }, 'skills', 'ironFlesh',  { max: 1, modelValue: 0, min: 0 }],
    [{ attributes: { strength: 1 }, skills: { points: 1, ironFlesh: 2 } }, 'skills', 'ironFlesh', { max: 2, modelValue: 2, min: 2 }],
    [{ attributes: { strength: 6 }, skills: { points: 1, ironFlesh: 2 } }, 'skills', 'ironFlesh', { max: 2, modelValue: 2, min: 2 }],
    [{ attributes: { strength: 9 }, skills: { points: 1, ironFlesh: 2 } }, 'skills', 'ironFlesh', { max: 3, modelValue: 2, min: 2 }],
    [{ attributes: { strength: 3 }, skills: { points: 1 } }, 'skills', 'powerStrike', { max: 1, modelValue: 0, min: 0 }],
    [{ attributes: { strength: 5 }, skills: { points: 4 } }, 'skills', 'powerStrike',  { max: 1, modelValue: 0, min: 0 }],
    [{ attributes: { strength: 3 }, skills: { points: 1 } }, 'skills', 'powerDraw', { max: 1, modelValue: 0, min: 0 }],
    [{ attributes: { strength: 0 }, skills: { points: 4 } }, 'skills', 'powerDraw', { max: 0, modelValue: 0, min: 0 }],
    [{ attributes: { strength: 2 }, skills: { points: 1 } }, 'skills', 'powerThrow', { max: 0, modelValue: 0, min: 0 }],
    [{ attributes: { strength: 6 }, skills: { points: 1 } }, 'skills', 'powerThrow', { max: 1, modelValue: 0, min: 0 }],
    [{ attributes: { agility: 1 },  skills: { points: 1 } }, 'skills', 'athletics', { max: 0, modelValue: 0, min: 0 }],
    [{ attributes: { agility: 3 },  skills: { points: 1 } }, 'skills', 'athletics', { max: 1, modelValue: 0, min: 0 }],
    [{ attributes: { agility: 12 }, skills: { points: 0, athletics: 4 } }, 'skills', 'athletics', { max: 4, modelValue: 4, min: 4 }],
    [{ attributes: { agility: 16 }, skills: { points: 1, athletics: 4 } }, 'skills', 'athletics', { max: 5, modelValue: 4, min: 4 }],
    [{ attributes: { agility: 1 },  skills: { points: 1 } }, 'skills', 'riding', { max: 0, modelValue: 0, min: 0 }],
    [{ attributes: { agility: 1 },  skills: { points: 1 } }, 'skills', 'weaponMaster',   { max: 0, modelValue: 0, min: 0 }],
    [{ attributes: { agility: 1 },  skills: { points: 1 } }, 'skills', 'mountedArchery', { max: 0, modelValue: 0, min: 0 }],
    [{ attributes: { agility: 5 },  skills: { points: 1 } }, 'skills', 'mountedArchery', { max: 0, modelValue: 0, min: 0 }],
    [{ attributes: { agility: 6 },  skills: { points: 1 } }, 'skills', 'mountedArchery', { max: 1, modelValue: 0, min: 0 }],
    [{ attributes: { agility: 9 },  skills: { points: 1 } }, 'skills', 'shield', { max: 1, modelValue: 0, min: 0 }],
    [{ attributes: { agility: 9 },  skills: { points: 0 } }, 'skills', 'shield', { max: 0, modelValue: 0, min: 0 }],
    [{ attributes: { agility: 9 },  skills: { points: 1, shield: 2 } }, 'skills', 'shield', { max: 2, modelValue: 2, min: 2 }],
    [{ attributes: { agility: 15 }, skills: { points: 1, shield: 2 } }, 'skills', 'shield', { max: 2, modelValue: 2, min: 2 }],
    [{ attributes: { agility: 18 }, skills: { points: 1, shield: 2 } }, 'skills', 'shield', { max: 3, modelValue: 2, min: 2 }],
    //
    [{ weaponProficiencies: { points: 0 } }, 'weaponProficiencies', 'oneHanded', { max: 0,  modelValue: 0,  min: 0 }],
    [{ weaponProficiencies: { points: 1 } }, 'weaponProficiencies', 'oneHanded', { max: 0,  modelValue: 0,  min: 0 }],
    [{ weaponProficiencies: { points: 3 } }, 'weaponProficiencies', 'oneHanded', { max: 1,  modelValue: 0,  min: 0 }],
    [{ weaponProficiencies: { points: 217 } }, 'weaponProficiencies', 'oneHanded', { max: 1,  modelValue: 0,  min: 0 }],
    [{ weaponProficiencies: { points: 79, oneHanded: 42 } }, 'weaponProficiencies', 'oneHanded', { max: 43, modelValue: 42, min: 42 }],
    [{ weaponProficiencies: { points: 2,  oneHanded: 60 } }, 'weaponProficiencies', 'oneHanded', { max: 60, modelValue: 60, min: 60 }],
    [{ weaponProficiencies: { points: 6,  oneHanded: 59 } }, 'weaponProficiencies', 'oneHanded', { max: 60, modelValue: 59, min: 59 }],
]
)(
  'getInputProps - %j, %s, %s',
  (characteristics, characteristicSectionKey, characteristicKey, expectation) => {
    const { getInputProps } = useCharacterCharacteristic(
      ref(createCharacteristics(characteristics))
    );

    expect(getInputProps(characteristicSectionKey, characteristicKey)).toContain(expectation);
  }
);

it.each<
  [
    PartialDeep<CharacterCharacteristics>,
    CharacteristicSectionKey,
    CharacteristicKey,
    number,
    PartialDeep<CharacterCharacteristics>
  ]
>(
  // prettier-ignore
  [
    [{ attributes: { points: 1, strength: 0 } }, 'attributes', 'strength', 1, { attributes: { points: 0, strength: 1 } }],
    // [{ attributes: { points: 0, strength: 0 } }, 'attributes', 'strength', 1, { attributes: { points: 0, strength: 0 } }],
    // [{ attributes: { points: 1, strength: 0 } }, 'attributes', 'strength', 2, { attributes: { points: 1, strength: 0 } }],
    [{ attributes: { points: 2, strength: 2 } }, 'attributes', 'strength', 4, { attributes: { points: 0, strength: 4 } }],
    [{ attributes: { points: 1, agility: 0 } },  'attributes', 'agility', 1, { attributes: { points: 0, agility: 1 }, weaponProficiencies: { points: 14 } }],
    // [{ skills: { points: 1, weaponMaster: 0 } }, 'skills', 'weaponMaster', 1, { skills: { points: 1, weaponMaster: 0 } }],
    [{ attributes: { agility: 3 }, skills: { points: 1} }, 'skills', 'weaponMaster', 1, { skills: { points: 0, weaponMaster: 1 } }],
    [{ attributes: { agility: 6 }, skills: { points: 1} }, 'skills', 'shield', 1, { skills: { points: 0, shield: 1 } }],
    // [{ attributes: { agility: 5 }, skills: { points: 1} }, 'skills', 'shield', 1, { skills: { points: 1, shield: 0 } }],
    // [{ skills: { points: 1, athletics: 0 } }, 'skills', 'athletics', 2, { skills: { points: 1, athletics: 0 } }],
    [{ weaponProficiencies: { points: 3 } }, 'weaponProficiencies', 'bow', 1, { weaponProficiencies: { bow: 1 } }],
    // [{ weaponProficiencies: { points: 3 } }, 'weaponProficiencies', 'bow', 2, { weaponProficiencies: { bow: 0 } }],
]
)(
  'onInput - %j, %s, %s, %s',
  (initialCharacteristics, characteristicSectionKey, characteristicKey, newValue, expectation) => {
    const { characteristics, onInput } = useCharacterCharacteristic(
      ref(createCharacteristics(initialCharacteristics))
    );

    onInput(characteristicSectionKey, characteristicKey, newValue);

    Object.entries(expectation).forEach(([key, values]) => {
      expect(characteristics.value).toEqual(
        expect.objectContaining({
          [key]: expect.objectContaining(values),
        })
      );
    });
  }
);

it('convertAttributeToSkills', () => {
  const { characteristics, convertAttributeToSkills } = useCharacterCharacteristic(
    ref(createCharacteristics({ attributes: { points: 5 }, skills: { points: 10 } }))
  );

  convertAttributeToSkills();
  convertAttributeToSkills();

  expect(characteristics.value.attributes.points).toEqual(3);
  expect(characteristics.value.skills.points).toEqual(14);
});

it('convertSkillsToAttribute', () => {
  const { characteristics, convertSkillsToAttribute } = useCharacterCharacteristic(
    ref(createCharacteristics({ attributes: { points: 5 }, skills: { points: 10 } }))
  );

  convertSkillsToAttribute();
  convertSkillsToAttribute();

  expect(characteristics.value.attributes.points).toEqual(7);
  expect(characteristics.value.skills.points).toEqual(6);
});

it('reset', () => {
  const { characteristics, reset, onInput } = useCharacterCharacteristic(
    ref(createCharacteristics({ attributes: { points: 5 }, skills: { points: 10 } }))
  );

  expect(characteristics.value.attributes.agility).toEqual(0);
  expect(characteristics.value.attributes.points).toEqual(5);

  onInput('attributes', 'agility', 2);

  expect(characteristics.value.attributes.agility).toEqual(2);
  expect(characteristics.value.attributes.points).toEqual(3);

  reset();

  expect(characteristics.value.attributes.agility).toEqual(0);
  expect(characteristics.value.attributes.points).toEqual(5);
});

it('free 1 attribute point scenario', () => {
  const {
    characteristics,
    wasChangeMade,
    isChangeValid,
    canConvertAttributesToSkills,
    onInput,
    getInputProps,
  } = useCharacterCharacteristic(ref(createCharacteristics({ attributes: { points: 1 } })));

  //
  expect(characteristics.value.attributes.points).toEqual(1);
  expect(characteristics.value.attributes.strength).toEqual(0);

  expect(getInputProps('attributes', 'strength').max).toEqual(1);
  expect(getInputProps('attributes', 'agility').max).toEqual(1);

  expect(canConvertAttributesToSkills.value).toBeTruthy();
  expect(wasChangeMade.value).toBeFalsy();

  // +1 strength
  onInput('attributes', 'strength', 1);

  //
  expect(characteristics.value.attributes.points).toEqual(0);
  expect(characteristics.value.attributes.strength).toEqual(1);

  expect(getInputProps('attributes', 'strength')).toEqual({
    min: 0,
    max: 1,
    modelValue: 1,
  });
  expect(getInputProps('attributes', 'agility').max).toEqual(0);

  expect(wasChangeMade.value).toBeTruthy();
  expect(isChangeValid.value).toBeTruthy();
  expect(canConvertAttributesToSkills.value).toBeFalsy();
});

it('scenario - 33 lvl, tin can', () => {
  const {
    characteristics,
    wasChangeMade,
    isChangeValid,
    canConvertSkillsToAttributes,
    canConvertAttributesToSkills,
    onInput,
    convertSkillsToAttribute,
  } = useCharacterCharacteristic(
    ref(
      createCharacteristics({
        attributes: { points: 32, strength: 3, agility: 3 },
        skills: { points: 34 },
        weaponProficiencies: { points: 322 },
      })
    )
  );

  onInput('attributes', 'strength', 30);
  onInput('attributes', 'agility', 8);

  for (let i = 0; i < 4; i++) {
    convertSkillsToAttribute();
  }

  onInput('attributes', 'agility', 12);

  expect(characteristics.value.weaponProficiencies.points).toEqual(448); // of agi

  onInput('skills', 'ironFlesh', 10);
  onInput('skills', 'powerStrike', 10);
  onInput('skills', 'athletics', 4);
  onInput('skills', 'weaponMaster', 2);

  expect(characteristics.value.weaponProficiencies.points).toEqual(618); // of wm

  onInput('weaponProficiencies', 'twoHanded', 117);

  expect(characteristics.value.weaponProficiencies.points).toEqual(1);

  expect(characteristics.value).toEqual({
    attributes: { points: 0, strength: 30, agility: 12 },
    skills: {
      points: 0,
      ironFlesh: 10,
      powerStrike: 10,
      powerDraw: 0,
      powerThrow: 0,
      athletics: 4,
      riding: 0,
      weaponMaster: 2,
      mountedArchery: 0,
      shield: 0,
    },
    weaponProficiencies: {
      points: 1,
      oneHanded: 0,
      twoHanded: 117,
      polearm: 0,
      bow: 0,
      throwing: 0,
      crossbow: 0,
    },
  });

  expect(wasChangeMade.value).toBeTruthy();
  expect(isChangeValid.value).toBeTruthy();

  expect(canConvertSkillsToAttributes.value).toBeFalsy();
  expect(canConvertAttributesToSkills.value).toBeFalsy();
});
