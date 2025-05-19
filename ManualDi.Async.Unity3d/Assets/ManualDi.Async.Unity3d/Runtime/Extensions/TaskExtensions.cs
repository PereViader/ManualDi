using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

internal static class TaskExtensions {
    /// <summary>
    /// Allows awaiting on a Task with a cancellation token, enabling the task to be cancelled part way through.
    /// </summary>
    /// <typeparam name="T">The type of object that the Task is expected to return upon completion.</typeparam>
    /// <param name="task">The Task to await on.</param>
    /// <param name="cancellationToken">The cancellation token that will cancel the await if triggered.</param>
    /// <returns>The awaited Task if it completes before being cancelled.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the provided cancellationToken is cancelled before the task completes.</exception>
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken) {
        var tcs = new TaskCompletionSource<bool>();
        await using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs)) {
            if (task != await Task.WhenAny(task, tcs.Task))
                throw new OperationCanceledException(cancellationToken);
        }
        return await task;
    }
}