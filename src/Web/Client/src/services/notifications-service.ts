import { ToastProgrammatic as Toast } from 'buefy';

export function notify(message: string) {
  Toast.open({
    message,
    type: 'is-warning',
    position: 'is-top-right',
    queue: false,
  });
}
