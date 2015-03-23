namespace Keiser.MvxPlugins.Bluetooth
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate void TimerCallback(object state);

    public sealed class Timer : CancellationTokenSource, IDisposable
    {
        public Timer(TimerCallback callback, object state, int dueTime, int period)
        {
            Task.Delay(dueTime, Token).ContinueWith(async (t, s) =>
            {
                var tuple = (Tuple<TimerCallback, object>)s;

                do
                {
                    if (IsCancellationRequested)
                        break;
#pragma warning disable 4014
                    Task.Run(() => tuple.Item1(tuple.Item2));
#pragma warning restore 4014
                    if (period > 0)
                        await Task.Delay(period);
                } while (period > 0);

            }, Tuple.Create(callback, state), CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
                TaskScheduler.Default);
        }

        public new void Dispose() { base.Cancel(); }
    }
}