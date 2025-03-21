using ImageBasedSearch.Models;
using ImageBasedSearch.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ImageBasedSearch.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SitefinityController : ControllerBase
	{
		private readonly ElasticService _elasticService;
		private readonly ImageService _imageService;
		private readonly ApiKeyService _apiKeyService;
		public static List<User> USERS = new();

		/*
		TODO - make an actual db and everything
		TODO - scaffold identity for register and login
		TODO - make it so that you can revoke and create a new API key
		TODO - make a page where users can upload pictures and see them.
				for that, use the same naming convention (the one for index names) for folder names
		 */
		public SitefinityController()
		{
			_elasticService = new();
			_imageService = new();
			_apiKeyService = new();
		}

		[HttpPost("reg")]
		public async Task<IActionResult> FakeRegister([FromBody] RegisterDto registerDto)
		{
			var user = new User
			{
				Email = registerDto.Email,
				ApiKey = _apiKeyService.GenerateApiKey()
			};

			USERS.Add(user);

			await _elasticService.InitIndex(user.IndexName);

			return Ok();
		}

		[HttpPost("upload")]
		public async Task<IActionResult> Upload([FromBody] SitefinityResponse sitefinityResponse)
		{
			var currentUser = HttpContext.Items["current-user"] as User;
			if (currentUser is null)
			{
				return BadRequest();
			}

			var filePath = await _imageService.DownloadImageFromUrl(sitefinityResponse.Item.Url);

			var imageDoc = _imageService.GetImageDocument(filePath);

			await _elasticService.InsertBulk([imageDoc], currentUser.IndexName);

			return Ok();
		}
	}

	public class RegisterDto
	{
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}
}
