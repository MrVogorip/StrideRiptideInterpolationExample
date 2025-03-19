using Riptide;
using System;
using Stride.Core.Mathematics;

namespace StrideRiptideInterpolationExample.Messages
{
    public class CharacterSpawnData
    {
        public ushort PlayerId { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }

    public class CharacterSpawnMessage : BaseMessage<CharacterSpawnData>
    {
        public override MessageSendMode Mode => MessageSendMode.Reliable;
        public override Enum MessageId => GameMessageId.CharacterSpawn;

        protected override void CreateMessage()
        {
            Message.AddUShort(Data.PlayerId);
            Message.AddVector3(Data.Position);
            Message.AddQuaternion(Data.Rotation);
        }

        protected override void FromMessage()
        {
            Data.PlayerId = Message.GetUShort();
            Data.Position = Message.GetVector3();
            Data.Rotation = Message.GetQuaternion();
        }
    }
}
