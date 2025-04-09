using ImageBasedSearch.Database;
using ImageBasedSearch.Models;
using ImageBasedSearch.Services;
using ImageBasedSearch.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ImageBasedSearch.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SitefinityController : ControllerBase
	{
		private readonly IElasticService _elasticService;
		private readonly IImageService _imageService;

		public SitefinityController(IElasticService elasticService, IImageService imageService)
		{
			_elasticService = elasticService;
			_imageService = imageService;
		}

		[HttpPost("upload")]
		public async Task<IActionResult> Upload([FromBody] SitefinityResponse sitefinityResponse)
		{
			var currentUser = HttpContext.Items["current-user"] as User;
			if (currentUser is null)
			{
				return BadRequest();
			}

			var filePath = await _imageService.DownloadImageFromUrl(sitefinityResponse.Item.Url, currentUser.IndexName);
			if (filePath is null)
			{
				return BadRequest();
			}

			var imageDoc = _imageService.GetImageDocument(filePath);

			await _elasticService.InsertBulk([imageDoc], currentUser.IndexName);

			return Ok();
		}
	}
}
