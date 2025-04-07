using ImageBasedSearch.Models;
using ImageBasedSearch.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ImageBasedSearch.Controllers;

public class HomeController : Controller
{
	private readonly ILogger<HomeController> _logger;
	private readonly IElasticService _elasticService;
	private readonly IImageService _imageService;

	public HomeController(ILogger<HomeController> logger,
					   IElasticService elasticService,
					   IImageService imageService)
	{
		_logger = logger;
		_imageService = imageService;
		_elasticService = elasticService;
	}

	public IActionResult Index()
	{
		return View();
	}

	[HttpPost]
	public async Task<IActionResult> Search([FromForm] IFormFile formFile)
	{
		var filePath = await _imageService.GetImagePathFromFormFile(formFile);
		if (filePath is null)
		{
			return BadRequest();
		}

		var imageVector = _imageService.GetImageVectors(filePath);
		System.IO.File.Delete(filePath);
		var resp = await _elasticService.SearchSimilar(imageVector, string.Empty);
		return View(nameof(SearchResult), resp);
	}

	public async Task<IActionResult> SearchWithPath([FromQuery] string filePath)
	{
		var imageVector = _imageService.GetImageVectors(filePath);
		var resp = await _elasticService.SearchSimilar(imageVector, string.Empty);
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
