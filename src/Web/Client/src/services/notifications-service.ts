import { ToastProgrammatic as Toast } from 'buefy';

export const enum NotificationType {
  Info,
  Warning,
  Error,
}

function typeToColor(type: NotificationType): string {
  switch (type) {
    case NotificationType.Info:
      return 'is-info';
    case NotificationType.Warning:
      return 'is-warning';
    case NotificationType.Error:
      return 'is-danger';
    default:
      throw new Error(`Unknown notification type ${type}`);
  }
}

export function notify(message: string, type: NotificationType = NotificationType.Info) {
  Toast.open({
    message,
    type: typeToColor(type),
    position: 'is-top-right',
    duration: 3000,
    queue: false,
  });
}
