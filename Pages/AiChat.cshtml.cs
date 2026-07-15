using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MidIAProjeto.Data;
using MidIAProjeto.Service;
using Microsoft.EntityFrameworkCore;

namespace MidIAProjeto.Pages;

public class AiChat : PageModel
{
    private readonly GroqClient _aiService;
    
    private readonly ApplicationDbContext _dbContext;
    
    private UserManager<IdentityUser> _userManager;
    
    [BindProperty]
    public InputModel UserInput {get; set;}

    public string? AiResponse { get; set; } = "";

    public AiChat(GroqClient aiService, ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        _aiService = aiService;
        _dbContext = dbContext;
        _userManager = userManager;
    }
    
    public void OnGet()
    {
        
    }
    
    public async Task<IActionResult> OnPostChatAsync()
    {
        if (string.IsNullOrWhiteSpace(UserInput.Message))
            return new JsonResult(new { success = false, reply = "A mensagem não pode estar vazia." });
        
        AiResponse = await _aiService.GetRecommendationsAsync(UserInput.Message);
        
        return new JsonResult(new {success = true, reply = AiResponse});
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var result = String.Empty;
        if (string.IsNullOrWhiteSpace(UserInput.ListToSave) || string.IsNullOrWhiteSpace(UserInput.UserSavePrompt))
            return new JsonResult(new { success = false, reply = "A mensagem não pode estar vazia." });

        try
        {
            result = await SaveList(UserInput.ListToSave, UserInput.UserSavePrompt);
            Console.WriteLine(result);
        }
        catch (Exception e)
        {
            return new JsonResult(new { success = false, reply = e.Message });
        }
        
        return new JsonResult(new {success = true, reply = result });
    }
    
    public async Task<string> SaveList(string listReply, string userPromptToSave)
    {
        var usr = await _userManager.GetUserAsync(HttpContext.User);
        
        var list = new MediaList
        {
            GeneratedList = new ListDetails
            {
                GeneratedList = listReply,
                UserPrompt = userPromptToSave
            },
            UserId = usr.Id
        };
        
        _dbContext.Add(list);
        await _dbContext.SaveChangesAsync();
        return "Salvo com sucesso.";
    }
}




public partial class InputModel
{
    public string? Message { get; set; }
    
    public string? ListToSave {get; set;}
    public string? UserSavePrompt {get; set;}
    
}