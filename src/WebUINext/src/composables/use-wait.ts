import { injectStrict } from '@/utils/injectStrict';
import { VueWaitKey } from '@/boot/wait';

/*
A loader manager for vue3 with reactive method
in component:

import { useWait } from '@/composables/use-wait.ts'
const $w = useWait();

$w.start('loading');
$w.end('loading');
$w.is('loading')
Boolean($w.any())
*/

export const useWait = () => injectStrict(VueWaitKey);
