using System;
using AppPlate.Core.Concurrency;

namespace AppPlate.Core.Messaging
{
    public interface IMessengerHub
    {
        /// <summary>
        /// Publishes a message to all registered subscribers
        /// </summary>
        /// <typeparam name="TMessage">The generic message type. Should implement <see cref="IMessage"/></typeparam>
        /// <param name="message">The instance of <typeparamref name="TMessage"/> that contains the message to be published</param>
        void Publish<TMessage>(TMessage message) where TMessage : IMessage;

        /// <summary>
        /// Publishes a message to all registered subscribers
        /// </summary>
        /// <typeparam name="TMessage">The generic message type. Should implement <see cref="IMessage"/></typeparam>
        /// <param name="message">The instance of <typeparamref name="TMessage"/> that contains the message to be published</param>
        /// <param name="dispatcher">The dispatcher used to publish the messages. This can be used to make the messengerhub
        /// dispatch messages asynchronously. Any subscribers with registered Dispatchers will be dispatched using those instead.</param>
        void Publish<TMessage>(TMessage message, IDispatcher dispatcher) where TMessage : IMessage;

        /// <summary>
        /// Adds a subscription to the specified message for the subscriber.
        /// </summary>
        /// <typeparam name="T">The message type to subscribe to</typeparam>
        /// <param name="subscriber">The subscriber instance. This is used when unsubscribing from the message. Each subscriber 
        /// should only have one subscription per message.</param>
        /// <param name="subscriberAction">The action to perform when an instance of the message is published</param>
        void Subscribe<T>(object subscriber, Action<T> subscriberAction) where T : IMessage;

        /// <summary>
        /// Adds a subscription to the specified message for the subscriber.
        /// </summary>
        /// <typeparam name="T">The message type to subscribe to</typeparam>
        /// <param name="subscriber">The subscriber instance. This is used when unsubscribing from the message. Each subscriber 
        /// should only have one subscription per message.</param>
        /// <param name="subscriberAction">The action to perform when an instance of the message is published</param>        
        /// <param name="dispatcher">The dispatcher used to deliver published messages. This can be used to marshal message
        /// deliveries back to a specific thread (e.g. the UI thread in order to avoid Chross-thread exceptions when updating
        /// the UI)</param>
        void Subscribe<T>(object subscriber, Action<T> subscriberAction, IDispatcher dispatcher) where T : IMessage;

        /// <summary>
        /// Unsubscribes a
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subscriber"></param>
        void Unsubscribe<T>(object subscriber) where T : IMessage;
    }
}