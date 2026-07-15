using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MidIAProjeto.Data;

namespace MidIAProjeto.Areas.Identity.Pages.Account;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    
    private readonly UserManager<IdentityUser> _userManager;
    
    public List<ListViewModel> SavedUserLists {get; set;}

    public DashboardModel(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }
    
    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);

            SavedUserLists = await _dbContext.MediaList
                .Where(l => user.Id == l.UserId)
                .Select(l => new ListViewModel
                {
                    AiResponse = l.GeneratedList.GeneratedList,
                    UserPrompt = l.GeneratedList.UserPrompt,
                    Id = l.Id
                })
                .ToListAsync();
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


    public class DeleteRequest
    {
        public int ListId { get; set; }
    }
}
