using ImageBasedSearch.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Runtime;
using Microsoft.ML.Transforms;

namespace ImageBasedSearch.Services
{
	public class OnnxModelScorer
	{
		private readonly MLContext _mlContext;

		public OnnxModelScorer(MLContext mlContext)
		{
			_mlContext = mlContext;
		}
		
		public float[][] GetAllImageFeatures(List<ImageInput> imageInputs)
		{
			var data = _mlContext.Data.LoadFromEnumerable(imageInputs);

			var normalizedImageFeatures = GetNormalizedImageFeatures(data);

			return normalizedImageFeatures;
		}

		public float[] GetOneImageFeatures(string imagePath)
		{
			var data = _mlContext.Data.LoadFromEnumerable([
				new ImageInput 
				{
					ImagePath = imagePath
				}	
			]);

			var normalizedImageFeatures = GetNormalizedImageFeatures(data);

			//var normalizedFeatures = featurizedImageColumnsPerRow.Select(v => L2Normalize(NormalizeVector(v))).ToArray();

			return normalizedImageFeatures[0];
		}

		private float[][] GetNormalizedImageFeatures(IDataView data)
		{
			var pipeline = GetPipeline(_mlContext);

			var transformedData = pipeline.Fit(data).Transform(data);

			var imageFeatures = transformedData
				.GetColumn<float[]>(Constants.FeaturizedImageColumn)
				.ToArray();

			var normalizedImageFeatures = imageFeatures.Select(NormalizeVector).ToArray();

			return normalizedImageFeatures;
		}

		private static float[] NormalizeVector(float[] vector)
		{
			float norm = (float)Math.Sqrt(vector.Sum(v => v * v));
			return vector.Select(v => v / norm).ToArray();
		}

		private EstimatorChain<TransformerChain<ColumnCopyingTransformer>> GetPipeline(MLContext mlContext)
		{
			// Installing the Microsoft.ML.DNNImageFeaturizer packages copies the models in the
			// `DnnImageModels` folder.
			var pipeline = mlContext.Transforms.LoadImages(
				outputColumnName: Constants.ImageObjectColumn,
				imageFolder: string.Empty, // empty, because we pass the whole path to the image
				inputColumnName: Constants.InputColumn
			)
			.Append(
				mlContext.Transforms.ResizeImages(
					outputColumnName: Constants.ImageObjectColumn,
					imageWidth: Constants.ImageWidth,
					imageHeight: Constants.ImageHeight
				)
			)
			.Append(
				mlContext.Transforms.ExtractPixels(
					outputColumnName: Constants.PixelsColumn,
					inputColumnName: Constants.ImageObjectColumn
				)
			)
			.Append(
				mlContext.Transforms.DnnFeaturizeImage(
					outputColumnName: Constants.FeaturizedImageColumn,
					m => m.ModelSelector.ResNet18(
						mlContext,
						m.OutputColumn,
						m.InputColumn
					),
					inputColumnName: Constants.PixelsColumn
				)
			);

			return pipeline;
		}

		/*public float[] GetImageEmbedding(string imagePath)
		{
			//var data = _mlContext.Data.LoadFromEnumerable(new List<ImageInput>());

			//var testPipeline = _mlContext.Transforms.LoadImages(Constants.OutputColumn, @"wwwroot/images", Constants.InputColumn)
			//	.Append(_mlContext.Transforms.ResizeImages(Constants.OutputColumn, Constants.ImageWidth, Constants.ImageHeight, Constants.InputColumn))
			//	.Append(_mlContext.Transforms.ExtractPixels(Constants.OutputColumn, Constants.InputColumn))
			//	.Append(_mlContext.Transforms.ApplyOnnxModel(modelFile: _modelLocation));

			var data = _mlContext.Data.LoadFromEnumerable([new ImageInput { ImagePath = imagePath }]);

			var testPipeline = _mlContext.Transforms.LoadImages(
				outputColumnName: Constants.InputColumn, // Must match ONNX input column
				imageFolder: string.Empty, // Use empty since full path is provided
				inputColumnName: Constants.InputColumn
			)
			.Append(_mlContext.Transforms.ResizeImages(
					outputColumnName: Constants.InputColumn, // ONNX expects input as "input"
					imageWidth: Constants.ImageWidth,
					imageHeight: Constants.ImageHeight
				)
			)
			//.Append(_mlContext.Transforms.ConvertToGrayscale(
			//		outputColumnName: Constants.InputColumn
			//	)
			//)
			.Append(_mlContext.Transforms.ExtractPixels(
					outputColumnName: Constants.InputColumn,
					scaleImage: 1.0f / 255.0f, // Normalize pixel values to [0,1]
					interleavePixelColors: true // Ensure correct channel order (RGB)
				)
			) // Extracts pixel values as floats
			.Append(_mlContext.Transforms.ApplyOnnxModel(
					modelFile: _modelLocation,
					outputColumnName: Constants.OutputColumn, // Model's output tensor
					inputColumnName: Constants.InputColumn
				)
			);

			var modelTest = testPipeline.Fit(data);
			var predictionEngine = _mlContext.Model.CreatePredictionEngine<ImageInput, ImageEmbedding>(modelTest);
			//var result = predictionEngine.Predict(new ImageInput { ImagePath = Image.FromFile(imagePath) });
			var result = predictionEngine.Predict(new ImageInput { ImagePath = imagePath });

			//var imageData = _mlContext.Data.CreateEnumerable<ImageInput>(data, false).First();
			//using (var grayscale = new Bitmap(imageData.ImagePath))
			//{
			//	grayscale.Save("grayscaled.png", ImageFormat.Png);
			//}

			return result.Embedding;
		}

		public float[][] BatchVectorize(List<string> imagePaths)
		{
			var imageInputs = imagePaths.Select(path => new ImageInput { ImagePath = path }).ToList();

			// Load images into IDataView
			var data = _mlContext.Data.LoadFromEnumerable(imageInputs);

			var testPipeline = _mlContext.Transforms.LoadImages(
				outputColumnName: Constants.InputColumn, // Must match ONNX input column
				imageFolder: string.Empty, // Use empty since full path is provided
				inputColumnName: Constants.InputColumn
			)
			.Append(_mlContext.Transforms.ResizeImages(
					outputColumnName: Constants.InputColumn, // ONNX expects input as "input"
					imageWidth: Constants.ImageWidth,
					imageHeight: Constants.ImageHeight
				)
			)
			//.Append(_mlContext.Transforms.ConvertToGrayscale(
			//		outputColumnName: Constants.InputColumn
			//	)
			//)
			.Append(_mlContext.Transforms.ExtractPixels(
					outputColumnName: Constants.InputColumn,
					scaleImage: 1.0f / 255.0f, // Normalize pixel values to [0,1]
					interleavePixelColors: true // Ensure correct channel order (RGB)
				)
			) // Extracts pixel values as floats
			.Append(_mlContext.Transforms.ApplyOnnxModel(
					modelFile: _modelLocation,
					outputColumnName: Constants.OutputColumn, // Model's output tensor
					inputColumnName: Constants.InputColumn
				)
			);

			var modelTest = testPipeline.Fit(data);

			// Transform the data in batch
			IDataView transformedData = modelTest.Transform(data);

			// Extract embeddings
			var embeddings = transformedData.GetColumn<float[]>("output").ToArray();

			return embeddings;
		}*/
	}
}
