<script setup lang="ts">
import { useCounter } from '@/composables/example';
import { useUserStore } from '@/stores/example';
import { useWait } from '@/composables/use-wait';

// Provide/Inject
const $w = useWait();

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

const waitingKey = 'waiting...';

// Async action with provide/inject calls
const changeInnerMsg = async (): Promise<void> => {
  $w.start(waitingKey);

  await new Promise(resolve => resolve(true));

  innerMsg.value = 'changed example cmp';

  $w.end(waitingKey);
};

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

// Vue Router
const route = useRoute();
const router = useRouter();

// i18n
const { t } = useI18n();

// TODO: more examples..
// vuelidate
</script>

<template>
  <div>
    <!--  -->
    <div>
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
      <div>
        method + provide/inject:
        <button data-aq-example-method-with-provide-inject @click="changeInnerMsg">
          Method, bro
        </button>
      </div>
      <div data-aq-example-loading-indicator v-if="$w.is(waitingKey)">Loading...</div>
    </div>

    <!--  -->

    <br />
    <br />

    <div>
      <div>
        composable state:
        <span data-aq-example-composable>{{ counter }}</span>
      </div>
      <div>
        composable methods
        <button data-aq-example-composable-method @click="increase">click bro</button>
      </div>
    </div>

    <br />
    <br />

    <!--  -->

    <div>
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

    <br />
    <br />

    <!--  -->

    <div>
      router
      <div>
        route path
        <span data-aq-example-route-path>
          {{ route.path }}
        </span>
      </div>
      <div>
        router methods
        <button data-aq-example-router-push @click="router.push({ name: 'index' })">
          push router, bro
        </button>
      </div>
    </div>

    <br />
    <br />

    <!--  -->

    <div>
      i18n
      <span data-aq-example-i18n>
        {{ t('button.back') }}
      </span>
    </div>

    <br />
    <br />

    <!--  -->

    <div>
      children component
      <SvgIcon
        data-aq-example-children-component
        name="example"
        viewBox="0 0 32 32"
        class="w-12"
      ></SvgIcon>
    </div>
  </div>
</template>
