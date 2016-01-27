namespace Keiser.MvxPlugins.Bluetooth.Droid
{
    using Keiser.MvxPlugins.Bluetooth;
    using Keiser.MvxPlugins.Bluetooth.LE;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public class CallbackQueuer
    {
        protected IScanCallback ScanCallback;
        protected ConcurrentQueue<IDevice> CallbackQueue = new ConcurrentQueue<IDevice>();
        protected CancellationTokenSource CallbackQueueCancellationTokenSource;
        protected Task CallbackQueueTask;

        public CallbackQueuer()
        {
#if DEBUG
            Trace.Info("CallbackQueuer: Constructed");
#endif
        }

        public void Push(IDevice device)
        {
            CallbackQueue.Enqueue(device);
        }

        public void Start(IScanCallback scanCallback)
        {
            ScanCallback = scanCallback;
            CallbackQueueCancellationTokenSource = new CancellationTokenSource();
            CallbackQueueTask = new Task(
                () => CallbackQueueAction(CallbackQueueCancellationTokenSource.Token),
                TaskCreationOptions.LongRunning
            );
            CallbackQueueTask.Start();
        }

        public void Stop()
        {
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
#if DEBUG
                        Trace.Info("Device Found: " + device.Name + " " + device.ID.ColonSeperated);
#endif
                        ScanCallback.ScanCallback(device);
                    }
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
    }
}
