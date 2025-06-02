using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ManualDi.Async.Unity3d.Tests.PlayMode
{
    internal static class TestExtensions
    {
        public static IEnumerator Async(this Func<CancellationToken, Task> task)
        {
            return task.Invoke(Application.exitCancellationToken).ToCoroutine();
        }
        
        private static IEnumerator ToCoroutine(this Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }
            
            if (task.Exception is not null)
            {
                throw task.Exception.InnerException!;
            }
        }
    }
}