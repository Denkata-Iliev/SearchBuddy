using System.Text.Json.Serialization;

namespace ImageBasedSearch.Models
{
	public class ImageDocument
	{
		[JsonPropertyName(nameof(Embeddings))]
		public float[] Embeddings { get; set; } = new float[512];

		[JsonPropertyName(nameof(ImagePath))]
		public string ImagePath { get; set; } = string.Empty;
	}
}
