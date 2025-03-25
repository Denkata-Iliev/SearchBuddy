using AspNetCoreGeneratedDocument;
using ImageBasedSearch.Database;
using ImageBasedSearch.Services;
using ImageBasedSearch.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ImageBasedSearch.Controllers
{
	[Authorize]
	public class UsersController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly SearchBuddyContext _context;
		private readonly IImageService _imageService;
		private readonly IElasticService _elasticService;

		public UsersController(SearchBuddyContext context, UserManager<User> userManager, IImageService imageService, IElasticService elasticService)
		{
			_context = context;
			_userManager = userManager;
			_imageService = imageService;
			_elasticService = elasticService;
		}

		public async Task<IActionResult> Album()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			List<string> imagesUrls = [];
			var imagesFullPath = Directory.EnumerateFiles(Path.Combine(Constants.ImagesFolder, user.IndexName));
			foreach (var item in imagesFullPath)
			{
				imagesUrls.Add(Path.Combine("images", user.IndexName, Path.GetFileName(item)));
			}

			return View(imagesUrls);
		}

		[HttpPost]
		public async Task<IActionResult> Album([FromForm] List<IFormFile> images)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			if (images.IsNullOrEmpty())
			{
				return BadRequest();
			}

			var userAlbumFolder = Path.Combine(Constants.ImagesFolder, user.IndexName);
			if (!Path.Exists(userAlbumFolder))
			{
				Directory.CreateDirectory(userAlbumFolder);
			}

			List<string> imagePaths = [];
			foreach (var image in images)
			{
				if (image.Length < 0 || image is null)
				{
					return BadRequest();
				}

				var fileExtension = Path.GetExtension(image.FileName);
				var fileName = Path.ChangeExtension(Path.GetRandomFileName(), fileExtension);

				var filePath = Path.Combine(userAlbumFolder, fileName);

				using var stream = System.IO.File.Create(filePath);
				await image.CopyToAsync(stream);

				imagePaths.Add(filePath);
			}

			var imageDocs = _imageService.GetImageDocuments(imagePaths);
			await _elasticService.InsertBulk(imageDocs, user.IndexName);
			
			return RedirectToAction(nameof(Album));
		}
	}
}
