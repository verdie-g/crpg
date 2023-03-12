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
            registerer.Register<CrpgNotificationId>(HandleNotificationId);
            registerer.Register<CrpgServerMessage>(HandleServerMessage);
        }
    }

    private void HandleNotification(CrpgNotification notification)
    {
        PrintNotification(notification.Message, notification.Type, notification.SoundEvent);
    }

    private void HandleNotificationId(CrpgNotificationId notification)
    {
        string message = GameTexts.FindText(notification.TextId, notification.TextVariation).ToString();
        PrintNotification(message, notification.Type, notification.SoundEvent);
    }

    private void HandleServerMessage(CrpgServerMessage message)
    {
        string msg = message.IsMessageTextId ? GameTexts.FindText(message.Message).ToString() : message.Message;
        InformationManager.DisplayMessage(new InformationMessage(msg, new Color(message.Red, message.Green, message.Blue, message.Alpha)));
    }

    private void PrintNotification(string message, CrpgNotificationType type, string? soundEvent)
    {
        if (type == CrpgNotificationType.Notification) // Small text at the top of the screen.
        {
            MBInformationManager.AddQuickInformation(new TextObject(message), 0, null, soundEvent);
        }
        else if (type == CrpgNotificationType.Announcement) // Big red text in the middle of the screen.
        {
            InformationManager.AddSystemNotification(message);
        }
        else if (type == CrpgNotificationType.Sound)
        {
            SoundEvent.CreateEventFromString(soundEvent, Mission.Scene).Play();
        }
    }
}
