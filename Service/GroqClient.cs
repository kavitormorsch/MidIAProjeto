using System.Text.Json.Nodes;
using GroqApiLibrary;


namespace MidIAProjeto.Service;

public class GroqClient
{
    private readonly GroqApiClient _groqClient;
    
    public GroqClient(GroqApiClient client)
        {
            _groqClient = client;
        }

    public async Task<string> GetRecommendationsAsync(string userInput)
    {
        var request = new JsonObject
        {
            ["model"] = "llama-3.3-70b-versatile",
            ["messages"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "system",
                    ["content"] =
                        "You are a media recommendation AI. Output a list of digital media like games, movies, e-books, etc."
                },
                new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = userInput
                }
            }
        };

        var result = await _groqClient.CreateChatCompletionAsync(request);

        Console.WriteLine(result?["choices"]?[0]?["message"]?["content"]?.ToString());
        return result?["choices"]?[0]?["message"]?["content"]?.ToString() ?? string.Empty;
    }
}