using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MidIAProjeto.Data;

namespace MidIAProjeto.Areas.Identity.Pages.Account;

public class ListView : PageModel
{
    public ListViewModel UserList { get; set; }
    
    private readonly ApplicationDbContext _dbContext;
    
    private readonly UserManager<IdentityUser> _userManager;

    public ListView (ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }
    
    public async Task<IActionResult> OnGetAsync([FromQuery] [FromRoute] int listId)
    {
        if (listId == null)
            return RedirectToPage("/Error", new { ErrorHttpCode = 400, ErrorHttpMessage = "There was an error with your request." });
        
        var user = await _userManager.GetUserAsync(HttpContext.User);
        
        UserList = await _dbContext.MediaList
            .Where(l => user.Id == l.UserId && listId == l.Id)
            .Select(l => new ListViewModel
            {
                AiResponse = l.GeneratedList.GeneratedList,
                UserPrompt =  l.GeneratedList.UserPrompt,
                Id =  l.Id
            }).SingleOrDefaultAsync();
        if (UserList == null)
        {
            TempData["ErrorHttpCode"] = 401;
            TempData["ErrorMessage"] = "You are not authorized to view this resource.";
            
            return RedirectToPage("/Error", new { area = ""});
        }
            
        
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromBody] int listId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (listId <= 0)
            return new JsonResult(new { success = false, reply = "Erro ao pegar ID da lista." + listId });
        
        try
        {
            var list = await _dbContext.MediaList
                .Where(l => user.Id == l.UserId && listId  == l.Id).SingleOrDefaultAsync();
            
            if (list == null)
                return new JsonResult(new { success = false, reply = "Lista não encontrada." });
            
            _dbContext.MediaList.Remove(list);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return new JsonResult(new { success = false, reply = "Erro ao deletar a lista." });
        }
        
        return new JsonResult(new {success = true, reply = "Lista foi deletada." });
    }
}

