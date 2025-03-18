using Microsoft.ML.Data;

namespace ImageBasedSearch.Models
{
	public class ImageInput
	{
		[ColumnName(Constants.InputColumn)]
		public string ImagePath { get; set; } = string.Empty;
	}
}
