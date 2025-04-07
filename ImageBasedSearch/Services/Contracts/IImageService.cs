using ImageBasedSearch.Models;

namespace ImageBasedSearch.Services.Contracts
{
	public interface IImageService
	{
		/// <summary>
		/// Copies <paramref name="formFile"/> to wwwroot\images if <paramref name="userAlbumFolder"/> is null (default value) or to wwwroot\images\{<paramref name="userAlbumFolder"/>} if not.
		/// </summary>
		/// <param name="formFile">The form file to copy</param>
		/// <param name="userAlbumFolder">The folder to copy the file to (optional)</param>
		/// <returns>The relative file path of the copied file or <paramref name="null"/> if the file was invalid</returns>
		Task<string?> GetImagePathFromFormFile(IFormFile formFile, string? userAlbumFolder = null);

		/// <summary>
		/// Downloads the image from the specified <paramref name="url"/> to the specified <paramref name="userAlbumFolder"/>
		/// </summary>
		/// <param name="url">The URL of the image to download</param>
		/// <param name="userAlbumFolder">The folder where the images is supposed to be downloaded to</param>
		/// <returns>The relative path to the downloaded image, <paramref name="null"/> otherwise</returns>
		Task<string?> DownloadImageFromUrl(string url, string userAlbumFolder);

		float[] GetImageVectors(string imagePath);

		ImageDocument GetImageDocument(string imagePath);

		List<ImageDocument> GetImageDocuments(IEnumerable<string> imagePaths);
	}
}
