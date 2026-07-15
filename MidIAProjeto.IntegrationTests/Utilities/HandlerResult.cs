using Newtonsoft.Json;

namespace MidIAProjeto.IntegrationTests.Utilities;

public class HandlerResult()
{
        [JsonProperty("success")]
        public required bool Success { get; init; }

        public required string Reply { get; init; }

        public HandlerResult(bool success, string reply) : this()
        {
        
            Success = success;
            Reply = reply;
        }
    
}
