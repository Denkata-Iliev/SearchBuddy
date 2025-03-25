using ImageBasedSearch.Models;

namespace ImageBasedSearch.Services.Contracts
{
	public interface IImageService
	{
		Task<string> DownloadImageFromUrl(string url);

		float[] GetImageVectors(string imagePath);

		ImageDocument GetImageDocument(string imagePath);

		List<ImageDocument> GetImageDocuments(IEnumerable<string> imagePaths);
	}
}
