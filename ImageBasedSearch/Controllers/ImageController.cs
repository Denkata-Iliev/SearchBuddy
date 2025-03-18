using ImageBasedSearch.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;

namespace ImageBasedSearch.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ImageController : ControllerBase
	{
		private readonly ImageService _imageService;
		private readonly ElasticService _elasticService;

		public ImageController()
		{
			_imageService = new ImageService();
			_elasticService = new ElasticService();
		}

		public async Task<IActionResult> InsertPictures()
		{
			await _elasticService.InitIndex();

			var imageDocs = _imageService.GetImageDocuments();

			await _elasticService.InsertBulk(imageDocs);

			return Ok();
		}

		[Route("img")]
		public IActionResult GetImageVectors([FromQuery] string imagePath)
		{
			return Ok(new { Vectors = _imageService.GetImageVectors(imagePath) });
		}
	}
}
