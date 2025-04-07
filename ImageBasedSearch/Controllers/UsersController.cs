using ImageBasedSearch.Database;
using ImageBasedSearch.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ImageBasedSearch.Controllers
{
	[Authorize]
	public class UsersController : Controller
	{
		private readonly UserManager<User> _userManager;
		private readonly IImageService _imageService;
		private readonly IElasticService _elasticService;

		public UsersController(UserManager<User> userManager,
						 IImageService imageService,
						 IElasticService elasticService)
		{
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
			var userAlbumFolder = Path.Combine(Constants.ImagesFolder, user.IndexName);
			var imagesFullPath = Directory.EnumerateFiles(userAlbumFolder);
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

			List<string> imagePaths = [];
			foreach (var image in images)
			{
				var filePath = await _imageService.GetImagePathFromFormFile(image, user.IndexName);
				if (filePath is null)
				{
					return BadRequest();
				}

				imagePaths.Add(filePath);
			}

			var imageDocs = _imageService.GetImageDocuments(imagePaths);
			await _elasticService.InsertBulk(imageDocs, user.IndexName);
			
			return RedirectToAction(nameof(Album));
		}

		[HttpPost]
		public async Task<IActionResult> Search([FromForm] IFormFile searchImage)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var filePath = await _imageService.GetImagePathFromFormFile(searchImage);
			if (filePath is null)
			{
				return BadRequest();
			}

			var imageVector = _imageService.GetImageVectors(filePath);
			System.IO.File.Delete(filePath);
			var resp = await _elasticService.SearchSimilar(imageVector, user.IndexName);
			return View(nameof(SearchResult), resp);
		}

		public async Task<IActionResult> SearchWithPath([FromQuery] string filePath)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var imageVector = _imageService.GetImageVectors(filePath);
			var resp = await _elasticService.SearchSimilar(imageVector, user.IndexName);
			return View(nameof(SearchResult), resp);
		}

		public IActionResult SearchResult()
		{
			return View();
		}
	}
}
