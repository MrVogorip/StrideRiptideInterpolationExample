using Riptide;
using System;
using Stride.Core.Mathematics;

namespace StrideRiptideInterpolationExample.Messages
{
    public class CharacterActionData
    {
        public double RemoteTime { get; set; }
        public ushort PlayerId { get; set; }
        public float RunSpeed { get; set; }
        public bool IsJumped { get; set; }
        public bool IsGrounded { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }

    public class CharacterActionMessage : BaseMessage<CharacterActionData>
    {
        public override MessageSendMode Mode => MessageSendMode.Unreliable;
        public override Enum MessageId => GameMessageId.CharacterAction;

        protected override void CreateMessage()
        {
            Message.AddDouble(Data.RemoteTime);
            Message.AddUShort(Data.PlayerId);
            Message.AddFloat(Data.RunSpeed);
            Message.AddBool(Data.IsJumped);
            Message.AddBool(Data.IsGrounded);
            Message.AddVector3(Data.Position);
            Message.AddQuaternion(Data.Rotation);
        }

        protected override void FromMessage()
        {
            Data.RemoteTime = Message.GetDouble();
            Data.PlayerId = Message.GetUShort();
            Data.RunSpeed = Message.GetFloat();
            Data.IsJumped = Message.GetBool();
            Data.IsGrounded = Message.GetBool();
            Data.Position = Message.GetVector3();
            Data.Rotation = Message.GetQuaternion();
        }
    }
}
