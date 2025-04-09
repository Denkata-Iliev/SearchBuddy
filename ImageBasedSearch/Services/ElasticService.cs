using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ImageBasedSearch.Models;
using ImageBasedSearch.Services.Contracts;

namespace ImageBasedSearch.Services
{
	public class ElasticService : IElasticService
	{
		private readonly ElasticsearchClient _client;

		public ElasticService()
		{
			_client = GetElasticClient();
		}

		public async Task<List<ResponseDocument>> SearchSimilar(float[] queryVector, string indexName)
		{
			// if indexName is empty or null, we're trying to
			// search through all indices available (i.e. all albums in the site)
			var allIndices = string.IsNullOrEmpty(indexName) ? Indices.All : null;
			var esResponse = await _client.SearchAsync<ResponseDocument>(
				new SearchRequest(allIndices is not null ? allIndices : indexName)
				{
					Sort = [
						SortOptions.Score(
							new ScoreSort
							{
								Order = SortOrder.Asc
							}
						)
					],
					Size = 50,
					Query = Query.ScriptScore(new ScriptScoreQuery
					{
						Query = Query.MatchAll(new MatchAllQuery()),
						Script = new Script
						{
							Source = "l2norm(params.query_vector, 'Embeddings')",
							Params = new Dictionary<string, object>
							{
								{ "query_vector", queryVector }
							}
						}
					}),
					Source = new SourceConfig(
						new SourceFilter
						{
							Excludes = Field.FromString(nameof(ImageDocument.Embeddings))
						}
					)
				}
			);

			var elements = esResponse.Hits
				.Where(h => h.Score < 0.8)
				.Select(h => new ResponseDocument
				{
					ImagePath = h.Source?.ImagePath,
					Score = h.Score,
					Id = h.Id
				}
				).ToList();

			return elements;
		}

		public async Task InitIndex(string indexName)
		{
			var response = await _client.Indices.GetAsync(indexName);

			if (response.IsValidResponse)
			{
				return;
			}

			await CreateIndex(indexName);
		}

		public async Task InsertBulk(IEnumerable<ImageDocument> imageDocuments, string indexName)
		{
			await _client.IndexManyAsync(imageDocuments, indexName);
		}

		public async Task DeleteIndex(string indexName)
		{
			await _client.Indices.DeleteAsync(indexName);
		}

		private async Task CreateIndex(string indexName)
		{
			await _client.Indices.CreateAsync(indexName, c => c
				.Mappings(m => m
					.Properties<IndexMapping>(p => p
						.DenseVector(nameof(IndexMapping.Embeddings), v => v.Dims(512))
						.Keyword(nameof(IndexMapping.ImagePath))
					)
				)
			);
		}

		private ElasticsearchClient GetElasticClient()
		{
			var settings = new ElasticsearchClientSettings()
				.EnableDebugMode()
				.PrettyJson()
				.RequestTimeout(TimeSpan.FromMinutes(2));

			return new ElasticsearchClient(settings);
		}
	}
}
