using System;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;

namespace SecureLearning.Client.Api
{
    /// <summary>
    /// Lets `await request.SendWebRequest()` be used directly instead of a coroutine —
    /// UnityWebRequestAsyncOperation has no built-in awaiter prior to Unity 2023.1.
    /// </summary>
    public static class UnityWebRequestAwaiterExtensions
    {
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new UnityWebRequestAwaiter(asyncOp);
        }
    }

    public readonly struct UnityWebRequestAwaiter : INotifyCompletion
    {
        private readonly UnityWebRequestAsyncOperation asyncOp;

        public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
        {
            this.asyncOp = asyncOp;
        }

        public bool IsCompleted => asyncOp.isDone;

        public void OnCompleted(Action continuation)
        {
            asyncOp.completed += _ => continuation();
        }

        public UnityWebRequest GetResult() => asyncOp.webRequest;
    }
}
