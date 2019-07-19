using System;
using System.Threading.Tasks;

namespace F3N.YaMVVM.Core.Extensions
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }

    public static class TaskUtilities
    {
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public static async void FireAndForgetSafeAsync(this Task task, IErrorHandler handler = null)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                handler?.HandleError(ex);
            }
        }
    }

    public class DefaultErrorHandler : IErrorHandler
    {
        public void HandleError(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Recovered from crash: {0}", ex.Message);
        }
    }
}
