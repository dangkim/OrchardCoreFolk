using System.Collections.Generic;
using Newtonsoft.Json;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace OrchardCore.SimService.RedocAttributeProcessors
{
    public class ReDocCodeSampleAppender : IOperationProcessor
    {
        private readonly string _language;
        private readonly string _source;
        private const string ExtensionKey = "x-code-samples";

        public ReDocCodeSampleAppender(string language, string source)
        {
            _language = language;
            _source = source;
        }

        public bool Process(OperationProcessorContext context)
        {
            if (context.OperationDescription.Operation.ExtensionData == null)
                context.OperationDescription.Operation.ExtensionData = new Dictionary<string, object>();

            var data = context.OperationDescription.Operation.ExtensionData;
            if (!data.ContainsKey(ExtensionKey))
                data[ExtensionKey] = new List<ReDocCodeSample>();

            var samples = (List<ReDocCodeSample>)data[ExtensionKey];
            samples.Add(new ReDocCodeSample
            {
                Language = _language,
                Source = _source,
            });

            return true;
        }
    }

    internal class ReDocCodeSample
    {
        [JsonProperty("lang")]
        public string Language { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }
    }
}

