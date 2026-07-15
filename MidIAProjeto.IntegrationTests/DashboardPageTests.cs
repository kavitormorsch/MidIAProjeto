using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using MidIAProjeto.Data;
using MidIAProjeto.Pages;
using Newtonsoft.Json;
using NuGet.Protocol;
using Xunit.Sdk;

using MidIAProjeto.IntegrationTests.Utilities;
using static MidIAProjeto.IntegrationTests.Utilities.HelperFunctions;

namespace MidIAProjeto.IntegrationTests;

public class DashboardPageTests : IClassFixture<WebApplicationFactory<Program>>
{
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        private List<int> DummyEntryId; 
        
        public DashboardPageTests(WebApplicationFactory<Program> factory)
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
                        
                        services.Configure<MvcOptions>(options =>
                        {
                            options.Filters.Add(new IgnoreAntiforgeryTokenAttribute { Order = int.MaxValue });
                        });

                        DummyEntryId = SeedDatabaseForTests(services.BuildServiceProvider().GetRequiredService<ApplicationDbContext>());
                    });
                })
                .CreateClient();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        }

        [Fact]
        public async Task GetPage_ShouldReturnOk()
        {
            await SetupTestUser(_factory);

            HttpResponseMessage response = await _client.GetAsync("/Identity/Account/Dashboard");
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        [Fact]
        public async Task DeleteList_WithValidId_ShouldReturnTrue()
        {
            await SetupTestUser(_factory);

            var content = new StringContent(DummyEntryId[0].ToString(), System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/Identity/Account/Dashboard?handler=Delete", content);
            
            response.EnsureSuccessStatusCode();
            
            var jsonResult = await ConvertResponse(response);
            
            Assert.True(jsonResult.Success);
            Assert.Contains("Lista foi deletada.", jsonResult.Reply);
        }
        
        [Fact]
        public async Task ViewList_WithValidId_ShouldReturnOk()
        {
            await SetupTestUser(_factory);
            
            HttpResponseMessage response = await _client.GetAsync("/Identity/Account/Dashboard/ListView?listid=" + DummyEntryId[1]);
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        [Fact]
        public async Task DeleteList_InListView_ShouldReturnTrue()
        {
            await SetupTestUser(_factory);
            
            var content = new StringContent(DummyEntryId[1].ToString(), System.Text.Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/Identity/Account/Dashboard/ListView?listid=" + DummyEntryId[1] + "&handler=Delete", content);
            
            response.EnsureSuccessStatusCode();
            
            var jsonResult = await ConvertResponse(response);
            
            Assert.True(jsonResult.Success);
            Assert.Contains("Lista foi deletada.", jsonResult.Reply);
        }
}