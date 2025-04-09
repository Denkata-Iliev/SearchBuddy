using Elastic.Clients.Elasticsearch.Mapping;

namespace ImageBasedSearch.Models
{
	public class IndexMapping
	{
		public DenseVectorProperty Embeddings { get; set; } = 
			new DenseVectorProperty { Dims = Constants.VectorDimensions };

		public KeywordProperty ImagePath { get; set; } = new KeywordProperty();
	}
}
