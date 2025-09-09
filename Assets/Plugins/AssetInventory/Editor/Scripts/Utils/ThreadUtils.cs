using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AssetInventory
{
    public static class ThreadUtils
    {
        private static SynchronizationContext _mainThreadContext;

        public static void Initialize()
        {
            if (_mainThreadContext == null)
            {
                _mainThreadContext = SynchronizationContext.Current;
            }
        }

        public static void InvokeOnMainThread(MethodInfo method, object target, object[] parameters)
        {
            if (_mainThreadContext == null)
            {
                throw new InvalidOperationException("MainThreadInvoker not initialized. Call Initialize() from the main thread.");
            }

            _mainThreadContext.Post(_ => method.Invoke(target, parameters), null);
        }

        public static async Task WithCancellation(this Task task, CancellationToken ct)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            using (ct.Register(s => ((TaskCompletionSource<object>)s).TrySetResult(null), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
                {
                    throw new OperationCanceledException(ct);
                }
            }
            await task.ConfigureAwait(false);
        }

        public static bool IsDisposed(this CancellationTokenSource cts)
        {
            if (cts == null) return true;
            try
            {
                _ = cts.Token;
                return false;
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
        }
    }
}