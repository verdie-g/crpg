import { NotificationProgrammatic } from '@oruga-ui/oruga-next';

export enum NotificationType {
  Success = 'success',
  Warning = 'warning',
  Danger = 'danger',
}

export const notify = (message: string, type: NotificationType = NotificationType.Success) => {
  NotificationProgrammatic.open({
    message,
    position: 'top',
    variant: type,
    duration: 5000,
    queue: false,
  });
};
