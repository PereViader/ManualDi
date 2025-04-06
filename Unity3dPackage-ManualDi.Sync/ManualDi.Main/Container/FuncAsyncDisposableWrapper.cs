using System;
using System.Threading.Tasks;

namespace ManualDi.Main
{
    internal sealed class FuncAsyncDisposableWrapper : IAsyncDisposable
    {
        private Func<ValueTask>? func;

        public FuncAsyncDisposableWrapper(Func<ValueTask>? func)
        {
            this.func = func;
        }

        public async ValueTask DisposeAsync()
        {
            if (func is null)
            {
                return;
            }
            
            await func.Invoke();
            func = null;
        }
    }
}