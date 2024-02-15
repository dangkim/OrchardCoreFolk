using NSwag.Annotations;

namespace OrchardCore.SimService.RedocAttributeProcessors
{
    public class ReDocCodeSampleAttribute : OpenApiOperationProcessorAttribute
    {
        public ReDocCodeSampleAttribute(string language, string source)
            : base(typeof(ReDocCodeSampleAppender), language, source)
        {
        }
    }
}

