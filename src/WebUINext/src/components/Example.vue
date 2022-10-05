<script setup lang="ts">
import { useCounter } from '@/composables/example';
import { useUserStore } from '@/stores/example';

// Props
interface Props {
  msg: string;
}

const props = withDefaults(defineProps<Props>(), {
  msg: 'Prop msg',
});

// Reactive data
const innerMsg = ref<string>('example cmp');
const computedMsg = computed((): string => `${props.msg} - ${innerMsg.value}`);

// Composable
const { counter, increase } = useCounter();

// Store
const userStore = useUserStore();

// Emit
const emit = defineEmits<{
  (
    e: 'example:changed',
    {
      message,
      superMessage,
    }: {
      message: string;
      superMessage: string;
    }
  ): void;
}>();

// TODO: provide/inject
// TODO: vue-router
// TODO: more examples..
</script>

<template>
  <div>
    <!--  -->
    <div>
      prop:
      <span data-aq-example-prop>{{ msg }}</span>
    </div>
    <div>
      ref:
      <span data-aq-example-ref>{{ innerMsg }}</span>
    </div>
    <div>
      computed:
      <span data-aq-example-computed>{{ computedMsg }}</span>
    </div>
    <div>
      emit:
      <button
        data-aq-example-emit
        @click="emit('example:changed', { message: msg, superMessage: computedMsg })"
      >
        Emit, bro
      </button>
    </div>

    <!--  -->

    <div>
      composable state:
      <span data-aq-example-composable>{{ counter }}</span>
    </div>
    <div>
      composable methods
      <button data-aq-example-composable-method @click="increase">click bro</button>
    </div>

    <!--  -->

    <div>
      store action:
      <button data-aq-example-store-action @click="userStore.fetch">fetch user</button>
    </div>
    <div>
      store state:
      <span data-aq-example-store-state>{{ userStore.name }}</span>
    </div>
    <div>
      store getter:
      <span data-aq-example-store-getter>
        {{ userStore.namePlusRole }}
      </span>
    </div>
  </div>
</template>
