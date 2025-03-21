using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ImageBasedSearch.Models;
using Microsoft.AspNetCore.Http;
using ImageBasedSearch.Services;
using NuGet.Packaging;

namespace ImageBasedSearch.Controllers;

public class HomeController : Controller
{
	private readonly ILogger<HomeController> _logger;
	private readonly ElasticService _elasticService;
	private readonly ImageService _imageService;

	public HomeController(ILogger<HomeController> logger)
	{
		_logger = logger;
		_elasticService = new();
		_imageService = new();
	}

	[HttpPost("down")]
	public async Task<IActionResult> Download([FromBody] SitefinityResponse sitefinityResponse)
	{
		var originalExtension = Path.GetExtension(sitefinityResponse.Item.Url);

		using var client = new HttpClient();
		using var stream = await client.GetStreamAsync(sitefinityResponse.Item.Url);

		var randomFileName = Path.GetRandomFileName();
		var fileNameOriginalExtension = Path.ChangeExtension(randomFileName, originalExtension);
		var fullFilePath = Path.Combine(Constants.ImagesFolder, fileNameOriginalExtension);
		
		stream.CopyToFile(fullFilePath);

		var imageDoc = _imageService.GetImageDocument(fullFilePath);
		//await _elasticService.InsertBulk([imageDoc]);

		return Ok(fullFilePath);
	}

	public IActionResult Index()
	{
		return View();
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

		var imageVector = _imageService.GetImageVectors(filePath);
		System.IO.File.Delete(filePath);
		var resp = await _elasticService.SearchSimilar(imageVector);
		return View(nameof(SearchResult), resp);
	}

	public async Task<IActionResult> SearchWithPath([FromQuery] string filePath)
	{
		var imageVector = _imageService.GetImageVectors(filePath);
		var resp = await _elasticService.SearchSimilar(imageVector);
		return View(nameof(SearchResult), resp);
	}

	public IActionResult SearchResult()
	{
		return View();
	}

	public IActionResult Privacy()
	{
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}
