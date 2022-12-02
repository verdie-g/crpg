<script setup lang="ts">
const props = withDefaults(
  defineProps<{
    modelValue: number;
    min?: number;
    max?: number;
    step?: number;
    exponential?: number;
    longPress?: boolean;
    readonly?: boolean;
  }>(),
  {
    readonly: false,
    step: 1,
    longPress: true,
  }
);

const emit = defineEmits<{
  (e: 'update:modelValue', modelValue: number): void;
}>();

const model = computed({
  set(val: number) {
    emit('update:modelValue', val);
  },

  get() {
    return props.modelValue;
  },
});

const decrement = () => {
  if (props.min !== undefined && model.value === props.min) {
    return;
  }

  model.value = model.value - props.step;
};

const increment = () => {
  if (props.max !== undefined && model.value === props.max) {
    return;
  }

  model.value = model.value + props.step;
};

const timesPressed = ref<number>(1);
let _intervalRef: number | null = null;

const longPressTick = (inc = false) => {
  if (inc) {
    increment();
  } else {
    decrement();
  }

  // @ts-ignore
  _intervalRef = setTimeout(
    () => {
      longPressTick(inc);
    },
    props.exponential !== undefined ? 250 / (props.exponential * timesPressed.value++) : 250
  );
};

const onCLick = (e: PointerEvent, inc = false) => {
  if (e.detail !== 0 || e.type !== 'click') return;

  if (inc) {
    increment();
    return;
  }

  decrement();
};

const onStartLongPress = (e: PointerEvent, inc = false) => {
  if (!props.longPress) return;
  if (e.button !== 0 && e.type !== 'touchstart') return;

  _intervalRef && clearTimeout(_intervalRef);
  longPressTick(inc);
};

const onStopLongPress = () => {
  if (!_intervalRef) return;
  timesPressed.value = 1;
  clearTimeout(_intervalRef);
  _intervalRef = null;
};
</script>

<template>
  <div class="flex items-center gap-2">
    <div
      @mouseup="onStopLongPress"
      @mouseleave="onStopLongPress"
      @touchend="onStopLongPress"
      @touchcancel="onStopLongPress"
    >
      <OButton
        iconLeft="minus"
        rounded
        variant="primary"
        size="2xs"
        inverted
        :disabled="model === props.min"
        @click="onCLick"
        @mousedown="onStartLongPress"
        @touchstart.prevent="onStartLongPress"
      />
    </div>

    <OInput
      v-model="model"
      type="number"
      :min="min"
      :max="max"
      :readonly="readonly"
      size="xs"
      class="w-12 text-center"
    />

    <div
      @mouseup="onStopLongPress"
      @mouseleave="onStopLongPress"
      @touchend="onStopLongPress"
      @touchcancel="onStopLongPress"
    >
      <OButton
        iconLeft="plus"
        rounded
        variant="primary"
        inverted
        size="2xs"
        :disabled="model === props.max"
        @click="onCLick($event, true)"
        @mousedown="onStartLongPress($event, true)"
        @touchstart.prevent="onStartLongPress($event, true)"
      />
    </div>
  </div>
</template>
