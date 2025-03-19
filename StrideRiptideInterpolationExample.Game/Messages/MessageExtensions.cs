using Riptide;
using Stride.Core.Mathematics;

namespace StrideRiptideInterpolationExample.Messages
{
    public static class MessageExtensions
    {
        public static Message AddVector3(this Message message, Vector3 value) =>
            message.AddFloat(value.X).AddFloat(value.Y).AddFloat(value.Z);

        public static Vector3 GetVector3(this Message message) =>
            new(message.GetFloat(), message.GetFloat(), message.GetFloat());

        public static Message AddQuaternion(this Message message, Quaternion value) =>
            message.AddFloat(value.X).AddFloat(value.Y).AddFloat(value.Z).AddFloat(value.W);

        public static Quaternion GetQuaternion(this Message message) =>
            new(message.GetFloat(), message.GetFloat(), message.GetFloat(), message.GetFloat());
    }
}
