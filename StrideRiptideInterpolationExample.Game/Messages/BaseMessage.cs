using Riptide;
using System;

namespace StrideRiptideInterpolationExample.Messages
{
    public enum GameMessageId : ushort
    {
        CharacterSpawn = 1,
        CharacterAction = 2,
    }

    public abstract class BaseMessage<T> where T : class, new()
    {
        public T Data { get; protected set; }
        public Message Message { get; protected set; }
        public abstract MessageSendMode Mode { get; }
        public abstract Enum MessageId { get; }

        public virtual Message Create(T data)
        {
            Data = data;
            Message = Message.Create(Mode, MessageId);
            CreateMessage();

            return Message;
        }

        public virtual T From(Message message)
        {
            Message = message;
            Data = new();
            FromMessage();

            return Data;
        }

        protected abstract void CreateMessage();
        protected abstract void FromMessage();
    }
}
