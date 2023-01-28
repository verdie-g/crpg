using System.Text;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>
/// Used for notifications which a shared between gamemodes (!a command etc.).
/// Also includes the Native MultiplayerGameNotificationsComponent class.
/// </summary>
internal class CrpgNotificationComponent : MultiplayerGameNotificationsComponent
{
    protected override void AddRemoveMessageHandlers(
        GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsClientOrReplay)
        {
            registerer.Register<CrpgNotification>(HandleNotification);
            registerer.Register<CrpgServerMessage>(HandleServerMessage);
        }
    }

    private void HandleNotification(CrpgNotification notification)
    {
        string message = notification.IsMessageTextId ? GameTexts.FindText(notification.Message).ToString() : notification.Message;

        // Notifcation like "Flag A and B were removed"
        if (notification.Type == CrpgNotification.NotificationType.Notification)
        {
            MBInformationManager.AddQuickInformation(new TextObject(message), 0, null, notification.SoundEvent);
        }

        // Red announcement like "A new update is available. Please update your client" (Lobbyscreen)
        else if (notification.Type == CrpgNotification.NotificationType.Announcement)
        {
            InformationManager.AddSystemNotification(message);
        }

        // Plays a sound event
        else if (notification.Type == CrpgNotification.NotificationType.Sound)
        {
            SoundEvent.CreateEventFromString(notification.SoundEvent, Mission.Scene).Play();
        }
    }

    private void HandleServerMessage(CrpgServerMessage message)
    {
        string msg = message.IsMessageTextId ? GameTexts.FindText(message.Message).ToString() : message.Message;
        InformationManager.DisplayMessage(new InformationMessage(msg, new Color(message.Red, message.Green, message.Blue, message.Alpha)));
    }
}
