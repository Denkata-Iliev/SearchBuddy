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

		public OnnxModelScorer()
		{
			_mlContext = new();
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
	}
}
