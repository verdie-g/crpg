using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgServerMessage : GameNetworkMessage
{
        public string Message
        {
            get;
            private set;
        }

        public float Red
        {
            get;
            private set;
        }

        public float Green
        {
            get;
            private set;
        }

        public float Blue
        {
            get;
            private set;
        }

        public float Alpha
        {
            get;
            private set;
        }

        public bool IsMessageTextId
        {
            get;
            private set;
        }

        public CrpgServerMessage(Color color, string message, bool isMessageTextId = false)
        {
            Message = message;
            Red = color.Red;
            Green = color.Green;
            Blue = color.Blue;
            Alpha = color.Alpha;
            IsMessageTextId = isMessageTextId;
        }

        protected override void OnWrite()
        {
            WriteStringToPacket(Message);
            WriteFloatToPacket(Red, CompressionInfo.Float.FullPrecision);
            WriteFloatToPacket(Green, CompressionInfo.Float.FullPrecision);
            WriteFloatToPacket(Blue, CompressionInfo.Float.FullPrecision);
            WriteFloatToPacket(Alpha, CompressionInfo.Float.FullPrecision);
            WriteBoolToPacket(IsMessageTextId);
        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;
            Message = ReadStringFromPacket(ref bufferReadValid);
            Red = ReadFloatFromPacket(CompressionInfo.Float.FullPrecision, ref bufferReadValid);
            Green = ReadFloatFromPacket(CompressionInfo.Float.FullPrecision, ref bufferReadValid);
            Blue = ReadFloatFromPacket(CompressionInfo.Float.FullPrecision, ref bufferReadValid);
            Alpha = ReadFloatFromPacket(CompressionInfo.Float.FullPrecision, ref bufferReadValid);
            IsMessageTextId = ReadBoolFromPacket(ref bufferReadValid);
            return bufferReadValid;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Messaging;
        }

        protected override string OnGetLogFormat()
        {
            return "Crpg Message from server: " + Message;
        }
    }
