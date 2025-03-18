using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Nodes;
using ImageBasedSearch.Models;
using ImageBasedSearch.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace ImageBasedSearch.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TestController : ControllerBase
	{
		private readonly ElasticService _elastic;
		private readonly ImageService _image;
		public TestController()
		{
			_elastic = new();
			_image = new();
		}

		[HttpPost]
		public async Task<IActionResult> Search([FromForm] IFormFile formFile)
		{
			if (formFile.Length < 0 || formFile is null)
			{
				return BadRequest();
			}

			var fileExtension = Path.GetExtension(formFile.FileName);
			var fileName = Path.ChangeExtension(Path.GetRandomFileName(), fileExtension);

			var filePath = Path.Combine(@"wwwroot\images\mine_small", fileName);

			using (var stream = System.IO.File.Create(filePath))
			{
				await formFile.CopyToAsync(stream);
			}

			var imageVector = _image.GetImageVectors(filePath);
			System.IO.File.Delete(filePath);
			var resp = await _elastic.SearchSimilar(imageVector);

			return Ok(resp);
		}
	}
}
