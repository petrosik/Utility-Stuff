namespace Petrosik
{
    namespace Utility
    {
        using System;
        using System.Threading;
        using System.Threading.Tasks;
        /// <summary>
        /// Represents a listener for events of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>This class provides mechanisms to trigger events and manage event listeners. It
        /// supports asynchronous waiting for specific event conditions and ensures thread-safe operations.</remarks>
        /// <typeparam name="T">The type of the event data.</typeparam>
        public class EventListener<T>
        {
            public event Action<T>? OnTrigger;
            private readonly object _lock = new();
            private T? _lastMessage;

            public void Trigger(T args)
            {
                Action<T>? handlers;
                lock (_lock)
                {
                    _lastMessage = args;
                    handlers = OnTrigger;
                }

                handlers?.Invoke(args);
            }

            public Task WaitForTriggerAsync(Func<T, bool> condition, CancellationToken cancellationToken = default)
            {
                var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

                lock (_lock)
                {
                    if (_lastMessage is { } msg && condition(msg))
                    {
                        tcs.SetResult();
                        return tcs.Task;
                    }

                    Action<T> handler = null!;
                    handler = (args) =>
                    {
                        if (condition(args))
                        {
                            tcs.TrySetResult();
                            RemoveListener(handler);
                        }
                    };

                    OnTrigger += handler;

                    cancellationToken.Register(() =>
                    {
                        tcs.TrySetCanceled();
                        RemoveListener(handler);
                    });
                }

                return tcs.Task;
            }

            public void AddListener(Action<T> listener)
            {
                lock (_lock)
                    OnTrigger += listener;
            }

            public void RemoveListener(Action<T> listener)
            {
                lock (_lock)
                    OnTrigger -= listener;
            }
        }
        /// <summary>
        /// Represents a listener for events that can be triggered and awaited asynchronously.
        /// </summary>
        /// <remarks>The <see cref="EventListener"/> class allows adding and removing event handlers that
        /// are invoked when the event is triggered. It provides functionality to wait asynchronously for the event to
        /// be triggered, supporting cancellation.</remarks>
        public class EventListener
        {
            public event Action? OnTrigger;
            private readonly object _lock = new();

            public void Trigger()
            {
                Action? handlers;
                lock (_lock)
                {
                    handlers = OnTrigger;
                }

                handlers?.Invoke();
            }

            public Task WaitForTriggerAsync(CancellationToken cancellationToken = default)
            {
                var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

                lock (_lock)
                {
                    Action handler = null!;
                    handler = () =>
                    {
                        tcs.TrySetResult();
                        RemoveListener(handler);
                    };

                    OnTrigger += handler;

                    cancellationToken.Register(() =>
                    {
                        tcs.TrySetCanceled();
                        RemoveListener(handler);
                    });
                }

                return tcs.Task;
            }

            public void AddListener(Action listener)
            {
                lock (_lock)
                    OnTrigger += listener;
            }

            public void RemoveListener(Action listener)
            {
                lock (_lock)
                    OnTrigger -= listener;
            }
        }
    }
}