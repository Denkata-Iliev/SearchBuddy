using ImageBasedSearch.Database;
using ImageBasedSearch.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ImageBasedSearch.Areas.Identity.Pages.Account.Manage
{
    public class ApiKeyModel : PageModel
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly UserManager<User> _userManager;
        private readonly SearchBuddyContext _context;

        public ApiKeyModel(IApiKeyService apiKeyService, SearchBuddyContext context, UserManager<User> userManager)
        {
            _apiKeyService = apiKeyService;
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            ApiKey = user.ApiKey;

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            user.ApiKey = _apiKeyService.GenerateApiKey();
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            StatusMessage = "Successfully revoked old API Key and created a new one.";

            return RedirectToPage();
        }
    }
}
