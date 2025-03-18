using System.Text.Json.Serialization;

namespace ImageBasedSearch.Models
{
	public class ResponseDocument
	{
		[JsonPropertyName("ImagePath")]
		public string? ImagePath { get; set; } = string.Empty;

		[JsonPropertyName("_id")]
		public string? Id { get; set; } = string.Empty;

		[JsonPropertyName("_score")]
		public double? Score { get; set; }

		public string? ImageName
		{
			get
			{
				return ImagePath?.Substring(ImagePath.IndexOf('\\') + 1);
			}
		}
	}
}