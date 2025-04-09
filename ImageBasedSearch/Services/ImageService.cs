using ImageBasedSearch.Models;
using ImageBasedSearch.Services.Contracts;
using NuGet.Packaging;

namespace ImageBasedSearch.Services
{
	public class ImageService : IImageService
	{
		private readonly OnnxModelScorer _scorer;

		public ImageService()
		{
			_scorer = new OnnxModelScorer();
		}

		public async Task<string?> DownloadImageFromUrl(string url, string userAlbumFolder)
		{
			if (string.IsNullOrEmpty(url))
			{
				return null;
			}

			var originalExtension = Path.GetExtension(url);

			using var client = new HttpClient();
			using var stream = await client.GetStreamAsync(url);

			var randomFileName = Path.GetRandomFileName();
			var fileNameOriginalExtension = Path.ChangeExtension(randomFileName, originalExtension);
			var filePath = Path.Combine(Constants.ImagesFolder, userAlbumFolder, fileNameOriginalExtension);

			stream.CopyToFile(filePath);

			return filePath;
		}

		public float[] GetImageVectors(string imagePath)
		{
			return _scorer.GetOneImageFeatures(imagePath);
		}

		public ImageDocument GetImageDocument(string imagePath)
		{
			return new ImageDocument
			{
				ImagePath = imagePath,
				Embeddings = _scorer.GetOneImageFeatures(imagePath)
			};
		}

		public List<ImageDocument> GetImageDocuments(IEnumerable<string> imagePaths)
		{
			List<ImageDocument> imageDocs = [];
			var imageInputs = imagePaths.Select(p => new ImageInput { ImagePath = p }).ToList();

			float[][] embeddings = _scorer.GetAllImageFeatures(imageInputs);

			foreach (var (path, embed) in imagePaths.Zip(embeddings))
			{
				imageDocs.Add(new ImageDocument
				{
					ImagePath = path,
					Embeddings = embed
				});
			}

			return imageDocs;
		}

		public async Task<string?> GetImagePathFromFormFile(IFormFile formFile, string? userAlbumFolder = null)
		{
			if (formFile.Length < 0 || formFile is null)
			{
				return null;
			}

			var fileExtension = Path.GetExtension(formFile.FileName);
			var fileName = Path.ChangeExtension(Path.GetRandomFileName(), fileExtension);

			var filePath = Path.Combine(
				Constants.ImagesFolder,
				userAlbumFolder is null ? string.Empty : userAlbumFolder,
				fileName
			);

			using (var stream = File.Create(filePath))
			{
				await formFile.CopyToAsync(stream);
			}

			return filePath;
		}
	}
}
