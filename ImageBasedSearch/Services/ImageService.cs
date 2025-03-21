using ImageBasedSearch.Models;
using Microsoft.ML;
using NuGet.Packaging;

namespace ImageBasedSearch.Services
{
	public class ImageService
	{
		private readonly OnnxModelScorer _scorer;

		public ImageService()
		{
			_scorer = new OnnxModelScorer(new MLContext());
		}

		public async Task<string> DownloadImageFromUrl(string url)
		{
			var originalExtension = Path.GetExtension(url);

			using var client = new HttpClient();
			using var stream = await client.GetStreamAsync(url);

			var randomFileName = Path.GetRandomFileName();
			var fileNameOriginalExtension = Path.ChangeExtension(randomFileName, originalExtension);
			var fullFilePath = Path.Combine(Constants.ImagesFolder, fileNameOriginalExtension);

			stream.CopyToFile(fullFilePath);

			return fullFilePath;
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

		public List<ImageDocument> GetImageDocuments()
		{
			List<ImageDocument> imageDocs = [];
			var images = Directory.GetFiles(Constants.ImagesFolder, string.Empty, SearchOption.AllDirectories);
			var imageInputs = images.Select(p => new ImageInput { ImagePath = p }).ToList();

			float[][] embeddings = _scorer.GetAllImageFeatures(imageInputs);

			foreach (var (path, embed) in images.Zip(embeddings))
			{
				imageDocs.Add(new ImageDocument
				{
					ImagePath = path,
					Embeddings = embed
				});
			}

			return imageDocs;
		}
	}
}
