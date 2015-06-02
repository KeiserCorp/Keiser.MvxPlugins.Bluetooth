namespace Keiser.MvxPlugins.Bluetooth.Droid
{
    using Keiser.MvxPlugins.Bluetooth;
    using Keiser.MvxPlugins.Bluetooth.LE;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public class CallbackQueuer
    {
        protected IScanCallback ScanCallback;
        protected ConcurrentQueue<IDevice> CallbackQueue = new ConcurrentQueue<IDevice>();
        protected CancellationTokenSource CallbackQueueCancellationTokenSource;
        protected Task CallbackQueueTask;
        public const int EmptyQueueThreshold = 15000, MaxEmptyQueueThreshold = 60000;
        protected event EventHandler EmptyQueueThresholdEvent;
        protected Bluetooth.Timer EmptyQueueTimer;

        public CallbackQueuer(IScanCallback scanCallback, EventHandler emptyQueueEvent)
        {
            ScanCallback = scanCallback;
            EmptyQueueThresholdEvent += emptyQueueEvent;
        }

        public void Push(IDevice device)
        {
            CallbackQueue.Enqueue(device);
        }

        public void Start()
        {
            CallbackQueueCancellationTokenSource = new CancellationTokenSource();
            CallbackQueueTask = new Task(
                () => CallbackQueueAction(CallbackQueueCancellationTokenSource.Token),
                TaskCreationOptions.LongRunning
            );
            CallbackQueueTask.Start();
            EmptyQueueTimerReset();
        }

        public void Stop()
        {
            EmptyQueueTimerStop();
            CallbackQueueCancellationTokenSource.Cancel();
        }

        protected async void CallbackQueueAction(CancellationToken cancelToken)
        {
#if DEBUG
            Trace.Info("Scanner Callback Queue: Starting");
#endif
            while (!cancelToken.IsCancellationRequested)
            {
                if (!CallbackQueue.IsEmpty)
                {
                    IDevice device;
                    while (CallbackQueue.TryDequeue(out device))
                    {
                        ScanCallback.ScanCallback(device);
                    }
                    EmptyQueueTimerReset();
                }
                await Task.Delay(100);
            }
            FinalizeQueue();
#if DEBUG
            Trace.Info("Scanner Callback Queue: Stopping");
#endif
        }

        protected void FinalizeQueue()
        {
            IDevice device;
            while (!CallbackQueue.IsEmpty)
                while (CallbackQueue.TryDequeue(out device)) { }
        }

        protected void EmptyQueueTimerReset(int threshold = EmptyQueueThreshold)
        {
            EmptyQueueTimerStop();
            EmptyQueueTimer = new Bluetooth.Timer(_ => EmptyQueueTimeout(), null, threshold, 0);
        }

        protected void EmptyQueueTimerStop()
        {
            if (EmptyQueueTimer != null)
                EmptyQueueTimer.Cancel();
        }

        protected void EmptyQueueTimeout()
        {
            if (EmptyQueueThresholdEvent != null)
                EmptyQueueThresholdEvent.Invoke(null, null);
            EmptyQueueTimerReset(MaxEmptyQueueThreshold);
        }
    }
}
