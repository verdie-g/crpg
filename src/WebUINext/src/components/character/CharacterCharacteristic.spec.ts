import { mount } from '@vue/test-utils';
import { createCharacteristics } from '@/services/characters-service';
import { type CharacterCharacteristics, CharacteristicConversion } from '@/models/character';

import CharacterCharacteristicComponent from './CharacterCharacteristic.vue';

it('emit - commit', async () => {
  const wrapper = mount(CharacterCharacteristicComponent, {
    shallow: true,
    props: {
      characterId: 6,
      characteristics: createCharacteristics({
        attributes: {
          points: 1,
          strength: 3,
          agility: 3,
        },
      }),
      weight: 0,
    },
  });

  const resetActionEl = wrapper.find('[data-aq-reset-action]');
  const commitActionEl = wrapper.find('[data-aq-commit-action]');
  const convertAttributesActionEl = wrapper.find('[data-aq-convert-attributes-action]');
  const convertSkillsActionEl = wrapper.find('[data-aq-convert-skills-action]');
  const strInputActionEl = wrapper.find<HTMLInputElement>(
    '[data-aq-control="attributes:strength"]'
  );
  expect(resetActionEl.attributes('disabled')).toBeDefined();
  expect(commitActionEl.attributes('disabled')).toBeDefined();
  expect(convertAttributesActionEl.attributes('disabled')).not.toBeDefined(); // btn is active
  expect(convertSkillsActionEl.attributes('disabled')).toBeDefined();

  await strInputActionEl.setValue(4); // input change event
  expect(strInputActionEl.element.value).toEqual('4');

  expect(resetActionEl.attributes('disabled')).not.toBeDefined();
  expect(commitActionEl.attributes('disabled')).not.toBeDefined();
  expect(convertAttributesActionEl.attributes('disabled')).toBeDefined();
  expect(convertSkillsActionEl.attributes('disabled')).toBeDefined();

  await commitActionEl.trigger('click'); // emit commit event

  const emittedCharacterCharacteristic =
    wrapper.emitted<CharacterCharacteristics[]>('commit')![0]![0];
  expect(emittedCharacterCharacteristic.attributes.strength).toEqual(4);
  expect(emittedCharacterCharacteristic.attributes.points).toEqual(0);

  expect(strInputActionEl.element.value).toEqual('3'); // reset was called
});

it('emit - convertCharacterCharacteristics', async () => {
  const wrapper = mount(CharacterCharacteristicComponent, {
    shallow: true,
    props: {
      characterId: 6,
      characteristics: createCharacteristics({
        attributes: {
          points: 1,
          strength: 3,
          agility: 3,
        },
      }),
      weight: 0,
    },
  });

  const resetActionEl = wrapper.find('[data-aq-reset-action]');
  const commitActionEl = wrapper.find('[data-aq-commit-action]');

  const convertAttributesActionEl = wrapper.find('[data-aq-convert-attributes-action]');
  const convertSkillsActionEl = wrapper.find('[data-aq-convert-skills-action]');
  const attributesPoints = wrapper.find('[data-aq-fields-group="attributes"] [data-aq-points]');
  const skillsPoints = wrapper.find('[data-aq-fields-group="skills"] [data-aq-points]');

  expect(resetActionEl.attributes('disabled')).toBeDefined();
  expect(commitActionEl.attributes('disabled')).toBeDefined();

  expect(convertAttributesActionEl.attributes('disabled')).not.toBeDefined();
  expect(convertSkillsActionEl.attributes('disabled')).toBeDefined();

  expect(attributesPoints.text()).toEqual('1');
  expect(skillsPoints.text()).toEqual('0');

  await convertAttributesActionEl.trigger('click');

  expect(convertAttributesActionEl.attributes('disabled')).toBeDefined(); // now, action not active
  expect(convertSkillsActionEl.attributes('disabled')).not.toBeDefined();

  expect(attributesPoints.text()).toEqual('0');
  expect(skillsPoints.text()).toEqual('2');

  expect(wrapper.emitted('convertCharacterCharacteristics')).toHaveLength(1);

  await convertSkillsActionEl.trigger('click');

  expect(wrapper.emitted('convertCharacterCharacteristics')).toHaveLength(2);

  const emittedCharacteristicConversion = wrapper.emitted<CharacteristicConversion[]>(
    'convertCharacterCharacteristics'
  );

  expect(emittedCharacteristicConversion![0][0]).toEqual(
    CharacteristicConversion.AttributesToSkills
  );

  expect(emittedCharacteristicConversion![1][0]).toEqual(
    CharacteristicConversion.SkillsToAttributes
  );

  expect(attributesPoints.text()).toEqual('1');
  expect(skillsPoints.text()).toEqual('0');

  expect(resetActionEl.attributes('disabled')).toBeDefined();
  expect(commitActionEl.attributes('disabled')).toBeDefined();
});

it('validation - currentSkillRequirementsSatisfied', async () => {
  const wrapper = mount(CharacterCharacteristicComponent, {
    shallow: true,
    props: {
      characterId: 6,
      characteristics: createCharacteristics({
        attributes: {
          points: 6,
          strength: 3,
          agility: 3,
        },
        skills: {
          points: 5,
        },
      }),
      weight: 0,
    },
  });

  const strInputActionEl = wrapper.find<HTMLInputElement>(
    '[data-aq-control="attributes:strength"]'
  );
  const ironFleshInputActionEl = wrapper.find<HTMLInputElement>(
    '[data-aq-control="skills:ironFlesh"]'
  );

  expect(wrapper.find('[data-aq-validation-warning]').exists()).toBeFalsy();

  await strInputActionEl.setValue(6);
  await ironFleshInputActionEl.setValue(2);
  await strInputActionEl.setValue(3);

  expect(wrapper.find('[data-aq-validation-warning]').exists()).toBeTruthy();
});
