using ImageBasedSearch.Models;
using Microsoft.ML;

namespace ImageBasedSearch.Services
{
	public class ImageService
	{
		private readonly OnnxModelScorer _scorer;

		public ImageService()
		{
			_scorer = new OnnxModelScorer(new MLContext());
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
