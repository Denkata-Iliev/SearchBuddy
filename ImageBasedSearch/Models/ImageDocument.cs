using System.Text.Json.Serialization;

namespace ImageBasedSearch.Models
{
	public class ImageDocument
	{
		[JsonPropertyName("Embeddings")]
		public float[] Embeddings { get; set; } = new float[512];

		[JsonPropertyName("ImagePath")]
		public string ImagePath { get; set; } = string.Empty;
	}
}
