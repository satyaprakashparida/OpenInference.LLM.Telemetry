using System.Diagnostics;
using OpenTelemetry;

namespace OpenInference.LLM.Telemetry.Extensions.OpenTelemetry.Instrumentation
{
    /// <summary>
    /// Processor for LLM telemetry activities.
    /// </summary>
    internal class LlmTelemetryProcessor : BaseProcessor<Activity>
    {
        /// <summary>
        /// Called when an activity is started. Ensures all LLM activities have the proper semantic conventions.
        /// </summary>
        /// <param name="activity">The activity that started.</param>
        public override void OnStart(Activity activity)
        {
            if (activity == null) return;
            
            // Only process activities from our source
            if (!IsLlmActivity(activity)) return;
            
            // Ensure the activity has the proper OpenInference span kind if missing
            var spanKindTag = new KeyValuePair<string, object?>(Core.SemanticConventions.OPENINFERENCE_SPAN_KIND, Core.SemanticConventions.SpanKind.LLM);
            if (!activity.TagObjects.Contains(spanKindTag))
            {
                activity.SetTag(Core.SemanticConventions.OPENINFERENCE_SPAN_KIND, Core.SemanticConventions.SpanKind.LLM);
            }
            
            base.OnStart(activity);
        }

        /// <summary>
        /// Called when an activity ends. Adds additional semantic conventions if needed.
        /// </summary>
        /// <param name="activity">The activity that ended.</param>
        public override void OnEnd(Activity activity)
        {
            if (activity == null) return;
            
            // Only process activities from our source
            if (!IsLlmActivity(activity)) return;
            
            // Add any timestamps
            var timestampTag = new KeyValuePair<string, object?>("timestamp", DateTimeOffset.UtcNow);
            if (!activity.TagObjects.Contains(timestampTag))
            {
                activity.SetTag("timestamp", DateTimeOffset.UtcNow);
            }
            
            base.OnEnd(activity);
        }

        /// <summary>
        /// Determines if the activity is an LLM telemetry activity.
        /// </summary>
        /// <param name="activity">The activity to check.</param>
        /// <returns>True if the activity is from our LLM telemetry source, false otherwise.</returns>
        private static bool IsLlmActivity(Activity activity)
        {
            return activity.Source.Name == LlmTelemetry.ActivitySource.Name;
        }
    }
}
