using ImageBasedSearch.Database;
using ImageBasedSearch.Services.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ImageBasedSearch.Controllers
{
    public class UsersController : Controller
    {
		private readonly IApiKeyService _apiKeyService;
		private readonly UserManager<User> _userManager;
		private readonly SearchBuddyContext _context;

		public UsersController(IApiKeyService apiKeyService, SearchBuddyContext context, UserManager<User> userManager)
		{
			_apiKeyService = apiKeyService;
			_context = context;
			_userManager = userManager;
		}

		public async Task<IActionResult> ApiKey()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			return View();
		}
    }
}
