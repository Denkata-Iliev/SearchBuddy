using ImageBasedSearch.Models;

namespace ImageBasedSearch.Services.Contracts
{
	public interface IElasticService
	{
		Task<List<ResponseDocument>> SearchSimilar(float[] queryVector, string indexName);

		Task InitIndex(string indexName);

		Task InsertBulk(IEnumerable<ImageDocument> imageDocuments, string indexName);

		Task DeleteIndex(string indexName);
	}
}
