using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AppPlate.Core.Concurrency;
using AppPlate.Core.Extensions;

namespace AppPlate.Core.Messaging
{
    /// <summary>
    /// A Tread-safe implementation of the <see cref="IMessengerHub"/>
    /// </summary>
    public class ThreadSafeMessengerHub : IMessengerHub
    {
        private readonly object _padLock = new object();

        private readonly Dictionary<Type, List<SubscriberInfo>> _subscribers =
            new Dictionary<Type, List<SubscriberInfo>>();

        private class SubscriberInfo
        {
            public WeakReference Subscriber { get; set; }
            public IDispatcher Dispatcher { get; set; }
        }

        private class SubscriberInfo<TMessage> : SubscriberInfo
            where TMessage : IMessage
        {
            public Action<TMessage> Action { get; set; }
        }

        public void Publish<TMessage>(TMessage message) where TMessage : IMessage
        {
            Publish(message, null);
        }

        public void Publish<TMessage>(TMessage message, IDispatcher dispatcher) where TMessage : IMessage
        {
            // Check if the generic type is the base type, then we need to do it by reflection and figure out the 'real' message type
            if (typeof (IMessage) == typeof (TMessage))
            {
                var messageType = message.GetType();
                this.CallMethod("PublishInternal", messageType, message, new List<object>(), dispatcher);
            }
            else
            {
                PublishInternal<TMessage>(message, new List<object>(), dispatcher);
            }
        }

        private void PublishInternal<TMessage>(TMessage message, IList<object> deliveries, IDispatcher dispatcher) where TMessage : IMessage
        {
            SubscriberInfo<TMessage>[] subscribersForMessage = null;

            lock (_padLock)
            {
                if (_subscribers.ContainsKey(typeof(TMessage)))
                {
                    var subscriberList = _subscribers[typeof(TMessage)];
                    subscribersForMessage = subscriberList.OfType<SubscriberInfo<TMessage>>().ToArray();
                }
            }

            if (subscribersForMessage != null)
            {
                foreach (var subscriberInfo in subscribersForMessage)
                {
                    if (subscriberInfo.Subscriber.IsAlive && !deliveries.Contains(subscriberInfo.Subscriber.Target))
                    {
                        // Dispatch it using the subscriber's Dispatcher if there is one
                        if (subscriberInfo.Dispatcher != null)
                        {
                            var info = subscriberInfo;
                            subscriberInfo.Dispatcher.DispatchAction(() => info.Action(message));
                        }
                        else
                        {
                            // If there is a dispatcher for the publication use that one
                            if (dispatcher != null)
                            {
                                var info = subscriberInfo;
                                dispatcher.DispatchAction(() => info.Action(message));
                            }
                            // Ok, let's not use a dispatcher, this will go directly on the same thread as the 
                            // publisher
                            else
                            {
                                subscriberInfo.Action(message);                                
                            }
                        }
                        deliveries.Add(subscriberInfo.Subscriber.Target);
                    }
                }
            }

            
            var parentMessageTypes = typeof(TMessage)
                .GetParentTypes((type) => type.ImplementsInterface<IMessage>())
                .ToArray();

            foreach (var parentMessageType in parentMessageTypes)
            {
                this.CallMethod("PublishInternal", parentMessageType, message, deliveries);
            }
        }

        public void Subscribe<TMessage>(object subscriber, Action<TMessage> subscriberAction) where TMessage : IMessage
        {
            Debug.WriteLine("MESSAGEHUB: {0} subscribe to message {1} started", subscriber.TypeName(),
                typeof (TMessage).Name);

            lock (_padLock)
            {
                var subscriberInfo = new SubscriberInfo<TMessage>()
                {
                    Subscriber = new WeakReference(subscriber),
                    Action = new Action<TMessage>(subscriberAction)
                };
                if (_subscribers.ContainsKey(typeof (TMessage)))
                {
                    var subscriberList = _subscribers[typeof (TMessage)];
                    subscriberList.Add(subscriberInfo);
                }
                else
                {
                    var subscriberList = new List<SubscriberInfo> {subscriberInfo};
                    _subscribers.Add(typeof (TMessage), subscriberList);
                }
            }
        }

        public void Subscribe<TMessage>(object subscriber, Action<TMessage> subscriberAction, IDispatcher dispatcher)
            where TMessage : IMessage
        {
            Debug.WriteLine("MESSAGEHUB: {0} subscribe to message {1} started", subscriber.TypeName(),
                typeof (TMessage).Name);

            lock (_padLock)
            {
                var subscriberInfo = new SubscriberInfo<TMessage>()
                {
                    Subscriber = new WeakReference(subscriber),
                    Action = new Action<TMessage>(subscriberAction),
                    Dispatcher = dispatcher
                };
                if (_subscribers.ContainsKey(typeof (TMessage)))
                {
                    var subscriberList = _subscribers[typeof (TMessage)];
                    subscriberList.Add(subscriberInfo);
                }
                else
                {
                    var subscriberList = new List<SubscriberInfo> {subscriberInfo};
                    _subscribers.Add(typeof (TMessage), subscriberList);
                }
            }
        }

        public void Unsubscribe<TMessage>(object subscriber) where TMessage : IMessage
        {
            Debug.WriteLine("MESSAGEHUB: {0} UNsubscribe to message {1} started", subscriber.TypeName(),
                typeof (TMessage).Name);

            lock (_padLock)
            {
                if (_subscribers.ContainsKey(typeof (TMessage)))
                {
                    var subscriberList = _subscribers[typeof (TMessage)];
                    subscriberList.RemoveAll(s => s.Subscriber.IsAlive && s.Subscriber.Target == subscriber);

                    if (subscriberList.Count == 0)
                    {
                        _subscribers.Remove(typeof (TMessage));
                    }
                }
            }
        }
    }
}