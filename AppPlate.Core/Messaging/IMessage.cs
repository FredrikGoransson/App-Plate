namespace AppPlate.Core.Messaging
{
    public interface IMessage
    {
    }

    public interface IMessage<T> : IMessage
    {
        T Body { get; set; }
    }
}