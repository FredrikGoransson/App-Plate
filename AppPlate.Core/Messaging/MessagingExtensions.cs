using System.Threading.Tasks;

namespace AppPlate.Core.Messaging
{
    public static class MessagingExtensions
    {
        public static void PublishAsync<TMessage>(this IMessengerHub messengerHub, TMessage message)
            where TMessage : IMessage
        {
            Task.Factory.StartNew(() => messengerHub.Publish(message));
        }

        public static void Unsubscribe<TMessage1, TMessage2>(
            this IMessengerHub messengerHub,
            object subscriber)
            where TMessage1 : IMessage
            where TMessage2 : IMessage
        {
            messengerHub.Unsubscribe<TMessage1>(subscriber);
            messengerHub.Unsubscribe<TMessage2>(subscriber);
        }

        public static void Unsubscribe<TMessage1, TMessage2, TMessage3>(
            this IMessengerHub messengerHub,
            object subscriber)
            where TMessage1 : IMessage
            where TMessage2 : IMessage
            where TMessage3 : IMessage
        {
            messengerHub.Unsubscribe<TMessage1>(subscriber);
            messengerHub.Unsubscribe<TMessage2>(subscriber);
            messengerHub.Unsubscribe<TMessage3>(subscriber);
        }

        public static void Unsubscribe<TMessage1, TMessage2, TMessage3, TMessage4>(
            this IMessengerHub messengerHub,
            object subscriber)
            where TMessage1 : IMessage
            where TMessage2 : IMessage
            where TMessage3 : IMessage
            where TMessage4 : IMessage
        {
            messengerHub.Unsubscribe<TMessage1>(subscriber);
            messengerHub.Unsubscribe<TMessage2>(subscriber);
            messengerHub.Unsubscribe<TMessage3>(subscriber);
            messengerHub.Unsubscribe<TMessage4>(subscriber);
        }

        public static void Unsubscribe<TMessage1, TMessage2, TMessage3, TMessage4, TMessage5>(
            this IMessengerHub messengerHub,
            object subscriber)
            where TMessage1 : IMessage
            where TMessage2 : IMessage
            where TMessage3 : IMessage
            where TMessage4 : IMessage
            where TMessage5 : IMessage
        {
            messengerHub.Unsubscribe<TMessage1>(subscriber);
            messengerHub.Unsubscribe<TMessage2>(subscriber);
            messengerHub.Unsubscribe<TMessage3>(subscriber);
            messengerHub.Unsubscribe<TMessage4>(subscriber);
            messengerHub.Unsubscribe<TMessage5>(subscriber);
        }

        public static void Unsubscribe<TMessage1, TMessage2, TMessage3, TMessage4, TMessage5, TMessage6>(
            this IMessengerHub messengerHub,
            object subscriber)
            where TMessage1 : IMessage
            where TMessage2 : IMessage
            where TMessage3 : IMessage
            where TMessage4 : IMessage
            where TMessage5 : IMessage
            where TMessage6 : IMessage
        {
            messengerHub.Unsubscribe<TMessage1>(subscriber);
            messengerHub.Unsubscribe<TMessage2>(subscriber);
            messengerHub.Unsubscribe<TMessage3>(subscriber);
            messengerHub.Unsubscribe<TMessage4>(subscriber);
            messengerHub.Unsubscribe<TMessage5>(subscriber);
            messengerHub.Unsubscribe<TMessage6>(subscriber);
        }

        public static void Unsubscribe<TMessage1, TMessage2, TMessage3, TMessage4, TMessage5, TMessage6, TMessage7>(
            this IMessengerHub messengerHub,
            object subscriber)
            where TMessage1 : IMessage
            where TMessage2 : IMessage
            where TMessage3 : IMessage
            where TMessage4 : IMessage
            where TMessage5 : IMessage
            where TMessage6 : IMessage
            where TMessage7 : IMessage
        {
            messengerHub.Unsubscribe<TMessage1>(subscriber);
            messengerHub.Unsubscribe<TMessage2>(subscriber);
            messengerHub.Unsubscribe<TMessage3>(subscriber);
            messengerHub.Unsubscribe<TMessage4>(subscriber);
            messengerHub.Unsubscribe<TMessage5>(subscriber);
            messengerHub.Unsubscribe<TMessage6>(subscriber);
            messengerHub.Unsubscribe<TMessage7>(subscriber);
        }
    }
}