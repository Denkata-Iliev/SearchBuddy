namespace ImageBasedSearch
{
	public class Constants
	{
		//public const string OnnxModelPath = "onnx32-open_clip-ViT-B-32-openai-visual.onnx";
		public const string InputColumn = "input";
		//public const string OutputColumn = "output";
		public const int PixelCount = 224 * 224 * 3;
		public const int ImageWidth = 224;
		public const int ImageHeight = 224;
		public const int VectorDimensions = 512;
		//public const float ColorNormalizationFactor = 255f;
		public const string MyIndexName = "image-index";
		public const string ImagesFolder = @"wwwroot\images";
		public const string ImageObjectColumn = "ImageObject";
		public const string PixelsColumn = "Pixels";
		public const string FeaturizedImageColumn = "FeaturizedImage";
	}
}
