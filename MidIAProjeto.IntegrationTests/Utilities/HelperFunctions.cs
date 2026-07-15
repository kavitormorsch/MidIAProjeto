using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using MidIAProjeto.Data;
using Newtonsoft.Json;

namespace MidIAProjeto.IntegrationTests.Utilities;

public class HelperFunctions
{
    public static List<int> SeedDatabaseForTests(ApplicationDbContext db)
    {
        var testEntries = GetSeedingLists();
        
        db.MediaList.AddRange(testEntries);
        db.SaveChanges();
        
        return [testEntries[0].Id, testEntries[1].Id, testEntries[2].Id];
    }

    public static List<MediaList> GetSeedingLists()
    {
        return new List<MediaList>()
        {
            new MediaList()
            {
                GeneratedList = new ListDetails()
                {
                    GeneratedList = "this is definitely a list 1",
                    UserPrompt = "this is definitely a prompt 1"
                },
                UserId = "admin-test-id-123"
            },
            new MediaList()
            {
                GeneratedList = new ListDetails()
                {
                    GeneratedList = "this is definitely a list 2",
                    UserPrompt = "this is definitely a prompt 2"
                },
                UserId = "admin-test-id-123"
            },
            new MediaList()
            {
                GeneratedList = new ListDetails()
                {
                    GeneratedList = "this is definitely a list 3",
                    UserPrompt = "this is definitely a prompt 3"
                },
                UserId = "admin-test-id-123"
            }
        };
    }

    public static async Task SetupTestUser(WebApplicationFactory<Program> factory)
    {
        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            if (await userManager.FindByIdAsync(TestAuthHandler.TestUserId) == null)
            {
                var testUser = new IdentityUser
                {
                    Id = TestAuthHandler.TestUserId,
                    UserName = "admin@midiaAi.com",
                    Email = "admin@midiaAi.com"
                };
                await userManager.CreateAsync(testUser, "12345Ab@");
            }

        };
    }
    
    public static async Task<HandlerResult> ConvertResponse(HttpResponseMessage response)
    {
        var responseString = await response.Content.ReadAsStringAsync();
        var jsonConvertResult = JsonConvert.DeserializeObject<HandlerResult>(responseString);
        return jsonConvertResult;
    }
}
    

