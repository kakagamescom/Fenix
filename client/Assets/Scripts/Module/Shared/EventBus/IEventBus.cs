using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Module.Shared {

    public interface IEventBus {

        List<object> Publish(string eventName, params object[] parameters);

        IEnumerable<object> NewPublishIEnumerable(string eventName, params object[] parameters);

        void Subscribe(object subscriber, string eventName, Delegate callback);

        IEnumerable<object> GetSubscribersFor(string eventName);

        bool Unsubscribe(object subscriber, string eventName);
        bool UnsubscribeAll(object subscriber);

        /// <summary> A list of events that where already published by the event bus </summary>
        ConcurrentQueue<string> eventHistory { get; set; }
        /// <summary> If true the <see cref="eventHistory"/> will be filled with  </summary>
        bool eventHistoryFillingEnabled { get; set; } 

    }

}