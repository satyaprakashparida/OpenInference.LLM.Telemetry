using System.Diagnostics;

namespace OpenInference.LLM.Telemetry.Core.Utilities
{
    /// <summary>
    /// Extension methods for Activity class.
    /// </summary>
    public static class ActivityExtensions
    {
        /// <summary>
        /// Records exception details in the activity.
        /// </summary>
        /// <param name="activity">The activity to record the exception in.</param>
        /// <param name="exception">The exception to record.</param>
        public static void RecordException(this Activity activity, Exception exception)
        {
            if (activity == null || exception == null)
                return;

            // Add standard exception attributes
            activity.SetTag("exception.type", exception.GetType().FullName);
            activity.SetTag("exception.message", exception.Message);
            activity.SetTag("exception.stacktrace", exception.StackTrace);

            // Add inner exception details if available
            if (exception.InnerException != null)
            {
                activity.SetTag("exception.inner.type", exception.InnerException.GetType().FullName);
                activity.SetTag("exception.inner.message", exception.InnerException.Message);
            }

            // Mark as error
            activity.SetTag("error", true);
        }
    }
}