using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using MidIAProjeto.Pages;
using Newtonsoft.Json;
using NuGet.Protocol;
using Xunit.Sdk;
using MidIAProjeto.IntegrationTests.Utilities;
using static MidIAProjeto.IntegrationTests.Utilities.HelperFunctions;

namespace MidIAProjeto.IntegrationTests;



public class AiChatPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AiChatPageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;

        _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.Configure<AuthenticationOptions>(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestScheme";
                        options.DefaultChallengeScheme = "TestScheme";
                    });

                    services.AddAuthentication(IdentityConstants.ApplicationScheme)
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme",
                            options => { });

                    services.AddRazorPages(options =>
                    {
                        options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
                    });


                });
            })
            .CreateClient();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");

    }

    [Fact]
    public async Task SaveList_WithCorrectBodyData_ShouldReturnTrue()
    {
        await SetupTestUser(_factory);

        var content = new FormUrlEncodedContent(
            new[]
            {
                new KeyValuePair<string, string>("UserInput.ListToSave", "this is definitely a list."),
                new KeyValuePair<string, string>("UserInput.UserSavePrompt", "this is definitely a prompt."),
            });

        HttpResponseMessage httpResult = await _client.PostAsync("/AiChat?handler=Save", content);

        httpResult.EnsureSuccessStatusCode();

        var result = await ConvertResponse(httpResult);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task SaveList_WithEmptyBodyData_ShouldReturnFalse()
    {
        await SetupTestUser(_factory);

        HttpResponseMessage httpResult = await _client.PostAsync("/AiChat?handler=Save", null);

        httpResult.EnsureSuccessStatusCode();

        var result = await ConvertResponse(httpResult);


        Assert.False(result.Success);
        Assert.IsType<string>(result.Reply).Contains("A mensagem não pode estar vazia.");
    }

    [Fact]
    public async Task GenerateList_WithCorrectBodyData_ShouldReturnTrue()
    {
        var content = new FormUrlEncodedContent(
            new[]
            {
                new KeyValuePair<string, string>("UserInput.Message", "this is definitely a prompt."),
            });

        HttpResponseMessage httpResult = await _client.PostAsync("/AiChat?handler=Chat", content);

        httpResult.EnsureSuccessStatusCode();

        var result = await ConvertResponse(httpResult);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task GenerateList_WithEmptyBodyData_ShouldReturnFalse()
    {
        HttpResponseMessage httpResult = await _client.PostAsync("/AiChat?handler=Chat", null);

        httpResult.EnsureSuccessStatusCode();

        var result = await ConvertResponse(httpResult);
        Assert.False(result.Success);
        Assert.IsType<string>(result.Reply).Contains("A mensagem não pode estar vazia.");
    }
}
    
    
    



