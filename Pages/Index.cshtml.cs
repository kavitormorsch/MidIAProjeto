using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MidIAProjeto.Pages;

public class IndexModel : PageModel
{ 
    public bool UserIsLoggedIn { get; set; }

    private readonly IAuthorizationService _authService;

    public IndexModel(IAuthorizationService authService)
    {
        _authService = authService;
    }
    
    public async Task<IActionResult> OnGetAsync()
    {
        if (User.Identity.IsAuthenticated)
            UserIsLoggedIn = true;
        else
            UserIsLoggedIn = false;
        
        return Page();
    }
}